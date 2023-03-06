using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.PaletteBuilder {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NewPaletteViewModel : DialogAwareBase {
        public const string     cPaletteName = "palette name";
        public const string     cImageFile = "image file";

        private string          mPaletteName;
        private string          mImageFile;

        public  ICommand        SelectFile { get; }
        
        public NewPaletteViewModel() {
            mPaletteName = String.Empty;
            mImageFile = String.Empty;

            SelectFile = new DelegateCommand( OnSelectFile );

            Title = "New Palette Properties";
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            mPaletteName = String.Empty;
            mImageFile = String.Empty;

            RaiseAllPropertiesChanged();
        }

        public string PaletteName {
            get => mPaletteName;
            set {
                mPaletteName = value;

                Ok.RaiseCanExecuteChanged();
            }
        }

        public string ImageFile {
            get => mImageFile;
            set {
                mImageFile = value;

                RaisePropertyChanged( () => ImageFile );
                Ok.RaiseCanExecuteChanged();
            }
        }

        private void OnSelectFile() {
            var dialog = new OpenFileDialog { Filter = "Images|*.jpg", Title = "Select Image" };

            if( dialog.ShowDialog() == true ) {
                ImageFile = dialog.FileName;
            }
        }

        protected override DialogParameters CreateClosingParameters() =>
            new() {
                { cPaletteName, mPaletteName },
                { cImageFile, mImageFile }
            };

        protected override bool CanAccept() =>
            !String.IsNullOrWhiteSpace( PaletteName ) &&
            !String.IsNullOrWhiteSpace( ImageFile ) &&
            File.Exists( ImageFile );
    }
}
