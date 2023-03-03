using System;
using System.Collections.Generic;
using HueLighting.Models;
using Mushrooms.Database;

namespace Mushrooms.Entities {
    internal class Scene : EntityBase {
        // protected sets are for LiteDB.
        public  string          SceneName { get; protected set; }
        public  ScenePalette    Palette { get; protected  set; }
        public  SceneParameters Parameters { get; protected  set; }
        public  SceneControl    Control { get; protected set; }
        public  IList<Bulb>     Bulbs { get; protected set; }
        public  SceneSchedule   Schedule { get; protected set; }

        protected Scene() {
            SceneName = String.Empty;
            Palette = ScenePalette.Default;
            Parameters = SceneParameters.Default;
            Control = SceneControl.Default;
            Bulbs = new List<Bulb>();
            Schedule = SceneSchedule.Default;
        }

        public Scene( string sceneName, ScenePalette palette, SceneParameters parameters, SceneControl control,     
                      IEnumerable<Bulb> bulbs, SceneSchedule schedule ) {
            SceneName = sceneName;
            Palette = palette;
            Parameters = parameters;
            Control = control;
            Bulbs = new List<Bulb>( bulbs );
            Schedule = schedule;
        }
    }
}
