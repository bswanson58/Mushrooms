using System;
using System.Collections.Generic;
using System.Linq;
using HueLighting.Hub;
using HueLighting.Models;
using Mushrooms.Database;
using Mushrooms.Models;
using Q42.HueApi.Models.Groups;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.SceneBuilder {
    internal class LightingItem {
        public  string          Name { get; }
        public  GroupType       GroupType { get; }
        public  IList<Bulb>     Bulbs { get; }

        public  bool            IsSelected { get; set; }

        public  string          DisplayName => $"{Name} ({GroupType})";

        public LightingItem( string name, GroupType groupType, IList<Bulb> bulbs ) {
            Name = name;
            GroupType = groupType;
            Bulbs = bulbs;

            IsSelected = false;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SceneEditorViewModel : PropertyChangeBase {
        private readonly IHubManager            mHubManager;
        private readonly IPaletteProvider       mPaletteProvider;
        private readonly ISceneProvider         mSceneProvider;
        private string                          mSceneName;
        private ScenePalette ?                  mSelectedPalette;

        public  RangeCollection<ScenePalette>   Palettes { get; }
        public  RangeCollection<LightingItem>   LightingList { get; }

        public  DelegateCommand                 CreateScene { get; }

        public SceneEditorViewModel( IHubManager hubManager, IPaletteProvider paletteProvider, ISceneProvider sceneProvider ) {
            mHubManager = hubManager;
            mPaletteProvider = paletteProvider;
            mSceneProvider = sceneProvider;

            mSceneName = String.Empty;

            Palettes = new RangeCollection<ScenePalette>();
            LightingList = new RangeCollection<LightingItem>();

            CreateScene = new DelegateCommand( OnCreateScene, CanCreateScene );

            LoadAssets();
            LoadBulbs();
        }

        public string SceneName {
             get => mSceneName;
             set {
                 mSceneName = value;

                 CreateScene.RaiseCanExecuteChanged();
             }
        }

        public ScenePalette ? SelectedPalette {
            get => mSelectedPalette;
            set {
                mSelectedPalette = value;

                CreateScene.RaiseCanExecuteChanged();
            }
        }

        private void LoadAssets() {
            Palettes.AddRange( mPaletteProvider.GetAll());
        }

        private async void LoadBulbs() {
            var groups = await mHubManager.GetBulbGroups();

            LightingList.AddRange( groups.Select( g => new LightingItem( g.Name, g.GroupType, g.Bulbs )));

            var bulbs = await mHubManager.GetBulbs();

            LightingList.AddRange( bulbs.Select( b => new LightingItem( b.Name, GroupType.Free, new List<Bulb>{ b } )));
        }

        private void OnCreateScene() {
            if(( SelectedPalette != null ) &&
               ( LightingList.Any( l => l.IsSelected ))) {
                var bulbList = LightingList
                    .Where( b => b.IsSelected )
                    .SelectMany( b => b.Bulbs )
                    .GroupBy( b => b.Id )
                    .Select( g => g.First())
                    .ToList();

                var scene = new Scene( SceneName, SelectedPalette, SceneParameters.Default, new SceneControl(), bulbList );

                mSceneProvider.Insert( scene );
            }
        }

        private bool CanCreateScene() =>
            !String.IsNullOrWhiteSpace( mSceneName ) &&
            LightingList.Any( b => b.IsSelected ) &&
            SelectedPalette != null;
    }
}
