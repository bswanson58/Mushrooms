using System;
using System.Collections.Generic;
using System.Linq;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System.Windows.Input;
using System.Windows.Media;
using Mushrooms.Entities;
using Mushrooms.Garden;
using Mushrooms.Services;

namespace Mushrooms.Models {
    internal class GardenSceneViewModel : PropertyChangeBase, IDisposable {
        private readonly IMushroomGarden    mGarden;
        private readonly ISceneCommands     mSceneCommands;
        private IDisposable ?               mSceneSubscription;

        public  ActiveScene                 ActiveScene { get; }
        public  Scene                       Scene => ActiveScene.Scene;

        public  string                      Name => Scene.SceneName;
        public  bool                        IsSceneActive => ActiveScene.SceneState.Equals( SceneState.Active );
        public  bool                        IsScheduled => Scene.Schedule.Enabled;
        public  bool                        IsScheduleActive => ActiveScene.SceneState.Equals( SceneState.Scheduled );
        public  string                      ScheduleSummary => $"Scene Schedule: {Scene.Schedule.ScheduleSummary()}";
        public  bool                        CanRecolorScene => ActiveScene.IsActive && !Scene.Parameters.AnimationEnabled;
        public  IEnumerable<Color>          SceneColors => ActiveScene.IsActive ? 
                                                ActiveScene.ActiveBulbs.OrderBy( b => b.Bulb.Name ).Select( s => s.ActiveColor ) :
                                                Scene.Palette.Palette.Take( 7 );

        public  ICommand                    StartSceneAnimated { get; }
        public  ICommand                    StartSceneStationary { get; }

        public  ICommand                    ActivateScene { get; }
        public  ICommand                    DeactivateScene { get; }
        public  ICommand                    SetPalette { get; }
        public  ICommand                    SetParameters { get; }
        public  ICommand                    SetLighting { get; }
        public  ICommand                    SetSchedule { get; }
        public  ICommand                    RecolorScene { get; }
        public  ICommand                    DeleteScene { get; }

        public GardenSceneViewModel( ActiveScene scene, IMushroomGarden garden, ISceneCommands sceneCommands ) {
            ActiveScene = scene;
            mGarden = garden;
            mSceneCommands = sceneCommands;

            StartSceneAnimated = new DelegateCommand( OnStartSceneAnimated );
            StartSceneStationary = new DelegateCommand( OnStartSceneStationary );

            ActivateScene = new DelegateCommand( OnActivateScene );
            DeactivateScene = new DelegateCommand( OnDeactivateScene );
            SetLighting = new DelegateCommand( OnSetLighting );
            SetPalette = new DelegateCommand( OnSetPalette );
            SetParameters = new DelegateCommand( OnSetParameters );
            SetSchedule = new DelegateCommand( OnSetSchedule );
            RecolorScene = new DelegateCommand( OnRecolorScene );
            DeleteScene = new DelegateCommand( OnDeleteScene );

            mSceneSubscription = ActiveScene.OnSceneChanged.Subscribe( OnSceneChanged );
        }

        public double Brightness {
            get => ActiveScene.Control.Brightness;
            set {
                var brightness = Math.Max( Math.Min( 1.0, value ), 0.0 );

                mGarden.UpdateSceneControl( Scene, new SceneControl( brightness, 1.0 ));
            }
        }

        public bool IsFavorite {
            get => Scene.IsFavorite;
            set => mSceneCommands.SetFavorite( Scene, value );
        }

        private void OnStartSceneAnimated() => mSceneCommands.StartSceneAnimated( Scene );

        private void OnStartSceneStationary() => mSceneCommands.StartSceneStationary( Scene );

        private void OnSetLighting() => mSceneCommands.SetLighting( Scene );

        private void OnSetPalette() => mSceneCommands.SetPalette( Scene );

        private void OnSetParameters() => mSceneCommands.SetParameters( Scene );

        private void OnSetSchedule() => mSceneCommands.SetSchedule( Scene );

        private void OnDeleteScene() => mSceneCommands.DeleteScene( Scene );

        private void OnSceneChanged( ActiveScene scene ) {
            RaiseAllPropertiesChanged();

            RaisePropertyChanged( () => CanRecolorScene );
        }

        private void OnActivateScene() {
            mGarden.StartScene( Scene );

            RaisePropertyChanged( () => CanRecolorScene );
        }

        private void OnDeactivateScene() {
            mGarden.StopScene( Scene );

            RaisePropertyChanged( () => CanRecolorScene );
        }

        private void OnRecolorScene() {
            mGarden.UpdateSceneColors( Scene );
        }

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
