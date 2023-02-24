using System;

namespace Mushrooms.Models {
    public enum TraversalStyle {
        Looping,
        Reversal,
        SinglePass
    }

    public interface ITraversalStrategy {
        double  GetNextStep( DateTime startTime, TimeSpan duration );
        bool    CouldTransition( DateTime startTime, TimeSpan duration );
    }

    internal class LoopingTraversalStrategy : ITraversalStrategy {
        public double GetNextStep( DateTime startTime, TimeSpan duration ) {
            var interval = ( DateTime.Now - startTime );

            return interval.TotalMilliseconds % duration.TotalMilliseconds;
        }

        public bool CouldTransition( DateTime startTime, TimeSpan duration ) {
            var interval = ( DateTime.Now - startTime );
            var hasCompletedOnce = interval > duration;

            return hasCompletedOnce && ( interval.TotalMilliseconds % duration.TotalMilliseconds ) < 250;
        }
    }

    internal class ReversalTraversalStrategy : ITraversalStrategy {
        public double GetNextStep( DateTime startTime, TimeSpan duration ) {
            var interval = ( DateTime.Now - startTime );
            var twoSteps = interval.TotalMilliseconds % ( duration.TotalMilliseconds * 2.0 );

            return twoSteps < duration.TotalMilliseconds ?
                twoSteps :
                ( duration.TotalMilliseconds * 2.0 ) - twoSteps;
        }

        public bool CouldTransition( DateTime startTime, TimeSpan duration ) {
            var interval = ( DateTime.Now - startTime );
            var hasReversed = interval > ( duration * 2 );

            return hasReversed && ( interval.TotalMilliseconds % duration.TotalMilliseconds ) < 250;
        }
    }

    internal class SinglePassTraversalStrategy : ITraversalStrategy {
        public double GetNextStep( DateTime startTime, TimeSpan duration ) {
            if( startTime + duration > DateTime.Now ) {
                return 1.0D;
            }

            var interval = ( DateTime.Now - startTime );

            return interval.TotalMilliseconds % duration.TotalMilliseconds;
        }

        public bool CouldTransition( DateTime startTime, TimeSpan duration ) =>
            ( startTime + duration ) > DateTime.Now;
    }
}
