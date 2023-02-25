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
        private readonly IPatternTest               mTestFacade;
        private IDisposable ?                       mAnimationSubscription;
        private Double                              mRateMultiplier;
        private Double                              mBrightness;
        private bool                                mSynchronizeBulbs;
        private int                                 mBulbCount;
        private ColorAnimationJob ?                 mJob;

        public  ObservableCollection<TestBulb>      Bulbs { get; }

        public PatternDisplayViewModel( IState<PatternTestState> state, IAnimationProcessor animationProcessor, 
                                        IPatternTest testFacade ) {
            mState = state;
            mAnimationProcessor = animationProcessor;
            mTestFacade = testFacade;

            mRandom = new LimitedRepeatingRandom();
            mRateMultiplier = mState.Value.DisplayParameters.RateMultiplier;
            mBrightness = 1.0D;
            mSynchronizeBulbs = false;
            mBulbCount = mState.Value.DisplayParameters.BulbCount;

            Bulbs = new ObservableCollection<TestBulb>();
            BindingOperations.EnableCollectionSynchronization( Bulbs, Bulbs );

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
                mRateMultiplier = Math.Min( 10.0, Math.Max( 0.1, value ));

                UpdateJobParameters();
                UpdateDisplayParameters();
            }
        }

        public bool SynchronizeBulbs {
            get => mSynchronizeBulbs;
            set {
                mSynchronizeBulbs = value;

                UpdateJobParameters();
            }
        }

        public int BulbCount {
            get => mBulbCount;
            set {
                mBulbCount = Math.Min( 20, Math.Max( 1, value ));

                UpdateAnimation();
                UpdateDisplayParameters();
            }
        }

        private void UpdateDisplayParameters() {
            mTestFacade.SetDisplayParameters( new DisplayParameters {
                BulbCount = mBulbCount,
                RateMultiplier = mRateMultiplier
            });
        }

        private void UpdateJobParameters() =>
            mJob?.UpdateParameters( CreateParameters());

        private ColorAnimationParameters CreateParameters() =>
            new( mSynchronizeBulbs, mRateMultiplier, mBrightness );

        private void OnAnimationResults( List<ColorAnimationResult> animationResults ) {
            lock( Bulbs ) {
                Bulbs.Clear();

                foreach( var result in animationResults ) {
                    foreach( var bulb in result.Bulbs ) {
                        Bulbs.Add( new TestBulb( bulb, result.Color, result.Brightness ));
                    }
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
            var bulbGroup = CreateBulbGroup();
            var palette = new List<ColorAnimationPattern>();

            for( var index = 1; index <= mState.Value.Parameters.PatternCount; index++ ) {
                palette.Add( BuildPattern( mState.Value.Palette, mState.Value.Parameters ));
            }

            var animationParameters = new ColorAnimationParameters( false );

            return new ColorAnimationJob( bulbGroup, palette, animationParameters );
        }

        private IList<string> CreateBulbGroup() =>
            Enumerable.Range( 1, mBulbCount ).Select( i => $"Bulb {i}" ).ToList();

        private ColorAnimationPattern BuildPattern( IReadOnlyList<Color> fromColors, PatternParameters parameters ) {
            var gradient = MultiGradient.Create( fromColors.ToList());
            var duration = parameters.Duration + TimeSpan.FromSeconds( mRandom.Next( 0, (int)parameters.DurationJitter.TotalSeconds ));

            return new ColorAnimationPattern( gradient, duration, parameters.TraversalStyle, parameters.EasingStyle );
        }

        public void Dispose() {
            mAnimationProcessor.StopAnimationProcessor();

            mAnimationSubscription?.Dispose();
            mAnimationSubscription = null;
        }
    }
}
