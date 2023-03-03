using System;
using DynamicData;
using DynamicData.Binding;
using Mushrooms.Models;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GardenDisplayViewModel : PropertyChangeBase, IDisposable {
        private IDisposable ?   mSceneSubscription;

        public  ObservableCollectionExtended<GardenSceneViewModel> SceneList { get; }

        public GardenDisplayViewModel( IMushroomGarden garden ) {
            SceneList = new ObservableCollectionExtended<GardenSceneViewModel>();

            mSceneSubscription = garden.Scenes
                .Connect()
                .Transform( s => new GardenSceneViewModel( s.Scene, garden ))
                .Bind( SceneList )
                .Subscribe();
        }

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
