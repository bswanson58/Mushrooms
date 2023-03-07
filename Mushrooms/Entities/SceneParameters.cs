﻿using System;

namespace Mushrooms.Entities {
    internal record SceneParameters {
        public  TimeSpan        BaseTransitionTime { get; init; }
        public  TimeSpan        TransitionJitter { get; init; }
        public  TimeSpan        BaseDisplayTime { get; init; }
        public  TimeSpan        DisplayTimeJitter { get; init; }

        public SceneParameters( TimeSpan transitionTime, TimeSpan transitionJitter,
                                TimeSpan displayTime, TimeSpan displayTimeJitter ) {
            BaseTransitionTime = transitionTime;
            TransitionJitter = transitionJitter;
            BaseDisplayTime = displayTime;
            DisplayTimeJitter = displayTimeJitter;
        }

        public static SceneParameters Default =>
            new( TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 3 ), 
                 TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 3 ));
    }
}