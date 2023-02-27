using Fluxor;
using Mushrooms.Models;

namespace Mushrooms.SceneBuilder.Store {
    internal interface ISceneFacade {
        void    SetScenePalette( ScenePalette palette );
        void    SetSceneParameters( SceneParameters parameters );
    }

    internal class SceneFacade : ISceneFacade {
        private readonly IDispatcher    mDispatcher;

        public SceneFacade( IDispatcher dispatcher ) {
            mDispatcher = dispatcher;
        }

        public void SetScenePalette( ScenePalette palette ) => 
            mDispatcher.Dispatch( new SetScenePaletteAction( palette ));

        public void SetSceneParameters( SceneParameters parameters ) =>
            mDispatcher.Dispatch( new SetSceneParametersAction( parameters ));
    }
}
