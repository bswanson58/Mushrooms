using System;
using System.Collections.Generic;
using System.Linq;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System.Windows.Input;
using System.Windows.Media;
using Mushrooms.Entities;

namespace Mushrooms.Models {
    internal class GardenSceneViewModel : PropertyChangeBase, IDisposable {
        private readonly IMushroomGarden    mGarden;
        private IDisposable ?               mSceneSubscription;

        public  ActiveScene                 ActiveScene { get; }
        public  Scene                       Scene => ActiveScene.Scene;

        public  string                      Name => Scene.SceneName;
        public  bool                        IsSceneActive => ActiveScene.IsActive;
        public  bool                        IsScheduled => Scene.Schedule.Enabled;
        public  string                      ScheduleSummary => Scene.Schedule.ScheduleSummary();
        public  IEnumerable<Color>          SceneColors => IsSceneActive ? 
                                                ActiveScene.ActiveBulbs.OrderBy( b => b.Bulb.Name ).Select( s => s.ActiveColor ) :
                                                Scene.Palette.Palette.Take( 7 );

        public  ICommand                    ActivateScene { get; }
        public  ICommand                    DeactivateScene { get; }

        public GardenSceneViewModel( ActiveScene scene, IMushroomGarden garden ) {
            ActiveScene = scene;
            mGarden = garden;

            ActivateScene = new DelegateCommand( OnActivateScene );
            DeactivateScene = new DelegateCommand( OnDeactivateScene );

            mSceneSubscription = ActiveScene.OnSceneChanged.Subscribe( OnSceneChanged );
        }

        public double Brightness {
            get => ActiveScene.Control.Brightness;
            set {
                var brightness = Math.Max( Math.Min( 1.0, value ), 0.0 );

                mGarden.UpdateSceneControl( Scene, new SceneControl( brightness, 1.0 ));
            }
        }

        private void OnSceneChanged( ActiveScene scene ) {
            RaisePropertyChanged( () => IsSceneActive );
            RaisePropertyChanged( () => SceneColors );
        }

        private void OnActivateScene() {
            mGarden.StartScene( Scene );
        }

        private void OnDeactivateScene() {
            mGarden.StopScene( Scene );
        }

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
