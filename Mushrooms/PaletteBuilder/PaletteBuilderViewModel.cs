using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using Microsoft.Win32;
using Mushrooms.Database;
using Mushrooms.Dialogs;
using Mushrooms.Entities;
using Mushrooms.Models;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Windows.Media.Color;

namespace Mushrooms.PaletteBuilder {
    internal class EditablePaletteViewModel : PaletteViewModel {
        private readonly Action<EditablePaletteViewModel> mOnDelete;

        public  ICommand        Delete { get; }

        public EditablePaletteViewModel( ScenePalette palette, Action<EditablePaletteViewModel> onDelete )
            : base( palette ) {
            Delete = new DelegateCommand( OnDelete );
            mOnDelete = onDelete;
        }

        private void OnDelete() {
            mOnDelete.Invoke( this );
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PaletteBuilderViewModel : PropertyChangeBase, IDisposable {
        private readonly IPaletteProvider               mPaletteProvider;
        private readonly IDialogService                 mDialogService;

        private string                                  mPaletteName;
        private IDisposable ?                           mPaletteSubscription;

        public  ObservableCollectionExtended<EditablePaletteViewModel>  Palettes { get; }
        public  ObservableCollection<ColorViewModel>    SwatchList { get; }

        public  DelegateCommand                         SavePalette { get; }
        public  DelegateCommand                         SelectImage { get; }
        public  ImageSource ?                           PatternImage { get; private set; }

        public PaletteBuilderViewModel( IPaletteProvider paletteProvider, IDialogService dialogService ) {
            mPaletteProvider = paletteProvider;
            mDialogService = dialogService;
            mPaletteName = String.Empty;

            Palettes = new ObservableCollectionExtended<EditablePaletteViewModel>();

            mPaletteSubscription = mPaletteProvider.Entities
                .Connect()
                .Transform( p => new EditablePaletteViewModel( p, OnDeletePalette ))
                .Bind( Palettes )
                .Subscribe();

            SwatchList = new ObservableCollection<ColorViewModel>();

            SavePalette = new DelegateCommand( OnSavePalette );
            SelectImage = new DelegateCommand( OnSelectFile );
        }

        public string PaletteName {
            get => mPaletteName;
            set => mPaletteName = value;
        }

        private void OnSelectFile() {
            var dialog = new OpenFileDialog { Filter = "Images|*.jpg", Title = "Select Image" };

            if( dialog.ShowDialog() == true ) {
                SelectImageColors( dialog.FileName );
                UpdatePaletteState();

                PatternImage = new BitmapImage( new Uri( dialog.FileName ));

                RaisePropertyChanged( () => PatternImage );
            }
        }

        private void OnSwatchSelectionChanged( ColorViewModel _ ) => 
            UpdatePaletteState();

        private void UpdatePaletteState() {}
//            mSceneFacade.SetScenePalette(
//                new ScenePalette( from swatch in Palette where swatch.IsSelected select swatch.SwatchColor ));

        private void SelectImageColors( string fileName ) {
            SwatchList.Clear();

            using( var image = Image.Load<Rgba32>( fileName )) {
                var colorThief = new ColorThief.ImageSharp.ColorThief();
                var palette = colorThief.GetPalette( image, 25, 5 );
                var swatchLimit = 10;

                foreach( var color in palette.OrderByDescending( c => c.Population )) {
                    SwatchList.Add( 
                        new ColorViewModel( Color.FromRgb( color.Color.R, color.Color.G, color.Color.B ), 
                                            swatchLimit > 0,
                                            OnSwatchSelectionChanged ));
                    swatchLimit--;
                }
            }
        }

        private void OnDeletePalette( EditablePaletteViewModel palette ) {
            var parameters = new DialogParameters{
                { ConfirmationDialogViewModel.cTitle, "Confirm Deletion" },
                { ConfirmationDialogViewModel.cMessage, $"Would you like to delete palette named: '{palette.Palette.Name}'?" }};

            mDialogService.ShowDialog<ConfirmationDialog>( parameters, result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    mPaletteProvider.Delete( palette.Palette );
                }
            });
        }

        private void OnSavePalette() {
            if((!String.IsNullOrWhiteSpace( mPaletteName )) &&
               ( SwatchList.Any( p => p.IsSelected ))) {
                var palette = new ScenePalette( 
                    from swatch in SwatchList where swatch.IsSelected select swatch.SwatchColor,
                    mPaletteName );

                mPaletteProvider.Insert( palette );
            }
        }

        public void Dispose() {
            mPaletteSubscription?.Dispose();
            mPaletteSubscription = null;
        }
    }
}
