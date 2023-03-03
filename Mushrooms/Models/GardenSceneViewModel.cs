using System.Collections.Generic;
using System.Linq;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System.Windows.Input;
using System.Windows.Media;
using Mushrooms.Entities;

namespace Mushrooms.Models {
    internal class GardenSceneViewModel : PropertyChangeBase {
        private readonly IMushroomGarden    mGarden;

        public  Scene                       Scene { get; }

        public  string                      Name => Scene.SceneName;
        public  IEnumerable<Color>          ExampleColors => Scene.Palette.Palette.Take( 7 );
        public  bool                        IsSceneActive { get; private set; }

        public  ICommand                    ActivateScene { get; }
        public  ICommand                    DeactivateScene { get; }

        public GardenSceneViewModel( Scene scene, IMushroomGarden garden ) {
            Scene = scene;
            mGarden = garden;

            ActivateScene = new DelegateCommand( OnActivateScene );
            DeactivateScene = new DelegateCommand( OnDeactivateScene );
        }

        private void OnActivateScene() {
            mGarden.StartScene( Scene );

            IsSceneActive = true;
            RaisePropertyChanged( () => IsSceneActive );
        }

        private void OnDeactivateScene() {
            mGarden.StopScene( Scene );

            IsSceneActive = false;
            RaisePropertyChanged( () => IsSceneActive );
        }
    }
}
