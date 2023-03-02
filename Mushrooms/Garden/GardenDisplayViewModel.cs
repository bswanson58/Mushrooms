using System;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using Mushrooms.Models;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {
    internal class SceneViewModel : PropertyChangeBase {
        private readonly IMushroomGarden    mGarden;
        private readonly Scene              mScene;

        public  string          SceneName => mScene.SceneName;
        public  bool            IsSceneActive { get; private set; }

        public  ICommand        ActivateScene { get; }
        public  ICommand        DeactivateScene { get; }

        public SceneViewModel( Scene scene, IMushroomGarden garden ) {
            mScene = scene;
            mGarden = garden;

            ActivateScene = new DelegateCommand( OnActivateScene );
            DeactivateScene = new DelegateCommand( OnDeactivateScene );
        }

        private void OnActivateScene() {
            mGarden.StartScene( mScene );

            IsSceneActive = true;
            RaisePropertyChanged( () => IsSceneActive );
        }

        private void OnDeactivateScene() {
            mGarden.StopScene( mScene );

            IsSceneActive = false;
            RaisePropertyChanged( () => IsSceneActive );
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GardenDisplayViewModel : PropertyChangeBase, IDisposable {
        private readonly IMushroomGarden            mGarden;
        private IDisposable ?                       mSceneSubscription;

        public  ObservableCollectionExtended<SceneViewModel> SceneList { get; }

        public GardenDisplayViewModel( IMushroomGarden garden ) {
            mGarden = garden;

            SceneList = new ObservableCollectionExtended<SceneViewModel>();

            mSceneSubscription = mGarden.Scenes
                .Connect()
                .Transform( s => new SceneViewModel( s.Scene, mGarden ))
                .Bind( SceneList )
                .Subscribe();
        }

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
