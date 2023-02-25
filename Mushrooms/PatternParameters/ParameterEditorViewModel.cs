using System;
using System.Collections.Generic;
using System.Linq;
using Fluxor;
using Mushrooms.Models;
using Mushrooms.PatternTester.Store;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.PatternParameters {
    internal class TraversalStyleItem {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public  string          DisplayString { get; }
        public  TraversalStyle  TraversalStyle { get; }

        public TraversalStyleItem( TraversalStyle traversalStyle, string displayString ) {
            TraversalStyle = traversalStyle;
            DisplayString = displayString;
        }
    }

    internal class EasingStyleItem {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public  string          DisplayString { get; }
        public  EasingStyle     EasingStyle { get; }

        public EasingStyleItem( EasingStyle easingStyle, string displayString ) {
            EasingStyle = easingStyle;
            DisplayString = displayString;
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ParameterEditorViewModel : PropertyChangeBase {
        private readonly IPatternTest               mTestFacade;
        private TimeSpan                            mDuration;
        private TimeSpan                            mDurationJitter;
        private TraversalStyleItem ?                mTraversalStyle;
        private EasingStyleItem ?                   mEasingStyle;
        private int                                 mPatternCount;

        public  List<TraversalStyleItem>            TraversalSelections { get; }
        public  List<EasingStyleItem>               EasingSelections { get; }

        public ParameterEditorViewModel( IState<PatternTestState> testState, IPatternTest testFacade ) {
            mTestFacade = testFacade;

            mDuration = testState.Value.Parameters.Duration;
            mDurationJitter = testState.Value.Parameters.DurationJitter;
            mPatternCount = testState.Value.Parameters.PatternCount;

            TraversalSelections = new List<TraversalStyleItem>( CreateTraversalSelections());
            mTraversalStyle = 
                TraversalSelections.FirstOrDefault( t => t.TraversalStyle.Equals( testState.Value.Parameters.TraversalStyle ));

            EasingSelections = new List<EasingStyleItem>( CreateEasingSelections());
            mEasingStyle = 
                EasingSelections.FirstOrDefault( e => e.EasingStyle.Equals( testState.Value.Parameters.EasingStyle ));

            RaiseAllPropertiesChanged();
        }

        public int DurationSeconds {
            get => (int)mDuration.TotalSeconds;
            set {
                mDuration = TimeSpan.FromSeconds( value );

                UpdateTestState();
            }
        }

        public int DurationJitterSeconds {
            get => (int)mDurationJitter.TotalSeconds;
            set {
                mDurationJitter = TimeSpan.FromSeconds( value );

                UpdateTestState();
            }
        }

        public int PatternCount {
            get => mPatternCount;
            set {
                mPatternCount = Math.Min( 40, Math.Max( 3, value ));

                UpdateTestState();
            }
        }

        public TraversalStyleItem ? SelectedTraversalStyle {
            get => mTraversalStyle;
            set {
                mTraversalStyle = value;

                UpdateTestState();
            }
        }

        public EasingStyleItem ? SelectedEasingStyle {
            get => mEasingStyle;
            set {
                mEasingStyle = value;

                UpdateTestState();
            }
        }

        private void UpdateTestState() {
            if(( mTraversalStyle != null ) &&
               ( mEasingStyle != null )) {
                mTestFacade.SetPatternParameters( new PatternTester.PatternParameters {
                    TraversalStyle = mTraversalStyle.TraversalStyle,
                    EasingStyle = mEasingStyle.EasingStyle,
                    Duration = mDuration,
                    DurationJitter = mDurationJitter,
                    PatternCount = mPatternCount
                });
            }
        }

        private IEnumerable<TraversalStyleItem> CreateTraversalSelections() =>
            new List<TraversalStyleItem> {
                new( TraversalStyle.Looping, "Looping" ),
                new( TraversalStyle.Reversal, "Reversing" )
            };

        private IEnumerable<EasingStyleItem> CreateEasingSelections() {
            var retValue = new List<EasingStyleItem> {
                new( EasingStyle.Linear, "Linear" ),
                new( EasingStyle.CircularIn, "Circular In" ),
                new( EasingStyle.CircularInOut, "Circular In/Out"),
                new( EasingStyle.CircularOut, "Circular Out" ),
                new( EasingStyle.CircularOutIn, "Circular Out/In" )
            };

            return retValue;
        }
    }
}
