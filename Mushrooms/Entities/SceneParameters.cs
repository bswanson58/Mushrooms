using System;

namespace Mushrooms.Entities {
    internal record SceneParameters {
        public  TimeSpan        BaseTransitionTime { get; init; }
        public  TimeSpan        TransitionJitter { get; init; }
        public  TimeSpan        BaseDisplayTime { get; init; }
        public  TimeSpan        DisplayTimeJitter { get; init; }
        public  bool            AnimationEnabled { get; init; }
        public  bool            SynchronizeLights { get; init; }

        public SceneParameters( TimeSpan transitionTime, TimeSpan transitionJitter,
                                TimeSpan displayTime, TimeSpan displayTimeJitter,
                                bool animationEnabled, bool synchronizeLights ) {
            BaseTransitionTime = transitionTime;
            TransitionJitter = transitionJitter;
            BaseDisplayTime = displayTime;
            DisplayTimeJitter = displayTimeJitter;
            AnimationEnabled = animationEnabled;
            SynchronizeLights = synchronizeLights;
        }

        public static SceneParameters Default =>
            new( TimeSpan.FromSeconds( 1 ), TimeSpan.FromSeconds( 3 ), 
                 TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 3 ),
                 true, false );
    }
}
