using System;

namespace Mushrooms.Models {
    internal record SceneParameters {
        public  TimeSpan        BaseTransitionTime { get; }
        public  TimeSpan        TransitionJitter { get; }
        public  TimeSpan        BaseDisplayTime { get; }
        public  TimeSpan        DisplayTimeJitter { get; }

        public SceneParameters( TimeSpan transitionTime, TimeSpan transitionJitter,
                                TimeSpan displayTime, TimeSpan displayTimeJitter ) {
            BaseTransitionTime = transitionTime;
            TransitionJitter = transitionJitter;
            BaseDisplayTime = displayTime;
            DisplayTimeJitter = displayTimeJitter;
        }

        public static SceneParameters Default =>
            new( TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 5 ), 
                 TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 5 ));
    }
}
