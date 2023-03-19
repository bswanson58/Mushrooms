using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using HueLighting.Hub;
using Mushrooms.Database;
using Mushrooms.Dialogs;
using Mushrooms.Entities;
using Mushrooms.Models;
using Mushrooms.Services;
using Q42.HueApi.Models.Groups;
using ReusableBits.Platform.Preferences;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {
    internal interface ISceneCommands {
        void    StartSceneAnimated( Scene scene );
        void    StartSceneStationary( Scene scene );

        void    SetLighting( Scene scene );
        void    SetPalette( Scene scene );
        void    SetParameters( Scene scene );
        void    SetSchedule( Scene scene );
        void    SetFavorite( Scene scene, bool state );
        void    RenameScene( Scene scene );

        void    DeleteScene( Scene scene );
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GardenDisplayViewModel : PropertyChangeBase, ISceneCommands, IDisposable {
        private readonly IHubManager        mHubManager;
        private readonly IMushroomGarden    mGarden;
        private readonly IPaletteProvider   mPaletteProvider;
        private readonly ISceneProvider     mSceneProvider;
        private readonly IDialogService     mDialogService;
        private IDisposable ?               mSceneSubscription;

        public  ObservableCollectionExtended<GardenSceneViewModel> SceneList { get; }

        public  ICommand                    CreateScene { get; }
        public  ICommand                    StopAll { get; }
        public  ICommand                    StartAll { get; }

        public GardenDisplayViewModel( IMushroomGarden garden, IDialogService dialogService, ISceneProvider sceneProvider,
                                       IPaletteProvider paletteProvider, IHubManager hubManager, IPreferences preferences ) {
            mDialogService = dialogService;
            mPaletteProvider = paletteProvider;
            mSceneProvider = sceneProvider;
            mHubManager = hubManager;
            mGarden = garden;
            SceneList = new ObservableCollectionExtended<GardenSceneViewModel>();

            mSceneSubscription = mGarden.ActiveScenes
                .Transform( scene => new GardenSceneViewModel( scene, garden, this, preferences ))
                .Sort( SortExpressionComparer<GardenSceneViewModel>.Ascending( s => s.Name ))
                .Bind( SceneList )
                .Subscribe();

            CreateScene = new DelegateCommand( OnCreateScene );
            StartAll = new DelegateCommand( OnStartAll );
            StopAll = new DelegateCommand( OnStopAll );
        }

        private async void OnCreateScene() {
            var lighting = await LoadGroupLighting();
            var palettes = mPaletteProvider.Entities.Items.Select( p => new PaletteViewModel( p )).ToList();
            var parameters = new DialogParameters {
                { NewSceneViewModel.cPaletteList, palettes.ToList() },
                { NewSceneViewModel.cLightingList, lighting }
            };

            mDialogService.ShowDialog<NewSceneView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var sceneName = result.Parameters.GetValue<string>( NewSceneViewModel.cSceneName ) ?? String.Empty;
                    var paletteId = result.Parameters.GetValue<string>( NewSceneViewModel.cSelectedPalette );
                    var lightId = result.Parameters.GetValue<string>( NewSceneViewModel.cSelectedLights );

                    if((!String.IsNullOrWhiteSpace( sceneName )) &&
                       (!String.IsNullOrWhiteSpace( paletteId )) &&
                       (!String.IsNullOrWhiteSpace( lightId ))) {
                        var palette = palettes.FirstOrDefault( p => p.Palette.Id.Equals( paletteId ));
                        var lightSource = lighting.FirstOrDefault( l => l.LightId.Equals( lightId ));

                        if(( palette != null ) &&
                           ( lightSource != null )) {
                            var scene = new Scene( sceneName, SceneMode.Animating, palette.Palette, ScenePalette.Default,
                                                   SceneParameters.Default, SceneControl.Default, 
                                                   new []{lightSource.LightSource }, SceneSchedule.Default );

                            mSceneProvider.Insert( scene );
                        }
                    }
                }
            });
        }

        public void StartSceneAnimated( Scene scene ) {
            scene.SetMode( SceneMode.Animating );
            mSceneProvider.Update( scene );

            mGarden.StartScene( scene );
        }

        public void StartSceneStationary( Scene scene ) {
            var parameters = new DialogParameters {
                { ColorSelectionViewModel.cColorPalette, scene.StationaryPalette.Copy() }
            };

            if( mGarden.IsSceneActive( scene )) {
                void SceneUpdate( ScenePalette palette ) => UpdateSceneColor( scene, palette );

                parameters.Add( ColorSelectionViewModel.cUpdateCallback, SceneUpdate );
            }

            mDialogService.ShowDialog<ColorSelectionView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var palette = result.Parameters.GetValue<ScenePalette>( ColorSelectionViewModel.cColorPalette );

                    if( palette != null ) {
                        scene.SetMode( SceneMode.Stationary );
                        scene.StationaryPalette.UpdateFrom( palette );

                        mSceneProvider.Update( scene );

                        mGarden.StartScene( scene );
                    }
                }
            });
        }

        private void UpdateSceneColor( Scene scene, ScenePalette palette ) {
            scene.StationaryPalette.UpdateFrom( palette );

            mSceneProvider.Update( scene );
            mGarden.UpdateSceneColors( scene );
        }

        public async void SetLighting( Scene scene ) {
            var lighting = await LoadAllLighting();
            var parameters = new DialogParameters {
                { LightingSelectorViewModel.cLightingList, lighting },
                { LightingSelectorViewModel.cSelectedLights, scene.Lights }
            };

            mDialogService.ShowDialog<LightingSelectorView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var selectedLights = result.Parameters.GetValue<IList<LightSource>?>( LightingSelectorViewModel.cSelectedLights );

                    if( selectedLights?.Any() == true ) {
                        scene.Update( selectedLights );

                        mSceneProvider.Update( scene );
                    }
                }
            });
        }

        public void SetPalette( Scene scene ) {
            var palettes = mPaletteProvider.Entities.Items.Select( p => new PaletteViewModel( p )).ToList();
            var parameters = new DialogParameters {
                { PaletteSelectorViewModel.cPaletteList, palettes.ToList() },
                { PaletteSelectorViewModel.cSelectedPalette, scene.Palette.Id }
            };

            mDialogService.ShowDialog<PaletteSelectorView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var paletteId = result.Parameters.GetValue<string>( PaletteSelectorViewModel.cSelectedPalette );

                    if(!String.IsNullOrWhiteSpace( paletteId )) {
                        var palette = palettes.FirstOrDefault( p => p.Palette.Id.Equals( paletteId ));

                        if( palette != null ) {
                            scene.Update( palette.Palette );

                            mSceneProvider.Update( scene );
                        }
                    }
                }
            });
        }

        public void SetParameters( Scene scene ) {
            var parameters = new DialogParameters {
                { AnimationParametersViewModel.cSceneParameters, scene.Parameters }
            };

            mDialogService.ShowDialog<AnimationParametersView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var sceneParameters = result.Parameters.GetValue<SceneParameters>( AnimationParametersViewModel.cSceneParameters );
                    
                    if( sceneParameters != null ) {
                        scene.Update( sceneParameters );

                        mSceneProvider.Update( scene );
                    }
                }
            });
        }

        public void SetSchedule( Scene scene ) {
            var parameters = new DialogParameters {
                { SceneScheduleViewModel.cSchedule, scene.Schedule }
            };

            mDialogService.ShowDialog<SceneScheduleView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var sceneSchedule = result.Parameters.GetValue<SceneSchedule>( SceneScheduleViewModel.cSchedule );
                    
                    if( sceneSchedule != null ) {
                        scene.Update( sceneSchedule );

                        mSceneProvider.Update( scene );
                    }
                }
            });
        }

        public void SetFavorite( Scene scene, bool state ) {
            scene.SetFavorite( state );

            mSceneProvider.Update( scene );
        }

        public void RenameScene( Scene scene ) {
            var parameters = new DialogParameters {
                { RenameViewModel.cTitle, "Scene Name" },
                { RenameViewModel.cName, scene.SceneName }
            };

            mDialogService.ShowDialog<RenameView>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    var name = result.Parameters.GetValue<string>( RenameViewModel.cName );

                    if(!String.IsNullOrWhiteSpace( name )) {
                        scene.SetName( name );

                        mSceneProvider.Update( scene );
                    }
                }
            });
        }

        public void DeleteScene( Scene scene ) {
            var parameters = new DialogParameters {
                { ConfirmationDialogViewModel.cTitle, "Confirm Deletion" },
                { ConfirmationDialogViewModel.cMessage, $"Would you like to delete scene '{scene.SceneName}'?" }
            };

            mDialogService.ShowDialog<ConfirmationDialog>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    mSceneProvider.Delete( scene );
                }
            });
        }

        private async Task<IList<LightSourceViewModel>> LoadAllLighting() {
            var bulbs = await mHubManager.GetBulbs();
            var retValue = bulbs
                .Select( bulb => new LightSourceViewModel( 
                    new LightSource( bulb.Name, GroupType.Free ), new []{ bulb }, _ => { }));

            return retValue
                .Concat( await LoadGroupLighting())
                .ToList();
        }

        private async Task<IList<LightSourceViewModel>> LoadGroupLighting() {
            var groups = await mHubManager.GetBulbGroups();

            return groups
                .OrderBy( g => g.Name )
                .Select( group =>
                    new LightSourceViewModel(
                        new LightSource( group.Name, group.GroupType ), group.Bulbs, _ => { } ))
                .ToList();
        }

        private void OnStartAll() {
            mGarden.StartAllFavorites();
        }

        private void OnStopAll() {
            mGarden.StopAllFavorites();
        }

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
