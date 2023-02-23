using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var left = stops.Last( s => s.Offset <= offset ); 
            var right = stops.First( s => s.Offset > offset );

            Debug.Assert( right != null );

            offset = Math.Round( ( offset - left.Offset ) / ( right.Offset - left.Offset ), 2 );

            byte a = (byte)(( right.Color.A - left.Color.A ) * offset + left.Color.A );
            byte r = (byte)(( right.Color.R - left.Color.R ) * offset + left.Color.R );
            byte g = (byte)(( right.Color.G - left.Color.G ) * offset + left.Color.G );
            byte b = (byte)(( right.Color.B - left.Color.B ) * offset + left.Color.B );

            return Color.FromArgb( a, r, g, b );
        }
    }
}
