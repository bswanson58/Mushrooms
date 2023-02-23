using System.Collections.Generic;
using System;
using System.Linq;

namespace Mushrooms.Support {
    public static class ListEx {
        private static readonly Random RandomSource = new();

        public static T Random<T>( this IList<T> list ) {
            if( !list.Any() ) {
                throw new ArgumentNullException( nameof( list ));
            }

            return list[RandomSource.Next( 0, list.Count )];
        }

        public static List<T> Randomize<T>( this IList<T> list ) =>
            list.OrderBy( _ => Guid.NewGuid() ).ToList();
    }
}
