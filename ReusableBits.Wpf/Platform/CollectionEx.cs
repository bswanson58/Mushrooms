using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ReusableBits.Wpf.Platform {
    public static class CollectionExtensions {
        //
        // Summary:
        //     Add a range of items to a collection.
        //
        // Parameters:
        //   collection:
        //     The collection to add items to.
        //
        //   items:
        //     The items to add to the collection.
        //
        // Type parameters:
        //   T:
        //     Type of objects within the collection.
        //
        // Returns:
        //     The collection.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     An System.ArgumentNullException is thrown if collection or items is null.
        public static Collection<T> AddRange<T>( this Collection<T> collection, IEnumerable<T> items ) {
            if( collection == null ) {
                throw new ArgumentNullException( nameof( collection ));
            }

            if( items == null ) {
                throw new ArgumentNullException( nameof( items ));
            }

            foreach( T item in items ) {
                collection.Add( item );
            }

            return collection;
        }
    }}