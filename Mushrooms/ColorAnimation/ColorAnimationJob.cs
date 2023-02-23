using System;
using System.Collections.Generic;
using System.Linq;
using Mushrooms.Support;
using NCuid;

namespace Mushrooms.ColorAnimation
{
    public class ColorAnimationJob {
        private readonly LimitedRepeatingRandom mRandom;

        public string JobId { get; }
        public List<string> BulbGroup { get; }
        public List<ColorAnimationPattern> ColorPalette { get; }
        public ColorAnimationParameters Parameters { get; private set; }

        public ColorAnimationJob( IEnumerable<string> bulbGroup, IEnumerable<ColorAnimationPattern> colorPalette,
                                  ColorAnimationParameters parameters ) {
            Parameters = parameters;
            BulbGroup = new List<string>( bulbGroup );
            ColorPalette = new List<ColorAnimationPattern>( colorPalette );

            mRandom = new LimitedRepeatingRandom( 0.6 );

            if( !ColorPalette.Any() ) {
                throw new ApplicationException( "A list of ColorAnimationPattern instances must be provided" );
            }

            JobId = Cuid.Generate();
        }

        public ColorAnimationPattern GetRandomPattern() =>
            ColorPalette[mRandom.Next( 0, ColorPalette.Count )];

        public ColorAnimationJob With( ColorAnimationParameters parameters ) =>
            new( BulbGroup, ColorPalette, parameters );

        public void UpdateParameters( ColorAnimationParameters parameters ) =>
            Parameters = parameters;
    }
}
