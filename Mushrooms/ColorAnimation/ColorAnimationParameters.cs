using System;

namespace Mushrooms.ColorAnimation {
    public class ColorAnimationParameters {
        public bool SynchronizedBulbs { get; }
        public double RateMultiplier { get; }
        public double Brightness { get; }

        public ColorAnimationParameters( bool synchronizedBulbs ) {
            SynchronizedBulbs = synchronizedBulbs;
            RateMultiplier = 1.0D;
            Brightness = 1.0D;
        }

        public ColorAnimationParameters( double rateMultiplier, double brightness ) :
            this( false, rateMultiplier, brightness ) { }

        public ColorAnimationParameters( bool synchronizedBulbs, double rateMultiplier, double brightness ) {
            SynchronizedBulbs = synchronizedBulbs;
            RateMultiplier = rateMultiplier;
            Brightness = brightness;

            if( RateMultiplier < 0.1D ||
               RateMultiplier > 100.0D ) {
                throw new ApplicationException( "Invalid RateMultiplier value." );
            }

            if( Brightness < 0.0D ||
               Brightness > 1.0D ) {
                throw new ApplicationException( "Invalid Brightness value." );
            }
        }
    }
}
