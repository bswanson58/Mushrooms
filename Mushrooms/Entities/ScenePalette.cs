using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Mushrooms.Database;

namespace Mushrooms.Entities {
    internal class ScenePalette :EntityBase {
        // protected sets are for LiteDB.
        public  IList<Color>    SourceColors { get; protected set; }
        public  IList<Color>    Palette { get; protected set; }
        public  string          Name { get; protected set; }

        private ScenePalette() :
            this( Enumerable.Empty<Color>(), Enumerable.Empty<Color>(), String.Empty ) { }

        public ScenePalette( IEnumerable<Color> sourceColors, IEnumerable<Color> colors, string name ) {
            Name = name;
            SourceColors = new List<Color>( sourceColors );
            Palette = new List<Color>( colors );
        }

        public void UpdateFrom( ScenePalette palette ) {
            SourceColors = new List<Color>( palette.SourceColors );
            Palette = new List<Color>( palette.Palette );
            Name = palette.Name;
        }

        public static ScenePalette Default =>
            new ();
    }
}
