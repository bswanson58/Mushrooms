using System;
using Fluxor;
using Mushrooms.Models;

namespace Mushrooms.SceneBuilder.Store {
    [FeatureState( CreateInitialStateMethodName = nameof( Factory ))]
    internal record SceneState {
        public  string          SceneName { get; init; }    = String.Empty;
        public  ScenePalette    Palette { get; init; }      = ScenePalette.Default;
        public  SceneParameters Parameters { get; init; }   = SceneParameters.Default;
        public  SceneControl    Control { get; init; }      = new ();

        public static SceneState Factory() => new ();
    }

    // actions
    internal class SetScenePaletteAction {
        public  ScenePalette    Palette { get; }

        public SetScenePaletteAction( ScenePalette palette ) => 
            Palette = palette;
    }

    internal class SetSceneParametersAction {
        public  SceneParameters Parameters { get; }

        public SetSceneParametersAction( SceneParameters parameters ) =>
            Parameters = parameters;
    }

    // reducers
    // ReSharper disable once UnusedType.Global
    internal static class SceneReducers {
        [ReducerMethod]
        public static SceneState OnSetPalette( SceneState state, SetScenePaletteAction action ) =>
            state with {
                Palette = action.Palette
            };

        [ReducerMethod]
        public static SceneState OnSetParameters( SceneState state, SetSceneParametersAction action ) =>
            state with {
                Parameters = action.Parameters
            };
    }
}
