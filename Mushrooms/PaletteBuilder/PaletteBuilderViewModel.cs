﻿using System;
using System.Collections.Generic;
using System.IO;
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
using ReusableBits.Wpf.Utility;
using ReusableBits.Wpf.ViewModelSupport;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Windows.Media.Color;

namespace Mushrooms.PaletteBuilder {
    internal enum ColorByProperty {
        Population,
        Hue,
        Saturation,
        Luminosity
    }

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
        private readonly IPaletteProvider       mPaletteProvider;
        private readonly IPictureCache          mPictureCache;
        private readonly IDialogService         mDialogService;
        private readonly List<ColorViewModel>   mSwatchList;

        private EditablePaletteViewModel ?      mSelectedPalette;
        private string                          mPaletteName;
        private string                          mPictureFile;
        private bool                            mDisplayOnlySelectedSwatches;
        private ColorByProperty                 mColorOrder;
        private IDisposable ?                   mPaletteSubscription;

        public  ObservableCollectionExtended<EditablePaletteViewModel>  Palettes { get; }
        public  IList<ColorByProperty>          ColorOrderList { get; }
        public  int                             SelectedSwatchCount => mSwatchList.Count( s => s.IsSelected );

        public  DelegateCommand                 NewPalette { get; }
        public  DelegateCommand                 SavePalette { get; }
        public  DelegateCommand                 SelectImage { get; }
        public  ImageSource ?                   PatternImage { get; private set; }

        public PaletteBuilderViewModel( IPaletteProvider paletteProvider, IPictureCache pictureCache,
                                        IDialogService dialogService ) {
            mPaletteProvider = paletteProvider;
            mPictureCache = pictureCache;
            mDialogService = dialogService;
            mPaletteName = String.Empty;
            mPictureFile = String.Empty;
            mColorOrder = ColorByProperty.Population;
            mDisplayOnlySelectedSwatches = false;
            mSwatchList = new List<ColorViewModel>();

            Palettes = new ObservableCollectionExtended<EditablePaletteViewModel>();

            ColorOrderList = new List<ColorByProperty> {
                ColorByProperty.Population, 
                ColorByProperty.Hue, 
                ColorByProperty.Saturation, 
                ColorByProperty.Luminosity
            };

            mPaletteSubscription = mPaletteProvider.Entities
                .Connect()
                .Transform( p => new EditablePaletteViewModel( p, OnDeletePalette ))
                .Sort( SortExpressionComparer<EditablePaletteViewModel>.Ascending( p => p.Name ))
                .Bind( Palettes )
                .Subscribe();

            NewPalette = new DelegateCommand( OnNewPalette );
            SavePalette = new DelegateCommand( OnSavePalette, CanSavePalette );
            SelectImage = new DelegateCommand( OnSelectFile );
        }

        public EditablePaletteViewModel ? SelectedPalette {
            get => mSelectedPalette;
            set {
                mSelectedPalette = value;

                SelectPalette();
            }
        }

        public ColorByProperty ColorBy {
            get => mColorOrder;
            set {
                mColorOrder = value;

                UpdateColorSwatches();
            }
        }

        private void SelectPalette() {
            if( mSelectedPalette != null ) {
                SelectImageFile( mPictureCache.GetPalettePictureFile( mSelectedPalette.Palette ));

                foreach( var swatch in mSwatchList ) {
                    swatch.IsSelected = mSelectedPalette.Palette.Palette.Contains( swatch.SwatchColor );
                }

                mPaletteName = mSelectedPalette.Name;

                RaiseAllPropertiesChanged();
            }
        }

        public IEnumerable<ColorViewModel> SwatchList =>
            mDisplayOnlySelectedSwatches ?
                mSwatchList.OrderByDescending( OrderByProperty ).Where( s => s.IsSelected ) :
                mSwatchList.OrderByDescending( OrderByProperty );

        private double OrderByProperty( ColorViewModel color ) {
            switch ( mColorOrder ) {
                default:
                case ColorByProperty.Population:
                    return color.Population;

                case ColorByProperty.Hue:
                    return -color.HslColor.H;

                case ColorByProperty.Saturation:
                    return color.HslColor.S;

                case ColorByProperty.Luminosity:
                    return color.HslColor.L;
            }
        }

        public string PaletteName {
            get => mPaletteName;
            set {
                mPaletteName = value;

                SavePalette.RaiseCanExecuteChanged();
            }
        }

