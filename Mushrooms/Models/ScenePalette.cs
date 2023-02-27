using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Mushrooms.Models {
    internal record ScenePalette {
        public  IReadOnlyList<Color>    Palette { get; }

        public ScenePalette( IEnumerable<Color> colors ) =>
            Palette = new List<Color>( colors );

        public static ScenePalette Default =>
            new ( Enumerable.Empty<Color>());
    }
}
