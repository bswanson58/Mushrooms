using Mushrooms.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Mushrooms.Models {
    internal class PaletteViewModel {
        public  ScenePalette        Palette { get; }

        public  string              Name => Palette.Name;
        public  IEnumerable<Color>  ExampleColors => Palette.Palette.Take( 7 );

        public PaletteViewModel( ScenePalette palette ) {
            Palette = palette;
        }
    }
}
