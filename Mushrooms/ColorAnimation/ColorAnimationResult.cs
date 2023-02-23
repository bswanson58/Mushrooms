using System.Collections.Generic;
using System.Windows.Media;

namespace Mushrooms.ColorAnimation {
    public class ColorAnimationResult {
        public List<string> Bulbs { get; }
        public Color Color { get; }
        public double Brightness { get; }

        public ColorAnimationResult() {
            Color = Colors.Black;
            Brightness = 1.0D;
            Bulbs = new List<string>();
        }

        public ColorAnimationResult( Color color, double brightness, IList<string> bulbs ) {
            Color = color;
            Brightness = brightness;
            Bulbs = new List<string>( bulbs );
        }
    }
}
