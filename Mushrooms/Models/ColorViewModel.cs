using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.ViewModelSupport;
using System;
using System.Windows.Input;
using System.Windows.Media;
using ReusableBits.Wpf.Utility;

namespace Mushrooms.Models {
    internal class ColorViewModel : PropertyChangeBase {
        private readonly Action<ColorViewModel>     mOnBulbSelectionChanged;
        private readonly Action<ColorViewModel>     mOnColorEdit;

        public  Color               SwatchColor { get; private set; }
        public  HslColor            HslColor { get; private set; }
        public  int                 Population { get; }
        public  bool                IsSelected { get; set; }

        public  string              Tooltip => $"{SwatchColor.ToString()} - Population: {Population}";

        public  DelegateCommand     SelectSwatch { get; }
        public  ICommand            EditColor { get; }

        public ColorViewModel( Color color, int population, bool isSelected, 
                               Action<ColorViewModel> onSelectionChanged,
                               Action<ColorViewModel> onColorEditRequested ) {
            mOnBulbSelectionChanged = onSelectionChanged;
            mOnColorEdit = onColorEditRequested;

            SwatchColor = color;
            HslColor = new HslColor( SwatchColor );
            Population = population;
            IsSelected = isSelected;

            SelectSwatch = new DelegateCommand( OnSelectSwatch );
            EditColor = new DelegateCommand( OnEditRequest );
        }

        public ColorViewModel( Color color, Action<ColorViewModel> onSelectionChanged, Action<ColorViewModel> onColorEditRequest ) {
            mOnBulbSelectionChanged = onSelectionChanged;
            mOnColorEdit = onColorEditRequest;

            SwatchColor = color;
            HslColor = new HslColor( SwatchColor );
            Population = 1;
            IsSelected = false;

            SelectSwatch = new DelegateCommand( OnSelectSwatch );
            EditColor = new DelegateCommand( OnEditRequest );
        }

        public void SetSelection( bool state ) {
            IsSelected = state;

            RaisePropertyChanged( () => IsSelected );
        }

        public void UpdateSwatch( Color swatch ) {
            SwatchColor = swatch;
            HslColor = new HslColor( SwatchColor );

            RaiseAllPropertiesChanged();
        }

        private void OnSelectSwatch() {
            IsSelected = !IsSelected;

            RaisePropertyChanged( () => IsSelected );
            mOnBulbSelectionChanged.Invoke( this );
        }

        private void OnEditRequest() {
            mOnColorEdit.Invoke( this );
        }
    }
}
