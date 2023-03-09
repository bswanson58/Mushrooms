using System;
using System.Collections.Generic;
using System.Linq;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System.Windows.Input;
using System.Windows.Media;
using Mushrooms.Entities;
using Mushrooms.Garden;

namespace Mushrooms.Models {
    internal class GardenSceneViewModel : PropertyChangeBase, IDisposable {
        private readonly IMushroomGarden    mGarden;
        private readonly ISceneCommands     mSceneCommands;
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
        public  ICommand                    SetPalette { get; }
        public  ICommand                    SetParameters { get; }
        public  ICommand                    SetLighting { get; }

        public GardenSceneViewModel( ActiveScene scene, IMushroomGarden garden, ISceneCommands sceneCommands ) {
            ActiveScene = scene;
            mGarden = garden;
            mSceneCommands = sceneCommands;

            ActivateScene = new DelegateCommand( OnActivateScene );
            DeactivateScene = new DelegateCommand( OnDeactivateScene );
            SetLighting = new DelegateCommand( OnSetLighting );
            SetPalette = new DelegateCommand( OnSetPalette );
            SetParameters = new DelegateCommand( OnSetParameters );

            mSceneSubscription = ActiveScene.OnSceneChanged.Subscribe( OnSceneChanged );
        }

        public double Brightness {
            get => ActiveScene.Control.Brightness;
            set {
                var brightness = Math.Max( Math.Min( 1.0, value ), 0.0 );

                mGarden.UpdateSceneControl( Scene, new SceneControl( brightness, 1.0 ));
            }
        }

        private void OnSetLighting() => mSceneCommands.SetLighting( Scene );

        private void OnSetPalette() => mSceneCommands.SetPalette( Scene );

        private void OnSetParameters() => mSceneCommands.SetParameters( Scene );


        private void OnSceneChanged( ActiveScene scene ) {
            RaiseAllPropertiesChanged();
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
