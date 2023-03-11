using System;
using System.Collections.Generic;
using Mushrooms.Database;

// ReSharper disable MemberCanBePrivate.Global

namespace Mushrooms.Entities {
    internal enum SceneMode {
        Animating = 1,
        Stationary = 2
    }

    internal class Scene : EntityBase {
        // protected sets are for LiteDB.
        public  string              SceneName { get; protected set; }
        public  SceneMode           SceneMode { get; protected set; }
        public  ScenePalette        AnimationPalette { get; protected  set; }
        public  ScenePalette        StationaryPalette { get; protected  set; }
        public  SceneParameters     Parameters { get; protected  set; }
        public  SceneControl        Control { get; protected set; }
        public  IList<LightSource>  Lights { get; protected set; }
        public  SceneSchedule       Schedule { get; protected set; }
        public  bool                IsFavorite { get; protected set; }

        public  ScenePalette        Palette => SceneMode.Equals( SceneMode.Animating ) ? AnimationPalette : StationaryPalette;

        protected Scene() {
            SceneName = String.Empty;
            AnimationPalette = ScenePalette.Default;
            StationaryPalette = ScenePalette.Default;
            Parameters = SceneParameters.Default;
            Control = SceneControl.Default;
            Lights = new List<LightSource>();
            Schedule = SceneSchedule.Default;
            IsFavorite = false;
            SceneMode = SceneMode.Stationary;
        }

        public Scene( string sceneName, SceneMode sceneMode, ScenePalette animationPalette, ScenePalette stationaryPalette,
                      SceneParameters parameters, SceneControl control, IEnumerable<LightSource> lights, SceneSchedule schedule ) {
            SceneName = sceneName;
            SceneMode = sceneMode;
            AnimationPalette = animationPalette;
            StationaryPalette = stationaryPalette;
            Parameters = parameters;
            Control = control;
            Lights = new List<LightSource>( lights );
            Schedule = schedule;
        }

        public void UpdateFrom( Scene scene ) {
            SceneName = scene.SceneName;
            SceneMode = scene.SceneMode;
            AnimationPalette = scene.AnimationPalette;
            StationaryPalette = scene.StationaryPalette;
            Parameters = scene.Parameters;
            Control = scene.Control;
            Lights = scene.Lights;
            Schedule = scene.Schedule;
            IsFavorite = scene.IsFavorite;
        }

        public void SetMode( SceneMode mode ) {
            SceneMode = mode;
        }

        public void SetFavorite( bool state ) {
            IsFavorite = state;
        }

        public void Update( IList<LightSource> lighting ) {
            Lights = lighting;
        }

        public void Update( ScenePalette palette ) {
            AnimationPalette = palette;
        }

        public void Update( SceneParameters parameters ) {
            Parameters = parameters;
        }

        public void Update( SceneSchedule schedule ) {
            Schedule = schedule;
        }

        public void Update( SceneControl control ) {
            Control = control;
        }
    }
}
