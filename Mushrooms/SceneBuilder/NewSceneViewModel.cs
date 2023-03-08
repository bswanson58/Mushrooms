using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Mushrooms.Models;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.Platform;

namespace Mushrooms.SceneBuilder {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NewSceneViewModel : DialogAwareBase {
        public  const string                cLightingList = "lighting list";
        public  const string                cSelectedLights = "selected light group";
        public  const string                cPaletteList = "palette list";
        public  const string                cSelectedPalette = "selected palette";
        public  const string                cSceneName = "scene name";

        private PaletteViewModel ?          mSelectedPalette;
        private LightSourceViewModel ?      mSelectedLights;
        private string                      mSceneName;

        public  ObservableCollection<PaletteViewModel>      Palettes { get; }
        public  ObservableCollection<LightSourceViewModel>  Lighting { get; }

        public NewSceneViewModel() {
            Palettes = new ObservableCollection<PaletteViewModel>();
            Lighting = new ObservableCollection<LightSourceViewModel>();

            mSceneName = String.Empty;
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            var lightingList = parameters.GetValue<IList<LightSourceViewModel>>( cLightingList );

            if( lightingList?.Any() == true) {
                Lighting.AddRange( lightingList.OrderBy( l => l.Name ));

                SelectedLighting = Lighting.FirstOrDefault();
            }

            var paletteList = parameters.GetValue<IList<PaletteViewModel>>( cPaletteList );

            if( paletteList?.Any() == true ) {
                Palettes.AddRange( paletteList.OrderBy( p => p.Name ));

                SelectedPalette = Palettes.FirstOrDefault();
            }

            mSceneName = String.Empty;

            RaiseAllPropertiesChanged();
        }

        public string SceneName {
            get => mSceneName;
            set {
                mSceneName = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public PaletteViewModel ? SelectedPalette {
            get => mSelectedPalette;
            set {
                mSelectedPalette = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public LightSourceViewModel ? SelectedLighting {
            get => mSelectedLights;
            set {
                mSelectedLights = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        protected override bool CanAccept() =>
            !String.IsNullOrWhiteSpace( mSceneName ) &&
            mSelectedLights != null &&
            mSelectedPalette != null;

        protected override DialogParameters CreateClosingParameters() =>
            new() {
                { cSceneName, mSceneName },
                { cSelectedLights, mSelectedLights?.LightId! },
                { cSelectedPalette, mSelectedPalette?.PaletteId! }
            };
    }
}
