using System;
using Mushrooms.Models;

namespace Mushrooms.ColorAnimation {
    public class ColorAnimationPattern {
        public MultiGradient    Gradient { get; }
        public TimeSpan         Duration { get; }
        public TraversalStyle   TraversalStyle { get; }
        public EasingStyle      EasingStyle { get; }

        public ColorAnimationPattern( MultiGradient gradient, TimeSpan duration, TraversalStyle traversalStyle ) {
            Gradient = gradient;
            Duration = duration;
            TraversalStyle = traversalStyle;
            EasingStyle = EasingStyle.Linear;
        }

        public ColorAnimationPattern( MultiGradient gradient, TimeSpan duration,
                                      TraversalStyle traversalStyle, EasingStyle easingStyle ) {
            Gradient = gradient;
            Duration = duration;
            TraversalStyle = traversalStyle;
            EasingStyle = easingStyle;
        }
    }
}
