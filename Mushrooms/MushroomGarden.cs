using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using DynamicData;
using DynamicData.Binding;
using HueLighting.Hub;
using HueLighting.HubSelection;
using HueLighting.Models;
using Microsoft.Extensions.Hosting;
using Mushrooms.Database;
using Mushrooms.Entities;
using Mushrooms.Support;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms {
    internal interface IMushroomGarden : IDisposable {
        Task    StartScene( Scene forScene );
        Task    StopScene( Scene forScene );

        Task    UpdateSceneControl( Scene forScene, SceneControl control );
        	
        IObservable<IChangeSet<ActiveScene>>    ActiveScenes { get; }
    }

    internal enum SceneState {
        Active,
        Scheduled,
        Inactive
    }

    internal class ActiveScene {
        private Subject<ActiveScene>        mChangeSubject;

        public  Scene                       Scene { get; }
        public  IList<ActiveBulb>           ActiveBulbs { get; }

        public  SceneState                  SceneState { get; private set; }
        public  bool                        IsActive { get; private set; }
        public  SceneControl                Control { get; private set; }

        public  IObservable<ActiveScene>    OnSceneChanged => mChangeSubject.AsObservable();

        public ActiveScene( Scene scene ) {
            Scene = scene;
            ActiveBulbs = new List<ActiveBulb>();
            Control = new SceneControl( Scene.Control.Brightness, Scene.Control.RateMultiplier );
            SceneState = SceneState.Inactive;
            IsActive = false;

            mChangeSubject = new Subject<ActiveScene>();
        }

        public void Activate() {
            IsActive = true;
            SceneState = SceneState.Active;

            mChangeSubject.OnNext( this );
        }

        public void ActiveBySchedule() {
            IsActive = true;
            SceneState = SceneState.Scheduled;

            mChangeSubject.OnNext( this );
        }

        public void Deactivate() {
            IsActive = false;
            SceneState = SceneState.Inactive;

            mChangeSubject.OnNext( this );
        }

        public void UpdateControl( SceneControl control ) {
            Control = control;

            mChangeSubject.OnNext( this );
        }

        public void UpdateActiveBulb( ActiveBulb bulb ) {
            var existing = ActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Bulb.Id ));

            if( existing != null ) {
                ActiveBulbs.Remove( existing );
            }

            ActiveBulbs.Add( bulb );

            mChangeSubject.OnNext( this );
        }
    }

    internal class ActiveBulb {
        public  Bulb        Bulb { get; }
        public  Color       ActiveColor { get; }
        public  DateTime    NextUpdateTime { get; }

        public ActiveBulb( Bulb bulb ) {
            ActiveColor = Colors.Transparent;
            Bulb = bulb;

            NextUpdateTime = DateTime.MinValue;
        }

        public ActiveBulb( Bulb bulb, Color color, DateTime nextUpdateTime ) {
            Bulb = bulb;
            ActiveColor = color;
            NextUpdateTime = nextUpdateTime;
        }
    }

    internal class MushroomGarden : BackgroundService, IMushroomGarden {
        private readonly IHubManager                                mHubManager;
        private readonly ISceneProvider                             mSceneProvider;
        private readonly IDialogService                             mDialogService;
        private readonly ObservableCollectionExtended<Scene>        mScenes;
        private readonly ObservableCollectionExtended<ActiveScene>  mActiveScenes;
        private readonly CancellationTokenSource                    mTokenSource;
        private readonly LimitedRepeatingRandom                     mLimitedRandom;
        private readonly Random                                     mRandom;
        private IDisposable ?                                       mSceneSubscription;
        private IDisposable ?                                       mActiveSceneSubscription;
        private Task ?                                              mLightingTask;
        private Task ?                                              mSchedulingTask;

        public  IObservable<IChangeSet<ActiveScene>>                ActiveScenes => mActiveScenes.ToObservableChangeSet();

        public MushroomGarden( IHubManager hubManager, IDialogService dialogService, ISceneProvider sceneProvider ) {
            mHubManager = hubManager;
            mSceneProvider = sceneProvider;
            mDialogService = dialogService;

            mActiveScenes = new ObservableCollectionExtended<ActiveScene>();
            mScenes = new ObservableCollectionExtended<Scene>();
            mTokenSource = new CancellationTokenSource();
            mLimitedRandom = new LimitedRepeatingRandom();
            mRandom = Random.Shared;
        }

        public async Task StartScene( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                await ActivateScene( scene.Scene );

                scene.Activate();
            }
        }

        private async Task ActivateScene( Scene scene ) {
            foreach( var bulb in scene.Bulbs ) {
                await mHubManager.SetBulbState( bulb, true );
                await mHubManager.SetBulbState( bulb, scene.Control.Brightness );
            }
        }

        public async Task StopScene( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                await DeactivateScene( scene.Scene );

                scene.Deactivate();
            }
        }

        public async Task UpdateSceneControl( Scene forScene, SceneControl control ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                scene.UpdateControl( control );

                await UpdateSceneControl( scene );
            }
        }

        private async Task UpdateSceneControl( ActiveScene scene ) {
            foreach ( var bulb in scene.Scene.Bulbs ) {
                await mHubManager.SetBulbState( bulb, scene.Control.Brightness );
            }
        }

        private async Task DeactivateScene( Scene scene ) {
            foreach( var bulb in scene.Bulbs ) {
                await mHubManager.SetBulbState( bulb, false );
            }
        }

        public override async Task StartAsync( CancellationToken cancellationToken ) {
            if(! await mHubManager.InitializeConfiguredHub()) {
                mDialogService.ShowDialog<HubSelectionView>();
            }

            await base.StartAsync( cancellationToken );
        }

        protected override Task ExecuteAsync( CancellationToken stoppingToken ) {
            mSceneSubscription = mSceneProvider.Entities
                .Connect()
                .Bind( mScenes )
                .Subscribe();

            mActiveSceneSubscription = mSceneProvider.Entities
                .Connect()
                .Transform( s => new ActiveScene( s ))
                .Bind( mActiveScenes )
                .Subscribe();

            mLightingTask = Repeat.Interval( TimeSpan.FromMilliseconds( 200 ), LightingTask, mTokenSource.Token );
            mSchedulingTask = Repeat.Interval( TimeSpan.FromSeconds( 31 ), SchedulingTask, mTokenSource.Token );

            return Task.CompletedTask;
        }

        private void LightingTask() {
            foreach( var scene in mActiveScenes.Where( s => s.IsActive )) {
                var updateList = BuildBulbList( scene );

                if( updateList.Any()) {
                    scene.UpdateActiveBulb( UpdateBulb( updateList.First(), scene.Scene, scene.Control ));
                }
            }
        }

        private async void SchedulingTask() {
            foreach( var scene in mActiveScenes.Where( s => s.Scene.Schedule.Enabled )) {
                if(!scene.IsActive ) {
                    await ActivateIfScheduleStart( scene );
                }
            }

            foreach( var scene in mActiveScenes.Where( s => s.SceneState.Equals( SceneState.Scheduled ))) {
                await DeactivateIfScheduleEnd( scene );
            }
        }

        private async Task ActivateIfScheduleStart( ActiveScene scene ) {
            var startTime = scene.Scene.Schedule.StartTimeForToday();
            var now = DateTime.Now;

            if(( now > startTime ) &&
               ( now < startTime.AddMinutes( 2 ))) {
                await ActivateScene( scene.Scene );

                scene.ActiveBySchedule();
            }
        }

        private async Task DeactivateIfScheduleEnd( ActiveScene scene ) {
            var stopTime = scene.Scene.Schedule.StopTimeForToday();

            if( DateTime.Now > stopTime ) {
                await DeactivateScene( scene.Scene );

                scene.Deactivate();
            }
        }

        private IList<ActiveBulb> BuildBulbList( ActiveScene forScene ) {
            var activity = new List<ActiveBulb>();

            foreach( var bulb in forScene.Scene.Bulbs ) {
                activity.Add( forScene.ActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Id ), new ActiveBulb( bulb )));
            }

            var now = DateTime.Now;

            return activity.Where( b => b.NextUpdateTime < now ).ToList();
        }

        private ActiveBulb UpdateBulb( ActiveBulb bulb, Scene inScene, SceneControl control ) {
            var color = inScene.Palette.Palette[ mLimitedRandom.Next( inScene.Palette.Palette.Count )];
            var transitionJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Parameters.DisplayTimeJitter.TotalSeconds ));
            var transitionTime = inScene.Parameters.BaseTransitionTime + transitionJitter;
            var displayJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Parameters.DisplayTimeJitter.TotalSeconds ));
            var displayTime = inScene.Parameters.BaseDisplayTime + displayJitter;
            var nextUpdateTime = DateTime.Now + transitionTime + displayTime;

            mHubManager.SetBulbState( bulb.Bulb, bulb.ActiveColor, control.Brightness, transitionTime );

            return new ActiveBulb( bulb.Bulb, color, nextUpdateTime );
        }

        public override void Dispose() {
            base.Dispose();

            mTokenSource.Cancel();

            if( mLightingTask != null ) {
                Task.WaitAll( new []{ mLightingTask }, TimeSpan.FromMilliseconds( 250 ));
            }

            if( mSchedulingTask != null ) {
                Task.WaitAll( new []{ mSchedulingTask }, TimeSpan.FromMilliseconds( 250 ));
            }

            mActiveSceneSubscription?.Dispose();
            mActiveSceneSubscription = null;

            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
