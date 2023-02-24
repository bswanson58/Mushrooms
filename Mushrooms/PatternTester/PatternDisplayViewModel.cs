using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using Fluxor;
using Mushrooms.ColorAnimation;
using Mushrooms.Models;
using Mushrooms.PatternTester.Store;
using Mushrooms.Support;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.PatternTester {
    internal class TestBulb {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public  string  BulbId { get; }
        public  Color   BulbColor { get; }
        public  double  Brightness { get; }

        public TestBulb( string bulbId, Color color, double brightness ) {
            BulbId = bulbId;
            BulbColor = color;
            Brightness = brightness;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class PatternDisplayViewModel : PropertyChangeBase, IDisposable {
        private readonly IState<PatternTestState>   mState;
        private readonly IAnimationProcessor        mAnimationProcessor;
        private readonly LimitedRepeatingRandom     mRandom;
        private IDisposable ?                       mAnimationSubscription;
        private Double                              mRateMultiplier;
        private Double                              mBrightness;
        private bool                                mSynchronizeBulbs;
        private ColorAnimationJob ?                 mJob;

        public  ObservableCollection<TestBulb>      Bulbs { get; }

        public PatternDisplayViewModel( IState<PatternTestState> state, IAnimationProcessor animationProcessor ) {
            mState = state;
            mAnimationProcessor = animationProcessor;

            mRandom = new LimitedRepeatingRandom();
            mRateMultiplier = 1.0D;
            mBrightness = 1.0D;
            mSynchronizeBulbs = false;

            Bulbs = new ObservableCollection<TestBulb>();
            BindingOperations.EnableCollectionSynchronization( Bulbs, new object());

            mState.StateChanged += OnStateChanged;
            mAnimationSubscription = mAnimationProcessor.OnResultsPublished.Subscribe( OnAnimationResults );

            UpdateAnimation();
            mAnimationProcessor.StartAnimationProcessor();
        }

        public double Brightness {
            get => mBrightness;
            set {
                if(( value >= 0.0D ) &&
                   ( value <= 1.0D )) {
                    mBrightness = value;

                    UpdateJobParameters();
                }
            }
        }

        public double RateMultiplier {
            get => mRateMultiplier;
            set {
                if(( value > 0.1D ) &&
                   ( value < 100.0D )) {
                    mRateMultiplier = value;

                    UpdateJobParameters();
                }
            }
        }

        public bool SynchronizeBulbs {
            get => mSynchronizeBulbs;
            set {
                mSynchronizeBulbs = value;

                UpdateJobParameters();
            }
        }

        private void UpdateJobParameters() =>
            mJob?.UpdateParameters( CreateParameters());

        private ColorAnimationParameters CreateParameters() =>
            new( mSynchronizeBulbs, mRateMultiplier, mBrightness );

        private void OnAnimationResults( List<ColorAnimationResult> animationResults ) {
            Bulbs.Clear();

            foreach( var result in animationResults ) {
                foreach( var bulb in result.Bulbs ) {
                    Bulbs.Add( new TestBulb( bulb, result.Color, result.Brightness ));
                }
            }
        }

        private void OnStateChanged( object ? sender, EventArgs e ) {
            UpdateAnimation();
        }

        private void UpdateAnimation() {
            if( IsStateRunnable()) {
                if( mJob != null ) {
                    mAnimationProcessor.RemoveJob( mJob );
                }

                mJob = CreateAnimationJob();
                mAnimationProcessor.AddJob( mJob );
            }
        }

        private bool IsStateRunnable() => 
            mState.Value.Palette.Count > 1 &&
            mState.Value.Parameters.PatternCount > 2;

        private ColorAnimationJob CreateAnimationJob() {
            var bulbGroup = new List<string>{ "bulb1", "bulb2", "bulb3", "bulb4", "bulb5", "bulb6", "bulb7" };
            var palette = new List<ColorAnimationPattern>();

            for( var index = 1; index <= mState.Value.Parameters.PatternCount; index++ ) {
                palette.Add( BuildPattern( mState.Value.Palette, mState.Value.Parameters ));
            }

            var animationParameters = new ColorAnimationParameters( false );

            return new ColorAnimationJob( bulbGroup, palette, animationParameters );
        }

        private ColorAnimationPattern BuildPattern( IReadOnlyList<Color> fromColors, PatternParameters parameters ) {
            var gradient = new MultiGradient( CreateGradients( fromColors.ToList()));
            var duration = parameters.Duration + TimeSpan.FromSeconds( mRandom.Next( 0, (int)parameters.DurationJitter.TotalSeconds ));

            return new ColorAnimationPattern( gradient, duration, parameters.TraversalStyle, parameters.EasingStyle );
        }

        private IList<GradientColor> CreateGradients( IList<Color> fromColors ) {
            var retValue = new List<GradientColor>();
            var gradientStop = 0.0D;
            var offset = 1.0D;

            if( fromColors.Count == 3 ) {
                offset = 0.5D;
            }
            else if( fromColors.Count > 3 ) {
                offset = ( 1.0D / ( fromColors.Count - 2 )) / 2.0D;
            }

            foreach( var color in fromColors.Randomize()) {
                retValue.Add( new GradientColor( color, gradientStop ));

                gradientStop = Math.Min( 1.0D, gradientStop + offset );
                offset += Math.Min( 1.0D, ( 1.0D / ( fromColors.Count - 2 )));
            }

            return retValue;
        }

        public void Dispose() {
            mAnimationProcessor.StopAnimationProcessor();

            mAnimationSubscription?.Dispose();
            mAnimationSubscription = null;
        }
    }
}
