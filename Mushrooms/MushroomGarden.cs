using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HueLighting.Hub;
using HueLighting.HubSelection;
using HueLighting.Models;
using Microsoft.Extensions.Hosting;
using Mushrooms.Database;
using Mushrooms.Models;
using Mushrooms.Support;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms {
    internal interface IMushroomGarden { }

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
        private readonly IHubManager                mHubManager;
        private readonly IPlanProvider              mPlanProvider;
        private readonly IDialogService             mDialogService;
        private readonly IList<Scene>               mActiveScenes;
        private readonly IList<ActiveBulb>          mActiveBulbs;
        private readonly CancellationTokenSource    mTokenSource;
        private readonly LimitedRepeatingRandom     mLimitedRandom;
        private readonly Random                     mRandom;
        private Task ?                              mLightingTask;

        public MushroomGarden( IHubManager hubManager, IDialogService dialogService, IPlanProvider planProvider ) {
            mHubManager = hubManager;
            mPlanProvider = planProvider;
            mDialogService = dialogService;

            mActiveScenes = new List<Scene>();
            mActiveBulbs = new List<ActiveBulb>();
            mTokenSource = new CancellationTokenSource();
            mLimitedRandom = new LimitedRepeatingRandom();
            mRandom = Random.Shared;
        }

        public override async Task StartAsync( CancellationToken cancellationToken ) {
            if(! await mHubManager.InitializeConfiguredHub()) {
                mDialogService.ShowDialog<HubSelectionView>();
            }

            await base.StartAsync( cancellationToken );
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
            var scenes = mPlanProvider.GetAll().ToList();
            var groups = await mHubManager.GetBulbGroups();
            var bulbs = ( await mHubManager.GetBulbs()).ToList();

            if(( scenes.Any()) &&
               ( bulbs.Any())) {
                var scene = scenes.First();

                var activeScene = new Scene {
                    SceneName = scene.PlanName,
                    Bulbs = bulbs.ToList(),
                    Plan = scene,
                    Control = new SceneControl {
                        Brightness = 0.3
                    }
                };

                await ActivateScene( activeScene );
                mActiveScenes.Add( activeScene );
            }

            mLightingTask = Repeat.Interval( TimeSpan.FromMilliseconds( 200 ), LightingTask, mTokenSource.Token );
        }

        private async Task ActivateScene( Scene scene ) {
            foreach( var bulb in scene.Bulbs ) {
                await mHubManager.SetBulbState( bulb, true );
                await mHubManager.SetBulbState( bulb, 10 ); // (int)scene.Control.Brightness );
            }
        }

        private void LightingTask() {
            foreach( var scene in mActiveScenes ) {
                var updateList = BuildBulbList( scene );

                if( updateList.Any()) {
                    UpdateActiveBulb( UpdateBulb( updateList.First().Bulb, scene ));
                }
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
            var color = inScene.Plan.Palette.Palette.ToList().Randomize().First();
            var transitionJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Plan.Parameters.DisplayTimeJitter.TotalSeconds ));
            var transitionTime = inScene.Plan.Parameters.BaseTransitionTime + transitionJitter;
            var displayJitter = TimeSpan.FromSeconds( mRandom.Next((int)inScene.Plan.Parameters.DisplayTimeJitter.TotalSeconds ));
            var displayTime = inScene.Plan.Parameters.BaseDisplayTime + displayJitter;
            var nextUpdateTime = DateTime.Now + transitionTime + displayTime;

            mHubManager.SetBulbState( bulb, color, inScene.Control.Brightness, transitionTime );

            return new ActiveBulb( bulb, nextUpdateTime );
        }

        private void UpdateActiveBulb( ActiveBulb bulb ) {
            var existing = mActiveBulbs.FirstOrDefault( b => b.Bulb.Id.Equals( bulb.Bulb.Id ));

            if( existing != null ) {
                mActiveBulbs.Remove( existing );
            }

            mActiveBulbs.Add( bulb );
        }
    }
}
