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
        public  string      Name { get; }
        public  GroupType   GroupType { get; }
        public  IList<Bulb> Bulbs { get; }

        public  bool        IsSelected { get; set; }

        public  string      DisplayName => $"{Name} ({GroupType})";

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
        private readonly IPlanProvider          mPlanProvider;
        private readonly ISceneProvider         mSceneProvider;
        private string                          mSceneName;
        private ScenePlan ?                     mSelectedPlan;

        public  RangeCollection<ScenePlan>      Plans { get; }
        public  RangeCollection<LightingItem>   LightingList { get; }

        public  DelegateCommand                 CreateScene { get; }

        public SceneEditorViewModel( IHubManager hubManager, IPlanProvider planProvider, ISceneProvider sceneProvider ) {
            mHubManager = hubManager;
            mPlanProvider = planProvider;
            mSceneProvider = sceneProvider;

            mSceneName = String.Empty;

            Plans = new RangeCollection<ScenePlan>();
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

        public ScenePlan ? SelectedScene {
            get => mSelectedPlan;
            set {
                mSelectedPlan = value;

                CreateScene.RaiseCanExecuteChanged();
            }
        }

        private void LoadAssets() {
            Plans.AddRange( mPlanProvider.GetAll());
        }

        private async void LoadBulbs() {
            var groups = await mHubManager.GetBulbGroups();

            LightingList.AddRange( groups.Select( g => new LightingItem( g.Name, g.GroupType, g.Bulbs )));

            var bulbs = await mHubManager.GetBulbs();

            LightingList.AddRange( bulbs.Select( b => new LightingItem( b.Name, GroupType.Free, new List<Bulb>{ b } )));
        }

        private void OnCreateScene() {
            if(( SelectedScene != null ) &&
               ( LightingList.Any( l => l.IsSelected ))) {
                var bulbList = LightingList
                    .Where( b => b.IsSelected )
                    .SelectMany( b => b.Bulbs )
                    .GroupBy( b => b.Id )
                    .Select( g => g.First())
                    .ToList();

                var scene = new Scene {
                    Bulbs = bulbList,
                    Control = new SceneControl(),
                    Plan = SelectedScene,
                    SceneName = mSceneName
                };

                mSceneProvider.Insert( scene );
            }
        }

        private bool CanCreateScene() =>
            !String.IsNullOrWhiteSpace( mSceneName ) &&
            LightingList.Any( b => b.IsSelected ) &&
            SelectedScene != null;
    }
}
