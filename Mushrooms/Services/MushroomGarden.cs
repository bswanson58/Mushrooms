using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using DynamicData;
using DynamicData.Binding;
using HueLighting.Hub;
using Microsoft.Extensions.Hosting;
using Mushrooms.Database;
using Mushrooms.Dialogs;
using Mushrooms.Entities;
using Mushrooms.Models;
using Mushrooms.Support;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.EventAggregator;

namespace Mushrooms.Services {
    internal interface IMushroomGarden : IDisposable {
        Task    StartScene( Scene forScene );
        Task    StopScene( Scene forScene );

        Task    StartAll();
        Task    StopAll();

        Task    UpdateSceneControl( Scene forScene, SceneControl control );
        void    UpdateSceneColors( Scene forScene );
        	
        IObservable<IChangeSet<ActiveScene>>    ActiveScenes { get; }
    }

    internal class MushroomGarden : BackgroundService, IMushroomGarden {
        private readonly IHubManager                                mHubManager;
        private readonly ISceneLightingHandler                      mLightingHandler;
        private readonly ISceneProvider                             mSceneProvider;
        private readonly IDialogService                             mDialogService;
        private readonly IPreferences                               mPreferences;
        private readonly IBasicLog                                  mLog;
        private readonly IEventAggregator                           mEventAggregator;
        private readonly CancellationTokenSource                    mTokenSource;
        private readonly ReadOnlyObservableCollection<ActiveScene>  mActiveScenes;
        private IDisposable ?                                       mActiveSceneSubscription;
        private Task ?                                              mLightingTask;
        private Task ?                                              mSchedulingTask;

        public  IObservable<IChangeSet<ActiveScene>>                ActiveScenes => mActiveScenes.ToObservableChangeSet();

        public MushroomGarden( IHubManager hubManager, ISceneLightingHandler lightingHandler, IPreferences preferences,
                               IDialogService dialogService, ISceneProvider sceneProvider, IEventAggregator eventAggregator,
                               IBasicLog log ) {
            mHubManager = hubManager;
            mLightingHandler = lightingHandler;
            mSceneProvider = sceneProvider;
            mPreferences = preferences;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;
            mLog = log;

            mTokenSource = new CancellationTokenSource();

            mActiveSceneSubscription = mSceneProvider.Entities
                .Connect()
                .Sort( SortExpressionComparer<Scene>.Ascending( s => s.SceneName ))
                .TransformWithInlineUpdate( scene => new ActiveScene( scene ), UpdateActiveScene )
                .Bind( out mActiveScenes )
                .Subscribe();
        }

        public async Task StartScene( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                scene.Update( await mLightingHandler.GetSceneBulbs( scene.Scene ));

                await StopConflictingScenes( scene );
                await mLightingHandler.ActivateScene( scene );

                UpdateSceneColors( scene.Scene );

                scene.SetActiveState( DetermineActivationState( scene.Scene ));
            }
        }

        private SceneState DetermineActivationState( Scene forScene ) {
            var retValue = SceneState.Active;

            if( forScene.Schedule.Enabled ) {
                var preferences = mPreferences.Load<MushroomPreferences>();

                var startTime = forScene.Schedule.StartTimeForToday( preferences.Latitude, preferences.Longitude );
                var stopTime = forScene.Schedule.StopTimeForToday( preferences.Latitude, preferences.Longitude );
                var now = DateTime.Now;

                if(( startTime < now ) &&
                   ( stopTime > now )) {
                    retValue = SceneState.Scheduled;
                }
            }

            return retValue;
        }

