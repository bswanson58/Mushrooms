using System.Collections.Generic;
using System.Windows.Media;
using Fluxor;

namespace Mushrooms.PatternTester.Store {
    [FeatureState( CreateInitialStateMethodName = "Factory")]
    public class PatternTestState {
        public  IReadOnlyList<Color>    Palette { get; }
        public  PatternParameters       Parameters { get; }
        public  DisplayParameters       DisplayParameters { get; }

        public PatternTestState( IReadOnlyList<Color> palette, PatternParameters parameters, DisplayParameters displayParameters ) {
            Palette = palette;
            Parameters = parameters;
            DisplayParameters = displayParameters;
        }

        public static PatternTestState Factory() => 
            new( new List<Color>(), PatternParameters.DefaultParameters, DisplayParameters.DefaultDisplayParameters );
    }

    public class SetPatternPaletteAction {
        public  IEnumerable<Color>  Palette { get; }

        public SetPatternPaletteAction( IEnumerable<Color> palette ) => 
            Palette = palette;
    }

    public class SetPatternParametersAction {
        public  PatternParameters   Parameters { get; }

        public SetPatternParametersAction( PatternParameters parameters ) => 
            Parameters = parameters;
    }

    public class SetDisplayParametersAction {
        public  DisplayParameters   Parameters { get; }

        public SetDisplayParametersAction( DisplayParameters parameters ) {
            Parameters = parameters;
        }
    }

    // ReSharper disable once UnusedType.Global
    public static class PatternStateReducers {
        [ReducerMethod]
        public static PatternTestState OnSetPalette( PatternTestState state, SetPatternPaletteAction action ) =>
            new( new List<Color>( action.Palette ), state.Parameters, state.DisplayParameters );

        [ReducerMethod]
        public static PatternTestState OnSetParameters( PatternTestState state, SetPatternParametersAction action ) =>
            new( state.Palette, action.Parameters, state.DisplayParameters );

        [ReducerMethod]
        public static PatternTestState OnSetDisplayParameters( PatternTestState state, SetDisplayParametersAction action ) =>
            new( state.Palette, state.Parameters, action.Parameters );
    }
}
