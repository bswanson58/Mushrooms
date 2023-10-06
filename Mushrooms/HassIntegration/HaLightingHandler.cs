using Mushrooms.Models;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using DynamicData;
using HassMqtt;
using Mushrooms.Services;

namespace Mushrooms.HassIntegration {
    public interface IHaLightingHandler {
        Task    InitializeAsync();
    }

    internal class HaLightingHandler : IHaLightingHandler, IDisposable {
        private readonly IMushroomGarden                    mGarden;
        private readonly IHassManager                       mHassManager;
        private readonly CompositeDisposable                mSubscriptions;

        public HaLightingHandler( IHassManager hassManager, IMushroomGarden garden ) {
            mHassManager = hassManager;
            mGarden = garden;

            mSubscriptions = new CompositeDisposable();
        }

        public async Task InitializeAsync() {
            await mHassManager.InitializeAsync();

            mSubscriptions.Add( mGarden.ActiveScenes
                .AsObservableList()
                .Connect()
                .OnItemAdded( OnSceneAdded )
//              .OnItemRefreshed( OnSceneRefreshed )
                .OnItemRemoved( OnSceneRemoved )
                .Subscribe());
        }

        private void OnSceneAdded( ActiveScene scene ) =>
            mHassManager.LightsManager.AddLight( new HaSceneLight( scene, mGarden ));

//        private void OnSceneRefreshed( ActiveScene scene ) {
//            OnSceneRemoved( scene );
//            OnSceneAdded( scene );
//        }

        private void OnSceneRemoved( ActiveScene scene ) {
            mHassManager.LightsManager.RemoveLight( scene.Scene.Id );
        }

        public void Dispose() {
            mSubscriptions.Dispose();
        }
    }
}
