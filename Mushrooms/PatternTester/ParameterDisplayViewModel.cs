using System;
using Fluxor;
using Mushrooms.PatternTester.Store;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.PatternTester {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ParameterDisplayViewModel : PropertyChangeBase {
        private readonly IState<PatternTestState>   mState;

        public  int         SwatchCount { get; private set; }
        public  int         PatternCount { get; private set; }
        public  int         BulbCount { get; private set; }
        public  string      BaseDuration {  get; private set; }
        public  string      DurationJitter { get; private set; }
        public  string      RateMultiplier { get; private set; }

        public ParameterDisplayViewModel( IState<PatternTestState> state ) {
            mState = state;

            mState.StateChanged += OnStateChanged;

            SwatchCount = 0;
            PatternCount = 0;
            BulbCount = 0;
            BaseDuration = String.Empty;
            DurationJitter = String.Empty;
            RateMultiplier = String.Empty;

            UpdateDisplay();
        }

        private void OnStateChanged( object ? sender, EventArgs e ) {
            UpdateDisplay();
        }

        private void UpdateDisplay() {
            BulbCount = mState.Value.DisplayParameters.BulbCount;
            SwatchCount = mState.Value.Palette.Count;
            PatternCount = mState.Value.Parameters.PatternCount;
            BaseDuration = mState.Value.Parameters.Duration.ToString();
            DurationJitter = mState.Value.Parameters.DurationJitter.ToString();
            RateMultiplier = mState.Value.DisplayParameters.RateMultiplier.ToString( "F2" );

            RaiseAllPropertiesChanged();
        }
    }
}
