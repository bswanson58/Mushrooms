using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using HueLighting.Hub;
using HueLighting.HubSelection;
using Microsoft.Extensions.Hosting;
using Mushrooms.Database;
using Mushrooms.Entities;
using Mushrooms.Models;
using Mushrooms.Support;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.Services {
    internal interface IMushroomGarden : IDisposable {
        Task    StartScene( Scene forScene );
        Task    StopScene( Scene forScene );

        Task    UpdateSceneControl( Scene forScene, SceneControl control );
        	
        IObservable<IChangeSet<ActiveScene>>    ActiveScenes { get; }
    }

    internal class MushroomGarden : BackgroundService, IMushroomGarden {
        private readonly IHubManager                                mHubManager;
        private readonly ISceneLightingHandler                      mLightingHandler;
        private readonly ISceneProvider                             mSceneProvider;
        private readonly IDialogService                             mDialogService;
        private readonly ObservableCollectionExtended<ActiveScene>  mActiveScenes;
        private readonly CancellationTokenSource                    mTokenSource;
        private IDisposable ?                                       mActiveSceneSubscription;
        private Task ?                                              mLightingTask;
        private Task ?                                              mSchedulingTask;

        public  IObservable<IChangeSet<ActiveScene>>                ActiveScenes => mActiveScenes.ToObservableChangeSet();

        public MushroomGarden( IHubManager hubManager, ISceneLightingHandler lightingHandler,
                               IDialogService dialogService, ISceneProvider sceneProvider ) {
            mHubManager = hubManager;
            mLightingHandler = lightingHandler;
            mSceneProvider = sceneProvider;
            mDialogService = dialogService;

            mActiveScenes = new ObservableCollectionExtended<ActiveScene>();
            mTokenSource = new CancellationTokenSource();
        }

        public async Task StartScene( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                scene.Update( await mLightingHandler.GetSceneBulbs( scene.Scene ));
                await mLightingHandler.ActivateScene( scene );

                scene.Activate();
            }
        }

        public async Task StopScene( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                await mLightingHandler.DeactivateScene( scene );

                scene.Deactivate();
            }
        }

        public async Task UpdateSceneControl( Scene forScene, SceneControl control ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                scene.Update( control );

                await mLightingHandler.UpdateSceneBrightness( scene );
                mSceneProvider.Update( scene.Scene );
            }
        }

        public override async Task StartAsync( CancellationToken cancellationToken ) {
            if(! await mHubManager.InitializeConfiguredHub()) {
                mDialogService.ShowDialog<HubSelectionView>();
            }

            await base.StartAsync( cancellationToken );
        }

        protected override Task ExecuteAsync( CancellationToken stoppingToken ) {
            mActiveSceneSubscription = mSceneProvider.Entities
                .Connect()
                .Sort( SortExpressionComparer<Scene>.Ascending( s => s.SceneName ))
                .TransformWithInlineUpdate( scene => new ActiveScene( scene ),
                                          ( activeScene, scene ) => activeScene.Update( scene ))
                .Bind( mActiveScenes )
                .Subscribe();

            mLightingTask = Repeat.Interval( TimeSpan.FromMilliseconds( 125 ), LightingTask, mTokenSource.Token );
            mSchedulingTask = Repeat.Interval( TimeSpan.FromSeconds( 31 ), SchedulingTask, mTokenSource.Token );

            return Task.CompletedTask;
        }

        private void LightingTask() {
            foreach( var scene in mActiveScenes.Where( s => s.IsActive )) {
                var updateList = mLightingHandler.BuildBulbList( scene );

                if( updateList.Any()) {
                    scene.Update( mLightingHandler.UpdateBulb( updateList.First(), scene.Scene, scene.Control ));
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
                await mLightingHandler.ActivateScene( scene );

                scene.ActiveBySchedule();
            }
        }

        private async Task DeactivateIfScheduleEnd( ActiveScene scene ) {
            var stopTime = scene.Scene.Schedule.StopTimeForToday();

            if( DateTime.Now > stopTime ) {
                await mLightingHandler.DeactivateScene( scene );

                scene.Deactivate();
            }
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
        }
    }
}
