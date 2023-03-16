using System;
using HueLighting.Models;
using System.Collections.Generic;
using Mushrooms.Entities;
using ReusableBits.Wpf.ViewModelSupport;
using System.Windows.Input;
using ReusableBits.Wpf.Commands;

namespace Mushrooms.Models {
    internal class LightSourceViewModel : PropertyChangeBase {
        private readonly Action<LightSourceViewModel>   mOnSelect;
        private bool            mSelected;

        public  LightSource     LightSource { get; }
        public  IList<Bulb>     Bulbs { get; }

        public  ICommand        ToggleSelection { get; }

        public  string          LightId => $"{LightSource.SourceName}-{LightSource.SourceType}";
        public  string          Name => LightSource.SourceName;

        public LightSourceViewModel( LightSource lightSource, IEnumerable<Bulb> bulbs, 
                                     Action<LightSourceViewModel> onSelect ) {
            LightSource = lightSource;
            Bulbs = new List<Bulb>( bulbs );
            mOnSelect = onSelect;

            mSelected = false;

            ToggleSelection = new DelegateCommand( OnToggleSelection );
        }

        private void OnToggleSelection() => IsSelected = !IsSelected;

        public bool IsSelected {
            get => mSelected;
            set {
                mSelected = value;

                mOnSelect.Invoke( this );
                RaisePropertyChanged( () => IsSelected );
            }
        }
    }
}
