using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Mushrooms.ColorAnimation;
using Mushrooms.Models;
using Mushrooms.Support;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MainWindowViewModel : PropertyChangeBase, IDisposable {
        private readonly IAnimationProcessor    mAnimationProcessor;
        private readonly LimitedRepeatingRandom mRandom;
        private IDisposable ?                   mAnimationSubscription;
        private Double                          mRateMultiplier;
        private Double                          mBrightness;
        private bool                            mSynchronizeBulbs;
        private ColorAnimationJob               mJob;

        public  Color                           ColorSwatch1 { get; private set; }
        public  Color                           ColorSwatch2 { get; private set; }
        public  Color                           ColorSwatch3 { get; private set; }
        public  Color                           ColorSwatch4 { get; private set; }
        public  Color                           ColorSwatch5 { get; private set; }
        public  Color                           ColorSwatch6 { get; private set; }
        public  Color                           ColorSwatch7 { get; private set; }

        public  Double                          ColorBrightness { get; private set; }


        public MainWindowViewModel( IAnimationProcessor animationProcessor ) {
            mAnimationProcessor = animationProcessor;
            mRandom = new LimitedRepeatingRandom();

            mRateMultiplier = 1.0D;
            mBrightness = 1.0D;
            mSynchronizeBulbs = false;

            ColorBrightness = 1.0D;
            /*
            mAnimationSubscription = mAnimationProcessor.OnResultsPublished.Subscribe( OnAnimationResults );

            mJob = CreateAnimationJob();

            mAnimationProcessor.AddJob( mJob );
            mAnimationProcessor.StartAnimationProcessor();
            */
        }

        public double Brightness {
            get => mBrightness;
            set {
                if(( value >= 0.0D ) &&
                   ( value <= 1.0D )) {
                    mBrightness = value;

                    mJob.UpdateParameters( CreateParameters());
                }
            }
        }

        public double RateMultiplier {
            get => mRateMultiplier;
            set {
                if(( value > 0.1D ) &&
                   ( value < 100.0D )) {
                    mRateMultiplier = value;

                    mJob.UpdateParameters( CreateParameters());
                }
            }
        }

        public bool SynchronizeBulbs {
            get => mSynchronizeBulbs;
            set {
                mSynchronizeBulbs = value;

                mJob.UpdateParameters( CreateParameters());
            }
        }

        private ColorAnimationJob CreateAnimationJob () {
            var bulbGroup = new List<string>{ "bulb1", "bulb2", "bulb3", "bulb4", "bulb5", "bulb6", "bulb7" };
            var palette = new List<ColorAnimationPattern>();

            for( var index = 1; index <= 9; index++ ) {
//                var colorList = index % 2 == 1 ? GetRandomColors().ToList() : GetAlternateColors().ToList();
                var colorList = GetAutumnColors().Randomize();
                var gradientColors = new List<GradientColor> {
                    new ( colorList[0], 0.0D ),
                    new ( colorList[1], 0.10D ),
                    new ( colorList[2], 0.30D ),
                    new ( colorList[3], 0.50D ),
                    new ( colorList[4], 0.70D ),
                    new ( colorList[5], 0.90D ),
                    new ( colorList[6], 1.0D ),
                };

                var pattern = new ColorAnimationPattern( 
                    new MultiGradient( gradientColors ), TimeSpan.FromSeconds( 3 + mRandom.Next( 0, 5 )), 
                                       TraversalStyle.Reversal, EasingStyle.CircularOut );

                palette.Add( pattern );
            }

            return new ColorAnimationJob( bulbGroup, palette, CreateParameters());
        }

        private ColorAnimationParameters CreateParameters() =>
            new( mSynchronizeBulbs, mRateMultiplier, mBrightness );

        private IList<Color> GetRandomColors() {
            var colorList = new List<Color> {
                Colors.DarkRed,
                Colors.DarkGoldenrod,
                Colors.ForestGreen,
                Colors.OrangeRed,
                Colors.Brown,
                Colors.Crimson,
                Colors.IndianRed,
                Colors.DarkOliveGreen,
                Colors.Gold,
                Colors.Goldenrod
            };

            return colorList.Randomize();
        }

        private IList<Color> GetAlternateColors() {
            var colorList = new List<Color> {
                Colors.DarkBlue,
                Colors.Aqua,
                Colors.DodgerBlue,
                Colors.LightBlue,
                Colors.CornflowerBlue,
                Colors.MediumSlateBlue,
                Colors.DarkOrchid,
                Colors.DarkSlateBlue,
                Colors.LightSkyBlue,
                Colors.Indigo
            };

            return colorList.Randomize();
        }

        private IList<Color> GetAutumnColors() =>
            new List<Color> {
                (Color)ColorConverter.ConvertFromString( "#CC7644" ),
                (Color)ColorConverter.ConvertFromString( "#CF4E21" ),
                (Color)ColorConverter.ConvertFromString( "#652F31" ),
                (Color)ColorConverter.ConvertFromString( "#CF5453" ),
                (Color)ColorConverter.ConvertFromString( "#B67A18" ),
                (Color)ColorConverter.ConvertFromString( "#BFB415" ),
                (Color)ColorConverter.ConvertFromString( "#EAA164" ),
                (Color)ColorConverter.ConvertFromString( "#E88D1A" ),
                (Color)ColorConverter.ConvertFromString( "#BC201F" ),
                (Color)ColorConverter.ConvertFromString( "#EEC213" ),
            };

        private void OnAnimationResults( List<ColorAnimationResult> resultsList ) {
            ColorSwatch1 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb1" )), new ColorAnimationResult()).Color;
            ColorSwatch2 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb2" )), new ColorAnimationResult()).Color;
            ColorSwatch3 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb3" )), new ColorAnimationResult()).Color;
            ColorSwatch4 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb4" )), new ColorAnimationResult()).Color;
            ColorSwatch5 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb5" )), new ColorAnimationResult()).Color;
            ColorSwatch6 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb6" )), new ColorAnimationResult()).Color;
            ColorSwatch7 = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb7" )), new ColorAnimationResult()).Color;

            ColorBrightness = resultsList.FirstOrDefault( r => r.Bulbs.Any( b => b.Equals( "bulb1" )), new ColorAnimationResult()).Brightness;

            RaisePropertyChanged(() => ColorSwatch1 );
            RaisePropertyChanged(() => ColorSwatch2 );
            RaisePropertyChanged(() => ColorSwatch3 );
            RaisePropertyChanged(() => ColorSwatch4 );
            RaisePropertyChanged(() => ColorSwatch5 );
            RaisePropertyChanged(() => ColorSwatch6 );
            RaisePropertyChanged(() => ColorSwatch7 );

            RaisePropertyChanged(() => ColorBrightness );
        }

        public void Dispose() {
            mAnimationProcessor.StopAnimationProcessor();

            mAnimationSubscription?.Dispose();
            mAnimationSubscription = null;
        }
    }
}
