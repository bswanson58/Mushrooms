using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
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
        	
        IObservableCache<ActiveScene, String>   Scenes { get; }
    }

    internal enum SceneState {
        Active,
        Scheduled,
        Inactive
    }

    internal class ActiveScene {
        public  Scene           Scene { get; }
        public  SceneState      SceneState { get; private set; }
        public  bool            IsActive { get; private set; }
        public  SceneControl    Control { get; private set; }

        public ActiveScene( Scene scene ) {
            Scene = scene;
            Control = new SceneControl( Scene.Control.Brightness, Scene.Control.RateMultiplier );
            SceneState = SceneState.Inactive;
            IsActive = false;
        }

        public void Activate() {
            IsActive = true;
            SceneState = SceneState.Active;
        }

        public void ActiveBySchedule() {
            IsActive = true;
            SceneState = SceneState.Scheduled;
        }

        public void Deactivate() {
            IsActive = false;
            SceneState = SceneState.Inactive;
        }

        public void UpdateControl( SceneControl control ) {
            Control = control;
        }
    }

    internal class ActiveBulb {
        public  Bulb        Bulb { get; }
        public  DateTime    NextUpdateTime { get; }

        public ActiveBulb( Bulb bulb ) {
            Bulb = bulb;

            NextUpdateTime = DateTime.MinValue;
        }

        public ActiveBulb( Bulb bulb, DateTime nextUpdateTime ) {
            Bulb = bulb;
            NextUpdateTime = nextUpdateTime;
        }
    }

    internal class MushroomGarden : BackgroundService, IMushroomGarden {
        private readonly IHubManager                        mHubManager;
        private readonly ISceneProvider                     mSceneProvider;
        private readonly IDialogService                     mDialogService;
        private readonly SourceCache<ActiveScene, string>   mScenes;
        private readonly IList<ActiveBulb>                  mActiveBulbs;
        private readonly CancellationTokenSource            mTokenSource;
        private readonly LimitedRepeatingRandom             mLimitedRandom;
        private readonly Random                             mRandom;
        private Task ?                                      mLightingTask;
        private Task ?                                      mSchedulingTask;

        public  IObservableCache<ActiveScene, string>       Scenes => mScenes.AsObservableCache();

        public MushroomGarden( IHubManager hubManager, IDialogService dialogService, ISceneProvider sceneProvider ) {
            mHubManager = hubManager;
            mSceneProvider = sceneProvider;
            mDialogService = dialogService;

            mScenes = new SourceCache<ActiveScene, string>( s => s.Scene.Id );
            mActiveBulbs = new List<ActiveBulb>();
            mTokenSource = new CancellationTokenSource();
            mLimitedRandom = new LimitedRepeatingRandom();
            mRandom = Random.Shared;
        }

        public async Task StartScene( Scene forScene ) {
            var scene = mScenes.Items.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                await ActivateScene( scene.Scene );

                scene.Activate();
            }
        }

        private async Task ActivateScene( Scene scene ) {
            foreach( var bulb in scene.Bulbs ) {
                await mHubManager.SetBulbState( bulb, true );
                await mHubManager.SetBulbState( bulb, 10 ); // (int)scene.Control.Brightness );
            }
        }

        public async Task StopScene( Scene forScene ) {
            var scene = mScenes.Items.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                await DeactivateScene( scene.Scene );

                scene.Deactivate();
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
            mScenes.AddOrUpdate( mSceneProvider.GetAll().Select( s => new ActiveScene( s )).ToList());

            mLightingTask = Repeat.Interval( TimeSpan.FromMilliseconds( 200 ), LightingTask, mTokenSource.Token );
            mSchedulingTask = Repeat.Interval( TimeSpan.FromSeconds( 31 ), SchedulingTask, mTokenSource.Token );

            return Task.CompletedTask;
        }

        private void LightingTask() {
            foreach( var scene in mScenes.Items.Where( s => s.IsActive )) {
                var updateList = BuildBulbList( scene.Scene );

                if( updateList.Any()) {
                    UpdateActiveBulb( UpdateBulb( updateList.First().Bulb, scene.Scene ));
                }
            }
        }

        private async void SchedulingTask() {
            foreach( var scene in mScenes.Items.Where( s => s.Scene.Schedule.Enabled )) {
                if(!scene.IsActive ) {
                    await ActivateIfScheduleStart( scene );
                }
            }

            foreach( var scene in mScenes.Items.Where( s => s.SceneState.Equals( SceneState.Scheduled ))) {
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

        private IList<ActiveBulb> BuildBulbList( Scene forScene ) {
            var activity = new List<ActiveBulb>();

            foreach( var bulb in forScene.Bulbs ) {
                activity.Add( mActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Id ), new ActiveBulb( bulb )));
            }

            var now = DateTime.Now;

            return activity.Where( b => b.NextUpdateTime < now ).ToList();
        }

        private ActiveBulb UpdateBulb( Bulb bulb, Scene inScene ) {
            var color = inScene.Palette.Palette[ mLimitedRandom.Next( inScene.Palette.Palette.Count )];
            var transitionJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Parameters.DisplayTimeJitter.TotalSeconds ));
            var transitionTime = inScene.Parameters.BaseTransitionTime + transitionJitter;
            var displayJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Parameters.DisplayTimeJitter.TotalSeconds ));
            var displayTime = inScene.Parameters.BaseDisplayTime + displayJitter;
            var nextUpdateTime = DateTime.Now + transitionTime + displayTime;

            mHubManager.SetBulbState( bulb, color, 0.05D, transitionTime );

            return new ActiveBulb( bulb, nextUpdateTime );
        }

        private void UpdateActiveBulb( ActiveBulb bulb ) {
            var existing = mActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Bulb.Id ));

            if( existing != null ) {
                mActiveBulbs.Remove( existing );
            }

            mActiveBulbs.Add( bulb );
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
        }
    }
}
