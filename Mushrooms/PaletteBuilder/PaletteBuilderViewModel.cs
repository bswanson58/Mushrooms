using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = System.Windows.Media.Color;

namespace Mushrooms.PaletteBuilder {
    internal class ColorSwatch : PropertyChangeBase {
        public  Color               SwatchColor { get; }
        public  bool                IsSelected { get; set; }

        public  DelegateCommand     SelectSwatch { get; }

        public ColorSwatch( Color color ) {
            SwatchColor = color;
            IsSelected = true;

            SelectSwatch = new DelegateCommand( OnSelectSwatch );
        }

        private void OnSelectSwatch() {
            IsSelected = !IsSelected;

            RaisePropertyChanged( () => IsSelected );
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PaletteBuilderViewModel : PropertyChangeBase {
        public  ObservableCollection<ColorSwatch>   Palette { get; }

        public  DelegateCommand     SelectImage { get; }
        public  ImageSource ?       PatternImage { get; private set; }

        public PaletteBuilderViewModel() {
            Palette = new ObservableCollection<ColorSwatch>();

            SelectImage = new DelegateCommand( OnSelectFile );
        }

        private void OnSelectFile() {
            var dialog = new OpenFileDialog { Filter = "Images|*.jpg", Title = "Select Image" };

            if( dialog.ShowDialog() == true ) {
                SelectImageColors( dialog.FileName );

                PatternImage = new BitmapImage( new Uri( dialog.FileName ));

                RaisePropertyChanged( () => PatternImage );
            }

        }

        private void SelectImageColors( string fileName ) {
            Palette.Clear();

            using( var image = Image.Load<Rgba32>( fileName )) {
                var colorThief = new ColorThief.ImageSharp.ColorThief();
                var palette = colorThief.GetPalette( image, 25, 5 );

                foreach( var color in palette.OrderByDescending( c => c.Population )) {
                    Palette.Add( new ColorSwatch( Color.FromRgb( color.Color.R, color.Color.G, color.Color.B )));
                }
            }
        }
    }
}
