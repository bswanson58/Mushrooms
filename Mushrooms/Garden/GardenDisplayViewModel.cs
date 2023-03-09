using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using HueLighting.Hub;
using Mushrooms.Database;
using Mushrooms.Entities;
using Mushrooms.Models;
using Mushrooms.SceneBuilder;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Garden {

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GardenDisplayViewModel : PropertyChangeBase, IDisposable {
        private readonly IMushroomGarden    mGarden;
        private readonly IHubManager        mHubManager;
        private readonly IPaletteProvider   mPaletteProvider;
        private readonly ISceneProvider     mSceneProvider;
        private readonly IDialogService     mDialogService;
        private IDisposable ?               mSceneSubscription;

        public  ObservableCollectionExtended<GardenSceneViewModel> SceneList { get; }

        public  ICommand        QuickScene { get; }

        public GardenDisplayViewModel( IMushroomGarden garden, IDialogService dialogService, ISceneProvider sceneProvider,
                                       IPaletteProvider paletteProvider, IHubManager hubManager ) {
            mGarden = garden;
            mDialogService = dialogService;
            mPaletteProvider = paletteProvider;
            mSceneProvider = sceneProvider;
            mHubManager = hubManager;
            SceneList = new ObservableCollectionExtended<GardenSceneViewModel>();

            mSceneSubscription = mGarden.ActiveScenes
                .Transform( scene => new GardenSceneViewModel( scene, garden ))
                .Sort( SortExpressionComparer<GardenSceneViewModel>.Ascending( s => s.Name ))
                .Bind( SceneList )
                .Subscribe();

            QuickScene = new DelegateCommand( OnQuickScene );
        }

        private async void OnQuickScene() {
            var lighting = await LoadLighting();
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

        private async Task<IList<LightSourceViewModel>> LoadLighting() {
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
