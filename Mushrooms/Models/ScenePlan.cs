using System;
using Mushrooms.Database;

namespace Mushrooms.Models {
    internal record ScenePlan : EntityBase {
        public  string              PlanName { get; init; }    = String.Empty;
        public  ScenePalette        Palette { get; init; }      = ScenePalette.Default;
        public  SceneParameters     Parameters { get; init; }   = SceneParameters.Default;
    }
}
