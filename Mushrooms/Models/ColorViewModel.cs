using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System;
using System.Windows.Media;
using ReusableBits.Wpf.Utility;

namespace Mushrooms.Models {
    internal class ColorViewModel : PropertyChangeBase {
        private readonly Action<ColorViewModel>    mOnBulbSelectionChanged;

        public  Color               SwatchColor { get; }
        public  HslColor            HslColor { get; }
        public  int                 Population { get; }
        public  bool                IsSelected { get; set; }

        public  string              Tooltip => $"{SwatchColor.ToString()} - Population: {Population}";

        public  DelegateCommand     SelectSwatch { get; }

        public ColorViewModel( Color color, int population, bool isSelected, Action<ColorViewModel> onSelectionChanged ) {
            mOnBulbSelectionChanged = onSelectionChanged;

            SwatchColor = color;
            HslColor = new HslColor( SwatchColor );
            Population = population;
            IsSelected = isSelected;

            SelectSwatch = new DelegateCommand( OnSelectSwatch );
        }

        public ColorViewModel( Color color, Action<ColorViewModel> onSelectionChanged ) {
            mOnBulbSelectionChanged = onSelectionChanged;

            SwatchColor = color;
            HslColor = new HslColor( SwatchColor );
            Population = 1;
            IsSelected = false;

            SelectSwatch = new DelegateCommand( OnSelectSwatch );
        }

        public void SetSelection( bool state ) {
            IsSelected = state;

            RaisePropertyChanged( () => IsSelected );
        }

        private void OnSelectSwatch() {
            IsSelected = !IsSelected;

            RaisePropertyChanged( () => IsSelected );
            mOnBulbSelectionChanged.Invoke( this );
        }
    }
}
