using System.Collections.Generic;
using System.Windows.Media;
using Fluxor;

namespace Mushrooms.PatternTester.Store {
    public interface IPatternTest {
        void    SetPatternPalette( IEnumerable<Color> palette );
        void    SetPatternParameters( PatternParameters parameters );
        void    SetDisplayParameters( DisplayParameters parameters );
    }

    public class PatternTestFacade : IPatternTest {
        private readonly IDispatcher    mDispatcher;

        public PatternTestFacade( IDispatcher dispatcher ) {
            mDispatcher = dispatcher;
        }

        public void SetPatternPalette( IEnumerable<Color> palette ) =>
            mDispatcher.Dispatch( new SetPatternPaletteAction( palette ));

        public void SetPatternParameters( PatternParameters parameters ) =>
            mDispatcher.Dispatch( new SetPatternParametersAction( parameters ));

        public void SetDisplayParameters( DisplayParameters parameters ) =>
            mDispatcher.Dispatch( new SetDisplayParametersAction( parameters ));
    }
}
