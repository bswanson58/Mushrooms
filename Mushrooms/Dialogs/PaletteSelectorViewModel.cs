using System;
using Mushrooms.Models;
using ReusableBits.Wpf.DialogService;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReusableBits.Wpf.Platform;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PaletteSelectorViewModel : DialogAwareBase {
        public  const string                cPaletteList = "palette list";
        public  const string                cSelectedPalette = "selected palette";

        private PaletteViewModel ?          mSelectedPalette;

        public  ObservableCollection<PaletteViewModel>  Palettes { get; }

        public PaletteSelectorViewModel() {
            Palettes = new ObservableCollection<PaletteViewModel>();

            Title = "Scene Palettes";
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            var paletteList = parameters.GetValue<IList<PaletteViewModel>>( cPaletteList );
            var selectedPalette = parameters.GetValue<string>( cSelectedPalette ) ?? String.Empty;

            if( paletteList?.Any() == true ) {
                Palettes.Clear();
                Palettes.AddRange( paletteList.OrderBy( p => p.Name ));

                SelectedPalette = Palettes.FirstOrDefault( p => p.PaletteId.Equals( selectedPalette ));
            }

            RaiseAllPropertiesChanged();
        }

        public PaletteViewModel ? SelectedPalette {
            get => mSelectedPalette;
            set {
                mSelectedPalette = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        protected override bool CanAccept() =>
            mSelectedPalette != null;

        protected override DialogParameters CreateClosingParameters() =>
            new() {
                { cSelectedPalette, mSelectedPalette?.PaletteId! }
            };
    }
}
