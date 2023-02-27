namespace Mushrooms.Models {
    internal record ScenePlan {
        public  ScenePalette        Palette { get; init; }      = ScenePalette.Default;
        public  SceneParameters     Parameters { get; init; }   = SceneParameters.Default;
    }
}
