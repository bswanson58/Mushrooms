using Mushrooms.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using HassMqtt;
using Microsoft.Extensions.Hosting;
using Mushrooms.Services;

// ReSharper disable IdentifierTypo

namespace Mushrooms.HassIntegration {
    internal class HaLightingHandler : BackgroundService {
        private readonly IMushroomGarden    mGarden;
        private readonly IHassManager       mHassManager;
        private IDisposable ?               mSceneSubscription;

        public HaLightingHandler( IHassManager hassManager, IMushroomGarden garden ) {
            mHassManager = hassManager;
            mGarden = garden;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
            await mHassManager.InitializeAsync();

            mSceneSubscription = mGarden.ActiveScenes
                .AsObservableList()
                .Connect()
                .OnItemAdded( OnSceneAdded )
                .OnItemRemoved( OnSceneRemoved )
                .Subscribe();
        }

        private void OnSceneAdded( ActiveScene scene ) =>
            mHassManager.LightsManager.AddLight( new HaSceneLight( scene, mGarden ));

        private void OnSceneRemoved( ActiveScene scene ) {
            mHassManager.LightsManager.RemoveLight( scene.Scene.Id );
        }

        public override void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
