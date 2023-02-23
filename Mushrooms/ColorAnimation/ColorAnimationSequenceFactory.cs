using System;
using System.Collections.Generic;
using Mushrooms.Models;

namespace Mushrooms.ColorAnimation {
    public interface ISequenceFactory {
        ColorAnimationSequence Create( IEnumerable<string> forBulbs, ColorAnimationPattern animationPattern );
    }

    public class ColorAnimationSequenceFactory : ISequenceFactory {
        public ColorAnimationSequence Create( IEnumerable<string> forBulbs, ColorAnimationPattern animationPattern ) {
            ITraversalStrategy traversalStrategy;

            switch( animationPattern.TraversalStyle ) {
                case TraversalStyle.Looping:
                    traversalStrategy = new LoopingTraversalStrategy();
                    break;

                case TraversalStyle.SinglePass:
                    traversalStrategy = new SinglePassTraversalStrategy();
                    break;

                default:
                case TraversalStyle.Reversal:
                    traversalStrategy = new ReversalTraversalStrategy();
                    break;
            }

            IEasingStrategy easingStrategy;

            switch( animationPattern.EasingStyle ) {
                case EasingStyle.SineIn:
                    easingStrategy = new SineInEasingStrategy();
                    break;

                case EasingStyle.SineInOut:
                    easingStrategy = new SineInOutEasingStrategy();
                    break;

                case EasingStyle.SineOut:
                    easingStrategy = new SineOutEasingStrategy();
                    break;

                case EasingStyle.SineOutIn:
                    easingStrategy = new SineOutInEasingStrategy();
                    break;

                case EasingStyle.QuadraticIn:
                    easingStrategy = new QuadraticInEasingStrategy();
                    break;

                case EasingStyle.QuadraticInOut:
                    easingStrategy = new QuadraticInOutEasingStrategy();
                    break;

                case EasingStyle.QuadraticOut:
                    easingStrategy = new QuadraticOutEasingStrategy();
                    break;

                case EasingStyle.QuadraticOutIn:
                    easingStrategy = new QuadraticOutInEasingStrategy();
                    break;

                case EasingStyle.QuarticIn:
                    easingStrategy = new QuinticInEasingStrategy();
                    break;

                case EasingStyle.QuarticInOut:
                    easingStrategy = new QuinticInOutEasingStrategy();
                    break;

                case EasingStyle.QuarticOut:
                    easingStrategy = new QuinticOutEasingStrategy();
                    break;

                case EasingStyle.QuarticOutIn:
                    easingStrategy = new QuinticOutInEasingStrategy();
                    break;

                case EasingStyle.QuinticIn:
                    easingStrategy = new QuinticInEasingStrategy();
                    break;

                case EasingStyle.QuinticInOut:
                    easingStrategy = new QuinticInOutEasingStrategy();
                    break;

                case EasingStyle.QuinticOut:
                    easingStrategy = new QuinticOutEasingStrategy();
                    break;

                case EasingStyle.QuinticOutIn:
                    easingStrategy = new QuinticOutInEasingStrategy();
                    break;

                case EasingStyle.ExponentialIn:
                    easingStrategy = new ExponentialInEasingStrategy();
                    break;

                case EasingStyle.ExponentialInOut:
                    easingStrategy = new ExponentialInOutEasingStrategy();
                    break;

                case EasingStyle.ExponentialOut:
                    easingStrategy = new ExponentialOutEasingStrategy();
                    break;

                case EasingStyle.ExponentialOutIn:
                    easingStrategy = new ExponentialOutInEasingStrategy();
                    break;

                case EasingStyle.CircularIn:
                    easingStrategy = new CircularInEasingStrategy();
                    break;

                case EasingStyle.CircularInOut:
                    easingStrategy = new CircularInOutEasingStrategy();
                    break;

                case EasingStyle.CircularOut:
                    easingStrategy = new CircularOutEasingStrategy();
                    break;

                case EasingStyle.CircularOutIn:
                    easingStrategy = new CircularOutInEasingStrategy();
                    break;

                case EasingStyle.CubicIn:
                    easingStrategy = new CubicInEasingStrategy();
                    break;

                case EasingStyle.CubicInOut:
                    easingStrategy = new CubicInOutEasingStrategy();
                    break;

                case EasingStyle.CubicOut:
                    easingStrategy = new CubicOutEasingStrategy();
                    break;

                case EasingStyle.CubicOutIn:
                    easingStrategy = new CubicOutInEasingStrategy();
                    break;

                default:
                case EasingStyle.Linear:
                    easingStrategy = new LinearEasingStrategy();
                    break;
            }

            if( traversalStrategy == null ) {
                throw new ApplicationException( "Traversal strategy could not be created." );
            }

            if( easingStrategy == null ) {
                throw new ApplicationException( "Easing strategy could not be created." );
            }

            return new ColorAnimationSequence( forBulbs, animationPattern, traversalStrategy, easingStrategy );
        }
    }
}
