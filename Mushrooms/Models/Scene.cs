using System;
using System.Collections.Generic;
using HueLighting.Models;
using Mushrooms.Database;

namespace Mushrooms.Models {
    internal record Scene : EntityBase {
        public  string          SceneName { get; init; }    = String.Empty;
        public  ScenePlan       Plan { get; init; }         = new ();
        public  SceneControl    Control { get; init; }      = new ();
        public  IList<Bulb>     Bulbs { get; init; }        = new List<Bulb>();
    }
}
