using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using HueLighting.Hub;
using HueLighting.Models;
using Mushrooms.Database;
using Mushrooms.Dialogs;
using Mushrooms.Entities;
using Mushrooms.Models;
using Mushrooms.Scheduler;
using Q42.HueApi.Models.Groups;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.SceneBuilder {
    internal class EditableSceneViewModel : SceneViewModel {
        private readonly Action<EditableSceneViewModel> mOnDelete;

        public  ICommand        Delete { get; }

        public EditableSceneViewModel( Scene scene, Action<EditableSceneViewModel> onDelete )
            : base( scene ) {
            Delete = new DelegateCommand( OnDelete );
            mOnDelete = onDelete;
        }

        private void OnDelete() {
            mOnDelete.Invoke( this );
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SceneEditorViewModel : PropertyChangeBase, IDisposable {
        private readonly IHubManager                mHubManager;
        private readonly ISceneProvider             mSceneProvider;
        private readonly IDialogService             mDialogService;
        private EditableSceneViewModel ?            mSelectedScene;
        private string                              mSceneName;
        private PaletteViewModel ?                  mSelectedPalette;
        private SceneSchedule                       mSchedule;
        private TimeSpan                            mTransitionDuration;
        private TimeSpan                            mTransitionJitter;
        private TimeSpan                            mDisplayDuration;
        private TimeSpan                            mDisplayJitter;
        private IDisposable ?                       mSceneSubscription;
        private IDisposable ?                       mPaletteSubscription;

        public  ObservableCollectionExtended<EditableSceneViewModel>    Scenes { get; }
        public  ObservableCollectionExtended<PaletteViewModel>          Palettes { get; }
        public  RangeCollection<LightSourceViewModel>                   RoomsList { get; }
        public  RangeCollection<LightSourceViewModel>                   ZonesList { get; }
        public  RangeCollection<LightSourceViewModel>                   BulbsList { get; }

        public  string                              DisplayDuration => mDisplayDuration.ToString();
        public  string                              DisplayJitter => mDisplayJitter.ToString();
        public  string                              TransitionDuration => mTransitionDuration.ToString();
        public  string                              TransitionJitter => mTransitionJitter.ToString();

        public  bool                                IsScheduleActive => mSchedule.Enabled;
        public  string                              ScheduleSummary => mSchedule.ScheduleSummary();

        public  DelegateCommand                     NewScene { get; }
        public  DelegateCommand                     EditSchedule { get; }
        public  DelegateCommand                     CreateScene { get; }

        public SceneEditorViewModel( IHubManager hubManager, IPaletteProvider paletteProvider, ISceneProvider sceneProvider,
                                     IDialogService dialogService ) {
            mHubManager = hubManager;
            mSceneProvider = sceneProvider;
            mDialogService = dialogService;

            mTransitionDuration = TimeSpan.MinValue;
            mTransitionJitter = TimeSpan.MinValue;
            mDisplayDuration = TimeSpan.MinValue;
            mDisplayJitter = TimeSpan.MinValue;
            mSchedule = SceneSchedule.Default;
            mSceneName = String.Empty;

            SetDefaultParameters();

            Scenes = new ObservableCollectionExtended<EditableSceneViewModel>();

            mSceneSubscription = mSceneProvider.Entities
                .Connect()
                .Transform( s => new EditableSceneViewModel( s, OnDeleteScene ))
                .Sort( SortExpressionComparer<EditableSceneViewModel>.Ascending( s => s.Name ))
                .Bind( Scenes )
                .Subscribe();

            Palettes = new ObservableCollectionExtended<PaletteViewModel>();

            mPaletteSubscription = paletteProvider.Entities
                .Connect()
                .Transform( p => new PaletteViewModel( p ))
                .Sort( SortExpressionComparer<PaletteViewModel>.Ascending( p => p.Name ))
                .Bind( Palettes )
                .Subscribe();

            RoomsList = new RangeCollection<LightSourceViewModel>();
            ZonesList = new RangeCollection<LightSourceViewModel>();
            BulbsList = new RangeCollection<LightSourceViewModel>();

            NewScene = new DelegateCommand( OnNewScene );
            EditSchedule = new DelegateCommand( OnEditSchedule );
            CreateScene = new DelegateCommand( OnCreateScene, CanCreateScene );

            LoadLighting();
        }

        public EditableSceneViewModel ? SelectedScene {
            get => mSelectedScene;
            set {
                mSelectedScene = value;

                SelectScene( mSelectedScene );
            }
        }

        public int TransitionDurationSeconds {
            get => (int)mTransitionDuration.TotalSeconds;
            set { 
                mTransitionDuration = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => TransitionDuration );
            }
        }

        public int TransitionDurationJitterSeconds {
            get => (int)mTransitionJitter.TotalSeconds;
            set { 
                mTransitionJitter = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => TransitionJitter );
            }
        }

        public int DisplayDurationSeconds {
            get => (int)mDisplayDuration.TotalSeconds;
            set { 
                mDisplayDuration = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => DisplayDuration );
            }
        }

        public int DisplayDurationJitterSeconds {
            get => (int)mDisplayJitter.TotalSeconds;
            set { 
                mDisplayJitter = TimeSpan.FromSeconds( value );

                RaisePropertyChanged( () => DisplayJitter );
            }
        }

        public bool IsScheduled {
            get => mSchedule.Enabled;
            set {
                mSchedule.SetEnabled( value );

                RaisePropertyChanged( () => IsScheduleActive );
            }
        }

        public string SceneName {
             get => mSceneName;
             set {
                 mSceneName = value;

                 CreateScene.RaiseCanExecuteChanged();
             }
        }

        private void SelectScene( EditableSceneViewModel ? scene ) {
            if( scene != null ) {
                SelectedPalette = Palettes.FirstOrDefault( p => p.Palette.Id.Equals( scene.Scene.Palette.Id ));

                foreach( var lightSource in RoomsList ) {
                    lightSource.IsSelected = scene.Scene.Lights.Any( l => l.SourceName.Equals( lightSource.LightSource.SourceName ));
                }
                foreach( var lightSource in ZonesList ) {
                    lightSource.IsSelected = scene.Scene.Lights.Any( l => l.SourceName.Equals( lightSource.LightSource.SourceName ));
                }
                foreach( var lightSource in BulbsList ) {
                    lightSource.IsSelected = scene.Scene.Lights.Any( l => l.SourceName.Equals( lightSource.LightSource.SourceName ));
                }

                mDisplayDuration = scene.Scene.Parameters.BaseDisplayTime;
                mDisplayJitter = scene.Scene.Parameters.DisplayTimeJitter;
                mTransitionDuration = scene.Scene.Parameters.BaseTransitionTime;
                mTransitionJitter = scene.Scene.Parameters.TransitionJitter;
                mSchedule = scene.Scene.Schedule;
                mSceneName = scene.Name;

                RaiseAllPropertiesChanged();
            }
        }

        public PaletteViewModel ? SelectedPalette {
            get => mSelectedPalette;
            set {
                mSelectedPalette = value;

                RaisePropertyChanged( () => SelectedPalette );
                CreateScene.RaiseCanExecuteChanged();
            }
        }

        private async void LoadLighting() {
            var groups = await mHubManager.GetBulbGroups();

            foreach( var group in groups.OrderBy( g => g.Name )) {
                if( group.GroupType.Equals( GroupType.Room )) {
                    RoomsList.Add( 
                        new LightSourceViewModel( 
                            new LightSource( group.Name, group.GroupType ), group.Bulbs, OnLightSelected ));
                }
                else {
                    ZonesList.Add( 
                        new LightSourceViewModel( 
                            new LightSource( group.Name, group.GroupType ), group.Bulbs, OnLightSelected ));
                }
            }

            var bulbs = await mHubManager.GetBulbs();

            BulbsList.AddRange( bulbs
                .OrderBy( b => b.Name )
                .Select( 
                b => new LightSourceViewModel( 
                    new LightSource( b.Name, GroupType.Free ), new List<Bulb>{ b }, OnLightSelected )));
        }

        private void OnLightSelected( LightSourceViewModel light ) {
            CreateScene.RaiseCanExecuteChanged();
        }

        private void OnNewScene() {
            var parameters = new DialogParameters {
                { NewSceneViewModel.cLightingList, RoomsList.Concat( ZonesList ).ToList() },
                { NewSceneViewModel.cPaletteList, Palettes }
            };

            mDialogService.ShowDialog<NewSceneView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var selectedPalette = result.Parameters.GetValue<string>( NewSceneViewModel.cSelectedPalette );

                    mSelectedPalette = Palettes.FirstOrDefault( p => p.PaletteId.Equals( selectedPalette ));
                    mSceneName = result.Parameters.GetValue<string>( NewSceneViewModel.cSceneName ) ?? String.Empty;

                    var selectedLighting = result.Parameters.GetValue<string>( NewSceneViewModel.cSelectedLights );
                    var lighting = RoomsList.FirstOrDefault( l => l.LightId.Equals( selectedLighting ));

                    ClearLightingSelections();

                    if( lighting != null ) {
                        lighting.IsSelected = true;
                    }
                    else {
                        lighting = ZonesList.FirstOrDefault( l => l.LightId.Equals( selectedLighting ));

                        if( lighting != null ) {
                            lighting.IsSelected = true;
                        }
                    }

                    SetDefaultParameters();
                    RaiseAllPropertiesChanged();
                }
            });
        }

        private void ClearLightingSelections() {
            foreach( var room in RoomsList ) {
                room.IsSelected = false;
            }

            foreach( var zone in ZonesList ) {
                zone.IsSelected = false;
            }

            foreach( var bulb in BulbsList ) {
                bulb.IsSelected = false;
            }
        }

        private void OnEditSchedule() {
            var parameters = new DialogParameters{{ ScheduleEditDialogViewModel.cSchedule, mSchedule }};

            mDialogService.ShowDialog<ScheduleEditDialog>( parameters, result => {
                if( result.Result == ButtonResult.Ok ) {
                    mSchedule = result.Parameters.GetValue<SceneSchedule>( ScheduleEditDialogViewModel.cSchedule ) ?? SceneSchedule.Default;

                    RaisePropertyChanged( () => ScheduleSummary );
                }
            });
        }

        private void OnDeleteScene( EditableSceneViewModel scene ) {
            var parameters = new DialogParameters{
                { ConfirmationDialogViewModel.cTitle, "Confirm Deletion" },
                { ConfirmationDialogViewModel.cMessage, $"Would you like to delete scene named: '{scene.Scene.SceneName}'?" }};

            mDialogService.ShowDialog<ConfirmationDialog>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    mSceneProvider.Delete( scene.Scene );
                }
            });
        }

        private void OnCreateScene() {
            if( CanCreateScene()) {
                if( mSelectedScene == null ) {
                    InsertScene();
                }
                else {
                    UpdateScene();
                }
            }
        }

        private void InsertScene() {
            if( mSelectedPalette != null ) {
                var sceneParameters = 
                    new SceneParameters( mTransitionDuration, mTransitionJitter, mDisplayDuration, mDisplayJitter );

                var scene = 
                    new Scene( SceneName, mSelectedPalette.Palette, sceneParameters, 
                        SceneControl.Default, CollectSelectedLights(), mSchedule );

                mSceneProvider.Insert( scene );
            }
        }

        private void UpdateScene() {
            if(( mSelectedScene != null ) &&
               ( SelectedPalette != null )) {
                var sceneParameters = 
                    new SceneParameters( mTransitionDuration, mTransitionJitter, mDisplayDuration, mDisplayJitter );

                mSelectedScene.Scene.UpdateFrom(
                    new Scene( SceneName, SelectedPalette.Palette, sceneParameters, 
                        SceneControl.Default, CollectSelectedLights(), mSchedule ));

                mSceneProvider.Update( mSelectedScene.Scene );
            }
        }

        private IEnumerable<LightSource> CollectSelectedLights() {
            var selectedRooms = RoomsList.Where( r => r.IsSelected ).Select( r => r.LightSource );
            var selectedZones = ZonesList.Where( z => z.IsSelected ).Select( z => z.LightSource );
            var selectedBulbs = BulbsList.Where( b => b.IsSelected ).Select( b => b.LightSource );

            return selectedRooms.Concat( selectedZones ).Concat( selectedBulbs );
        }

        private bool CanCreateScene() =>
            !String.IsNullOrWhiteSpace( mSceneName ) &&
            CollectSelectedLights().Any() &&
            SelectedPalette != null;

        private void SetDefaultParameters() {
            var defaultParameters = SceneParameters.Default;

            mDisplayDuration = defaultParameters.BaseDisplayTime;
            mDisplayJitter = defaultParameters.DisplayTimeJitter;
            mTransitionDuration = defaultParameters.BaseTransitionTime;
            mTransitionJitter = defaultParameters.TransitionJitter;

            mSchedule = SceneSchedule.Default;
        }

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;

            mPaletteSubscription?.Dispose();
            mPaletteSubscription = null;
        }
    }
}
