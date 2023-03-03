using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Mushrooms.Database;

namespace Mushrooms.Entities {
    internal class ScenePalette :EntityBase {
        // protected sets are for LiteDB.
        public  IList<Color>    Palette { get; protected set; }
        public  string          Name { get; protected set; }

        private ScenePalette() :
            this( Enumerable.Empty<Color>(), String.Empty ) { }

        public ScenePalette( IEnumerable<Color> colors, string name ) {
            Name = name;
            Palette = new List<Color>( colors );
        }

        public static ScenePalette Default =>
            new ();
    }
}
