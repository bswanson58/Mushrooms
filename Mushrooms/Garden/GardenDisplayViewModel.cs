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
using Mushrooms.SceneBuilder;
using Mushrooms.Services;
using Q42.HueApi.Models.Groups;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {
    internal interface ISceneCommands {
        void    SetLighting( Scene scene );
        void    SetPalette( Scene scene );
        void    SetParameters( Scene scene );
        void    SetSchedule( Scene scene );

        void    DeleteScene( Scene scene );
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GardenDisplayViewModel : PropertyChangeBase, ISceneCommands, IDisposable {
        private readonly IHubManager        mHubManager;
        private readonly IPaletteProvider   mPaletteProvider;
        private readonly ISceneProvider     mSceneProvider;
        private readonly IDialogService     mDialogService;
        private IDisposable ?               mSceneSubscription;

        public  ObservableCollectionExtended<GardenSceneViewModel> SceneList { get; }

        public  ICommand                    CreateScene { get; }

        public GardenDisplayViewModel( IMushroomGarden garden, IDialogService dialogService, ISceneProvider sceneProvider,
                                       IPaletteProvider paletteProvider, IHubManager hubManager ) {
            mDialogService = dialogService;
            mPaletteProvider = paletteProvider;
            mSceneProvider = sceneProvider;
            mHubManager = hubManager;
            SceneList = new ObservableCollectionExtended<GardenSceneViewModel>();

            mSceneSubscription = garden.ActiveScenes
                .Transform( scene => new GardenSceneViewModel( scene, garden, this ))
                .Sort( SortExpressionComparer<GardenSceneViewModel>.Ascending( s => s.Name ))
                .Bind( SceneList )
                .Subscribe();

            CreateScene = new DelegateCommand( OnCreateScene );
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
                            var scene = new Scene( sceneName, palette.Palette, SceneParameters.Default, SceneControl.Default, 
                                                   new []{lightSource.LightSource }, SceneSchedule.Default );

                            mSceneProvider.Insert( scene );
                        }
                    }
                }
            });
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

        public void Dispose() {
            mSceneSubscription?.Dispose();
            mSceneSubscription = null;
        }
    }
}
