using System;
using System.Collections.Generic;
using Mushrooms.Database;

namespace Mushrooms.Entities {
    internal class Scene : EntityBase {
        // protected sets are for LiteDB.
        public  string              SceneName { get; protected set; }
        public  ScenePalette        Palette { get; protected  set; }
        public  SceneParameters     Parameters { get; protected  set; }
        public  SceneControl        Control { get; protected set; }
        public  IList<LightSource>  Lights { get; protected set; }
        public  SceneSchedule       Schedule { get; protected set; }

        protected Scene() {
            SceneName = String.Empty;
            Palette = ScenePalette.Default;
            Parameters = SceneParameters.Default;
            Control = SceneControl.Default;
            Lights = new List<LightSource>();
            Schedule = SceneSchedule.Default;
        }

        public Scene( string sceneName, ScenePalette palette, SceneParameters parameters, SceneControl control,     
                      IEnumerable<LightSource> lights, SceneSchedule schedule ) {
            SceneName = sceneName;
            Palette = palette;
            Parameters = parameters;
            Control = control;
            Lights = new List<LightSource>( lights );
            Schedule = schedule;
        }

        public void UpdateFrom( Scene scene ) {
            SceneName = scene.SceneName;
            Palette = scene.Palette;
            Parameters = scene.Parameters;
            Control = scene.Control;
            Lights = scene.Lights;
            Schedule = scene.Schedule;
        }

        public void UpdateControl( SceneControl control ) {
            Control = control;
        }
    }
}
