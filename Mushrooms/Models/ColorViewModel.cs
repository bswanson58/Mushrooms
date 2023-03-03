using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System;
using System.Windows.Media;

namespace Mushrooms.Models {
    internal class ColorViewModel : PropertyChangeBase {
        private readonly Action<ColorViewModel>    mOnBulbSelectionChanged;

        public  Color               SwatchColor { get; }
        public  bool                IsSelected { get; set; }

        public  DelegateCommand     SelectSwatch { get; }

        public ColorViewModel( Color color, Action<ColorViewModel> onSelectionChanged ) {
            mOnBulbSelectionChanged = onSelectionChanged;

            SwatchColor = color;
            IsSelected = true;

            SelectSwatch = new DelegateCommand( OnSelectSwatch );
        }

        private void OnSelectSwatch() {
            IsSelected = !IsSelected;

            RaisePropertyChanged( () => IsSelected );
            mOnBulbSelectionChanged.Invoke( this );
        }
    }
}
