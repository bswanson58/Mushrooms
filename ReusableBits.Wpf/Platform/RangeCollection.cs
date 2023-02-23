using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

// from: https://stackoverflow.com/questions/13302933/how-to-avoid-firing-observablecollection-collectionchanged-multiple-times-when-r

namespace ReusableBits.Wpf.Platform {
    public class RangeCollection<T> : ObservableCollection<T> {
        public RangeCollection() { }

        public RangeCollection( IEnumerable<T> collection ) :
            base( collection ) {
        }

        public RangeCollection( List<T> list ) :
            base( list ) {
        }

        public void AddRange( IEnumerable<T> range ) {
            foreach( var item in range ) {
                Items.Add( item );
            }

            OnPropertyChanged( new PropertyChangedEventArgs( "Count" ) );
            OnPropertyChanged( new PropertyChangedEventArgs( "Item[]" ) );
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ));
        }

        public void Reset( IEnumerable<T> range ) {
            Items.Clear();

            AddRange( range );
        }
    }
}
