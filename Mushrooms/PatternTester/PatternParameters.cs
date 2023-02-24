using System;
using Mushrooms.Models;

namespace Mushrooms.PatternTester {
    public record PatternParameters {
        public  TraversalStyle  TraversalStyle { get; init; }
        public  EasingStyle     EasingStyle { get; init; }
        public  TimeSpan        Duration { get; init; }
        public  TimeSpan        DurationJitter { get; init; }
        public  int             PatternCount { get; init; }

        public static PatternParameters DefaultParameters => 
            new PatternParameters {
                TraversalStyle = TraversalStyle.Reversal, 
                EasingStyle = EasingStyle.Linear,
                Duration = TimeSpan.FromSeconds( 30 ),
                DurationJitter = TimeSpan.FromSeconds( 5 ),
                PatternCount = 5
            };
    }
}