        public bool DisplayOnlySelected {
            get => mDisplayOnlySelectedSwatches;
            set {
                mDisplayOnlySelectedSwatches = value;

                RaisePropertyChanged( () => SwatchList );
            }
        }

        private void OnNewPalette() {
            mDialogService.ShowDialog<NewPaletteView>( result => {
                if( result.Result.Equals( ButtonResult.Ok )) {
                    mPaletteName = result.Parameters.GetValue<string>( NewPaletteViewModel.cPaletteName ) ?? String.Empty;
                    SelectImageFile( result.Parameters.GetValue<string>( NewPaletteViewModel.cImageFile ) ?? String.Empty );
                }
            });
        }

        private void UpdateAllProperties() {
            UpdateColorSwatches();
            RaiseAllPropertiesChanged();
            SavePalette.RaiseCanExecuteChanged();
        }

        private void UpdateColorSwatches() {
            // force the collection source to change since it is not an observable collection
            mDisplayOnlySelectedSwatches = !mDisplayOnlySelectedSwatches;
            RaisePropertyChanged( () => SwatchList );
            mDisplayOnlySelectedSwatches = !mDisplayOnlySelectedSwatches;
            RaisePropertyChanged( () => SwatchList );
        }

        private void OnSelectFile() {
            var dialog = new OpenFileDialog { Filter = "Images|*.jpg;*.png;", Title = "Select Image" };

            if( dialog.ShowDialog() == true ) {
                SelectImageFile( dialog.FileName );
            }
        }

        private void SelectImageFile( string fileName ) {
            SelectImageColors( fileName );

            PatternImage = new BitmapImage( new Uri( fileName ));
            mPictureFile = fileName;

            UpdateAllProperties();
        }

        private void OnSwatchSelectionChanged( ColorViewModel _ ) { 
            RaisePropertyChanged( () => SwatchList );
            RaisePropertyChanged( () => SelectedSwatchCount );
        }


        private void SelectImageColors( string fileName ) {
            if( File.Exists( fileName )) {
                mSwatchList.Clear();

                using( var image = Image.Load<Rgba32>( fileName )) {
                    var colorThief = new ColorThief.ImageSharp.ColorThief();
                    var palette = colorThief.GetPalette( image, 25, 1 );
                    var swatchLimit = 10;

                    foreach( var color in palette.Where( c => c.Population > 100 ).OrderByDescending( c => c.Population )) {
                        var hsl = new HslColor( color.Color.A, color.Color.R, color.Color.G, color.Color.B );

                        mSwatchList.Add( 
                            new ColorViewModel( Color.FromRgb( color.Color.R, color.Color.G, color.Color.B ),
                                hsl, color.Population,
                                swatchLimit > 0,
                                OnSwatchSelectionChanged ));
                        swatchLimit--;
                    }
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

                    mPictureCache.DeletePicture( palette.Palette );
                }
            });

            if( palette.Equals( mSelectedPalette )) {
                mSwatchList.Clear();
                PatternImage = null;
                mPaletteName = String.Empty;
                mPictureFile = String.Empty;
            }
        }

        private void OnSavePalette() {
            if((!String.IsNullOrWhiteSpace( mPaletteName )) &&
               ( SwatchList.Any( p => p.IsSelected ))) {
                if( mSelectedPalette != null ) {
                    UpdatePalette( mSelectedPalette.Palette );
                }
                else {
                    CreatePalette();
                }
            }
        }

        private void CreatePalette() {
            var palette = new ScenePalette(
                from swatch in mSwatchList select swatch.SwatchColor,
                from swatch in mSwatchList where swatch.IsSelected select swatch.SwatchColor,
                mPaletteName );

            mPaletteProvider.Insert( palette );
            mPictureCache.SavePicture( palette, mPictureFile );
        }

        private void UpdatePalette( ScenePalette currentPalette ) {
            currentPalette.UpdateFrom( 
                new ScenePalette(
                    from swatch in mSwatchList select swatch.SwatchColor,
                    from swatch in mSwatchList where swatch.IsSelected select swatch.SwatchColor,
                    mPaletteName ));

            mPaletteProvider.Update( currentPalette );
        }

        private bool CanSavePalette() =>
            !String.IsNullOrWhiteSpace( PaletteName ) &&
            mSwatchList.Any( s => s.IsSelected );

        public void Dispose() {
            mPaletteSubscription?.Dispose();
            mPaletteSubscription = null;
        }
    }
}
