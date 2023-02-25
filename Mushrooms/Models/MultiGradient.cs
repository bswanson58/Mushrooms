using Mushrooms.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Mushrooms.Models {
    public class GradientColor {
        public  Color   Color { get; }
        public  double  Offset { get; }

        public GradientColor( Color color, double offset ) {
            Color = color;
            Offset = offset;

            if(( Offset < 0.0D ) ||
               ( offset > 1.0D )) {
                throw new ApplicationException( "Invalid offset for GradientColor" );
            }
        }
    }

    public class MultiGradient {
        private readonly List<GradientColor>    mGradientStops;

        public MultiGradient( IEnumerable<GradientColor> colors ) {
            mGradientStops = new List<GradientColor>( colors );
        }

        public void SetGradientColors( IEnumerable<GradientColor> colors ) {
            mGradientStops.Clear();

            foreach ( var stop in colors ) {
                mGradientStops.Add( stop );
            }
        }

        public Color ColorAt( double position ) {
            if( position > 1.0D ) {
                position = 1.0D;
            }

            return GetColorByOffset( position );
        }

        // from: https://stackoverflow.com/questions/9650049/get-color-in-specific-location-on-gradient
        private Color GetColorByOffset( double offset ) {
            var stops = mGradientStops.OrderBy( x => x.Offset ).ToArray();

            if( offset <= 0 ) 
                return stops[0].Color;

            if( offset >= 1 ) 
                return stops[^1].Color;
            /*
            GradientStop left = stops[0], right = null;

            foreach( GradientStop stop in stops ) {
                if( stop.Offset >= offset ) {
                    right = stop;
                    break;
                }
                left = stop;
            }
            */
            var left = stops.LastOrDefault( s => s.Offset <= offset, stops.First()); 
            var right = stops.FirstOrDefault( s => s.Offset > offset, stops.Last());
            var colorOffset = Math.Round(( offset - left.Offset ) / ( right.Offset - left.Offset ), 2 );

            byte a = (byte)(( right.Color.A - left.Color.A ) * colorOffset + left.Color.A );
            byte r = (byte)(( right.Color.R - left.Color.R ) * colorOffset + left.Color.R );
            byte g = (byte)(( right.Color.G - left.Color.G ) * colorOffset + left.Color.G );
            byte b = (byte)(( right.Color.B - left.Color.B ) * colorOffset + left.Color.B );

            return Color.FromArgb( a, r, g, b );
        }

        public static MultiGradient Create( IList<Color> fromColors ) =>
            new( CreateGradients( fromColors ));

        private static IList<GradientColor> CreateGradients( IList<Color> fromColors ) {
            var retValue = new List<GradientColor>();
            var gradientStop = 0.0D;
            var offset = 1.0D;

            if( fromColors.Count == 3 ) {
                offset = 0.5D;
            }
            else if( fromColors.Count > 3 ) {
                offset = CalculateOffset( fromColors.Count ) / 2.0D;
            }

            foreach( var color in fromColors.Randomize()) {
                retValue.Add( new GradientColor( color, gradientStop ));

                gradientStop = Math.Min( 1.0D, gradientStop + offset );
                offset = CalculateOffset( fromColors.Count );
            }

            return retValue;
        }

        private static double CalculateOffset( int totalStops ) {
            var offset = 1.0D;

            if( totalStops == 3 ) {
                offset = 0.5D;
            }
            else if( totalStops > 3 ) {
                offset = ( 1.0D / ( totalStops - 2 ));
            }

            return offset;
        }
    }
}
