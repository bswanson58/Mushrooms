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
        public  RangeCollection<LightSourceViewModel>                   LightingList { get; }

        public  string                              DisplayDuration => mDisplayDuration.ToString();
        public  string                              DisplayJitter => mDisplayJitter.ToString();
        public  string                              TransitionDuration => mTransitionDuration.ToString();
        public  string                              TransitionJitter => mTransitionJitter.ToString();

        public  DelegateCommand                     EditSchedule { get; }
        public  DelegateCommand                     CreateScene { get; }

        public SceneEditorViewModel( IHubManager hubManager, IPaletteProvider paletteProvider, ISceneProvider sceneProvider,
                                     IDialogService dialogService ) {
            mHubManager = hubManager;
            mSceneProvider = sceneProvider;
            mDialogService = dialogService;

            mSceneName = String.Empty;

            var defaultParameters = SceneParameters.Default;

            mDisplayDuration = defaultParameters.BaseDisplayTime;
            mDisplayJitter = defaultParameters.DisplayTimeJitter;
            mTransitionDuration = defaultParameters.BaseTransitionTime;
            mTransitionJitter = defaultParameters.TransitionJitter;

            mSchedule = SceneSchedule.Default;

            Scenes = new ObservableCollectionExtended<EditableSceneViewModel>();

            mSceneSubscription = mSceneProvider.Entities
                .Connect()
                .Transform( s => new EditableSceneViewModel( s, OnDeleteScene ))
                .Bind( Scenes )
                .Subscribe();

            Palettes = new ObservableCollectionExtended<PaletteViewModel>();

            mPaletteSubscription = paletteProvider.Entities
                .Connect()
                .Transform( p => new PaletteViewModel( p ))
                .Bind( Palettes )
                .Subscribe();

            LightingList = new RangeCollection<LightSourceViewModel>();

            EditSchedule = new DelegateCommand( OnEditSchedule );
            CreateScene = new DelegateCommand( OnCreateScene, CanCreateScene );

            LoadBulbs();
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

        public string SceneName {
             get => mSceneName;
             set {
                 mSceneName = value;

                 CreateScene.RaiseCanExecuteChanged();
             }
        }

        public PaletteViewModel ? SelectedPalette {
            get => mSelectedPalette;
            set {
                mSelectedPalette = value;

                CreateScene.RaiseCanExecuteChanged();
            }
        }

        private async void LoadBulbs() {
            var groups = await mHubManager.GetBulbGroups();

            LightingList.AddRange( groups.Select( 
                g => new LightSourceViewModel( new LightSource( g.Name, g.GroupType ), g.Bulbs )));

            var bulbs = await mHubManager.GetBulbs();

            LightingList.AddRange( bulbs.Select( 
                b => new LightSourceViewModel( new LightSource( b.Name, GroupType.Free ), new List<Bulb>{ b })));
        }

        private void OnEditSchedule() {
            var parameters = new DialogParameters{{ ScheduleEditDialogViewModel.cSchedule, mSchedule }};

            mDialogService.ShowDialog<ScheduleEditDialog>( parameters, result => {
                if( result.Result == ButtonResult.Ok ) {
                    mSchedule = result.Parameters.GetValue<SceneSchedule>( ScheduleEditDialogViewModel.cSchedule ) ?? SceneSchedule.Default;
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
            if(( SelectedPalette != null ) &&
               ( LightingList.Any( l => l.IsSelected ))) {
                var lightingList = LightingList
                    .Where( b => b.IsSelected )
                    .Select( l => l.LightSource )
                    .ToList();

                var sceneParameters = 
                    new SceneParameters( mTransitionDuration, mTransitionJitter, mDisplayDuration, mDisplayJitter );

                var scene = 
                    new Scene( SceneName, SelectedPalette.Palette, sceneParameters, SceneControl.Default, lightingList, mSchedule );

                mSceneProvider.Insert( scene );
            }
        }

        private bool CanCreateScene() =>
            !String.IsNullOrWhiteSpace( mSceneName ) &&
            LightingList.Any( b => b.IsSelected ) &&
            SelectedPalette != null;

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;

            mPaletteSubscription?.Dispose();
            mPaletteSubscription = null;
        }
    }
}