        public void UpdateSceneColors( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                var bulbs = scene.GetSceneBulbs().Select( b => new ActiveBulb( b )).ToList();

                if( scene.Scene.SceneMode.Equals( SceneMode.Stationary )) {
                    var sceneColor = scene.Scene.Palette.Palette.FirstOrDefault( Colors.AntiqueWhite );
                    var updates = mLightingHandler.UpdateBulbs( bulbs, sceneColor, scene.Control );

                    foreach( var update in updates ) {
                        scene.Update( update );
                    }
                }
                else if( scene.Scene.Parameters.SynchronizeLights ) {
                    var updates = mLightingHandler.UpdateBulbs( bulbs, scene.Scene, scene.Control );

                    foreach( var update in updates ) {
                        scene.Update( update );
                    }
                }
                else {
                    foreach( var bulb in bulbs ) {
                        scene.Update( mLightingHandler.UpdateBulb( bulb, scene.Scene, scene.Control ));
                    }
                }
            }
        }

        public async Task StartAll() {
            var activeScenes = mActiveScenes
                .Where( s => s is {
                    IsActive: false, 
                    Scene.IsFavorite: true
                })
                .ToList();

            foreach( var scene in activeScenes ) {
                await StartScene( scene.Scene );
            }
        }

        public async Task StopScene( Scene forScene ) {
            var scene = mActiveScenes.FirstOrDefault( s => s.Scene.Id.Equals( forScene.Id ));

            if( scene != null ) {
                scene.Deactivate();

                await mLightingHandler.DeactivateScene( scene );
            }
        }

        public async Task StopAll() {
            var activeScenes = mActiveScenes.Where( s => s.IsActive ).ToList();

            foreach( var scene in activeScenes ) {
                await StopScene( scene.Scene );
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
                mDialogService.ShowDialog<HubRegistrationView>();
            }

            await base.StartAsync( cancellationToken );
        }

        public override async Task StopAsync( CancellationToken cancellationToken ) {
            await StopAll();

            // wait for light commands to stream out.
            await Task.Delay( 1000, cancellationToken );

            await base.StopAsync( cancellationToken );
        }

        private async Task StopConflictingScenes( ActiveScene scene ) {
            var activeScenes = mActiveScenes.Where( s => s.IsActive ).ToList();
            var sceneBulbs = scene.GetSceneBulbs();

            foreach( var activeScene in activeScenes.Where( s => !s.Scene.Id.Equals( scene.Scene.Id ))) {
                if( activeScene.GetSceneBulbs().Any( bulb => sceneBulbs.Any( b => b.Id.Equals( bulb.Id )))) {
                    await StopScene( activeScene.Scene );
                }
            }
        }

        protected override Task ExecuteAsync( CancellationToken stoppingToken ) {
            mLightingTask = Repeat.Interval( TimeSpan.FromMilliseconds( 100 ), LightingTask, mTokenSource.Token );
            mSchedulingTask = Repeat.Interval( TimeSpan.FromSeconds( 31 ), SchedulingTask, mTokenSource.Token );

            return Task.CompletedTask;
        }

        private void UpdateActiveScene( ActiveScene activeScene, Scene scene ) {
            if( activeScene.IsActive ) {
                SwitchSceneLighting( activeScene, scene );
            }

            activeScene.Update( scene );
        }

        private async void SwitchSceneLighting( ActiveScene activeScene, Scene scene ) {
            var originalLights = activeScene.GetOriginalLights();
            var currentLights = scene.Lights;

            // if the lighting list has changed, clear the current list and start over.
            if(( originalLights.Except( currentLights ).Any()) ||
               ( currentLights.Except( originalLights ).Any())) {
                await mLightingHandler.DeactivateScene( activeScene );

                activeScene.Update( currentLights );
                activeScene.ClearActiveBulbs();
                activeScene.Update( await mLightingHandler.GetSceneBulbs( scene ));

                await StopConflictingScenes( activeScene );

                await mLightingHandler.ActivateScene( activeScene );
            }
        }

        private void LightingTask() {
            try {
                var scenes = mActiveScenes
                    .Where( s => s is { Scene.SceneMode: SceneMode.Animating,
                        IsActive: true, 
                        Scene.Parameters.AnimationEnabled: true })
                    .ToList();

                foreach( var scene in scenes ) {
                    var updateList = mLightingHandler.GetBulbUpdateList( scene );

                    if( updateList.Any()) {
                        UpdateSceneLights( scene, updateList );
                    }
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "LightingTask Error", ex );
            }
        }

        private void UpdateSceneLights( ActiveScene scene, IList<ActiveBulb> updateList ) {
            if( scene.Scene.Parameters.SynchronizeLights ) {
                var updates = mLightingHandler.UpdateBulbs( scene.GetActiveBulbs(), scene.Scene, scene.Control );

                foreach( var update in updates ) {
                    scene.Update( update );
                }
            }
            else {
                scene.Update( mLightingHandler.UpdateBulb( updateList.First(), scene.Scene, scene.Control ));
            }
        }

        private async void SchedulingTask() {
            try {
                var potentialScenes = mActiveScenes.Where( s => s.Scene.Schedule.Enabled && !s.IsActive ).ToList();
                var runningScenes = mActiveScenes.Where( s => s.SceneState.Equals( SceneState.Scheduled )).ToList();

                foreach( var scene in potentialScenes ) {
                    await ActivateIfScheduleStart( scene );
                }

                foreach( var scene in runningScenes ) {
                    await DeactivateIfScheduleEnd( scene );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( "SchedulingTask", ex );
            }
        }

        private async Task ActivateIfScheduleStart( ActiveScene scene ) {
            var preferences = mPreferences.Load<MushroomPreferences>();
            var startTime = scene.Scene.Schedule.StartTimeForToday( preferences.Latitude, preferences.Longitude );
            var stopTime = scene.Scene.Schedule.StopTimeForToday( preferences.Latitude, preferences.Longitude );
            var now = DateTime.Now;

            if(( now > startTime ) &&
               ( now < stopTime ) &&
               ( now < startTime.AddMinutes( 2 ))) {
                await StartScene( scene.Scene );

                mLog.LogMessage( $"Started scheduled scene '{scene.Scene.SceneName}' at {DateTime.Now.ToShortTimeString()}" );
                mEventAggregator.Publish( 
                    new Events.StatusEvent( $"Started scheduled scene '{scene.Scene.SceneName}'" ) 
                        { ExtendDisplay = true });
            }
        }

        private async Task DeactivateIfScheduleEnd( ActiveScene scene ) {
            var preferences = mPreferences.Load<MushroomPreferences>();
            var stopTime = scene.Scene.Schedule.StopTimeForToday( preferences.Latitude, preferences.Longitude );

            if( DateTime.Now > stopTime ) {
                await mLightingHandler.DeactivateScene( scene );

                scene.Deactivate();

                mLog.LogMessage( $"Stopped scheduled scene '{scene.Scene.SceneName}' at {DateTime.Now.ToShortTimeString()}" );
                mEventAggregator.Publish( 
                    new Events.StatusEvent( $"Stopped scheduled scene '{scene.Scene.SceneName}'" ) 
                        { ExtendDisplay = true });
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
