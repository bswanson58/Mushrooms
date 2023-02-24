using System;
using System.Collections.Generic;
using Mushrooms.Models;

namespace Mushrooms.ColorAnimation {
    public class ColorAnimationSequence {
        private readonly ColorAnimationPattern  mAnimationPattern;
        private readonly ITraversalStrategy     mTraversalStrategy;
        private readonly IEasingStrategy        mEasingStrategy;
        private DateTime                        mStartTime;

        public IList<string>                    Bulbs { get; }
        public bool                             CouldTransition { get; private set; }

        public ColorAnimationSequence( IEnumerable<string> forBulbs, ColorAnimationPattern animationPattern,
                                       ITraversalStrategy traversalStrategy, IEasingStrategy easingStrategy ) {
            mAnimationPattern = animationPattern;
            Bulbs = new List<string>( forBulbs );
            mTraversalStrategy = traversalStrategy;
            mEasingStrategy = easingStrategy;
        }

        public ColorAnimationResult RunAnimationStep( ColorAnimationParameters parameters ) {
            if( mStartTime == DateTime.MinValue ) {
                mStartTime = DateTime.Now;
            }

            var duration = mAnimationPattern.Duration * parameters.RateMultiplier;
            var step = mTraversalStrategy.GetNextStep( mStartTime, duration );
            var scaledStep = 1.0D / duration.TotalMilliseconds * step;
            var eased = mEasingStrategy.GetEasedValue( scaledStep );

            var color = mAnimationPattern.Gradient.ColorAt( eased );

            CouldTransition = mTraversalStrategy.CouldTransition( mStartTime, duration );

            return new ColorAnimationResult( color, parameters.Brightness, Bulbs );
        }
    }
}
