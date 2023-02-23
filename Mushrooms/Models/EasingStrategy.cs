namespace Mushrooms.Models {
    public enum EasingStyle {
        Linear,
        SineIn, SineOut, SineInOut, SineOutIn,
        QuadraticIn, QuadraticOut, QuadraticInOut, QuadraticOutIn,
        CubicIn, CubicOut, CubicInOut, CubicOutIn,
        QuarticIn, QuarticOut, QuarticInOut, QuarticOutIn,
        QuinticIn, QuinticOut, QuinticInOut, QuinticOutIn,
        ExponentialIn, ExponentialOut, ExponentialInOut, ExponentialOutIn,
        CircularIn, CircularOut, CircularInOut, CircularOutIn
    }
    public interface IEasingStrategy {
        double  GetEasedValue( double current );
    }

    // Linear
    public class LinearEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.Linear( current, 0.0D, 1.0D, 1.0D );
    }

    // Circular
    public class CircularInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CircEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class CircularOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CircEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class CircularInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CircEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class CircularOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CircEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }

    // Exponential
    public class ExponentialInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.ExpoEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class ExponentialOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.ExpoEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class ExponentialInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.ExpoEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class ExponentialOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.ExpoEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }

    // Quadratic
    public class QuadraticInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuadEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuadraticOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuadEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuadraticInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuadEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuadraticOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuadEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }

    // Sine
    public class SineInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.SineEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class SineOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.SineEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class SineInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.SineEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class SineOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.SineEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }

    // Cubic
    public class CubicInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CubicEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class CubicOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CubicEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class CubicInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CubicEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class CubicOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.CubicEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }

    // Quartic
    public class QuarticInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuartEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuarticOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuartEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuarticInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuartEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuarticOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuartEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }

    // Quintic
    public class QuinticInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuintEaseIn( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuinticOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuintEaseOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuinticInOutEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuintEaseInOut( current, 0.0D, 1.0D, 1.0D );
    }

    public class QuinticOutInEasingStrategy : IEasingStrategy {
        public double GetEasedValue( double current ) => Easing.QuintEaseOutIn( current, 0.0D, 1.0D, 1.0D );
    }
}
