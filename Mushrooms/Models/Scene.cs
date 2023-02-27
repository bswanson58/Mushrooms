using System;

namespace Mushrooms.Models {
    internal record Scene {
        public  string          SceneName { get; init; }    = String.Empty;
        public  ScenePlan       Plan { get; init; }         = new ();
        public  SceneControl    Control { get; init; }      = new ();
    }
}
