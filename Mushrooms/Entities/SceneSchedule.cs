using System;

namespace Mushrooms.Entities {
    internal enum ScheduleTimeType {
        SpecificTime,
        Sunrise,
        Sunset,
        Duration
    }

    internal class SceneSchedule {
        public  ScheduleTimeType    OnTimeType { get; protected set; }
        public  TimeOnly            OnTime { get; protected set; }
        public  ScheduleTimeType    OffTimeType { get; protected set; }
        public  TimeOnly            OffTime { get; protected set; }
        public  TimeSpan            OnDuration { get; protected set; }
        public  bool                Enabled { get; protected set; }

        public SceneSchedule( ScheduleTimeType onTimeType, TimeOnly onTime,
                              ScheduleTimeType offTimeTimeType, TimeOnly offTime, TimeSpan onDuration,
                              bool enabled ) {
            OnTimeType = onTimeType;
            OnTime = onTime;
            OffTimeType = offTimeTimeType;
            OffTime = offTime;
            Enabled = enabled;
        }
        private SceneSchedule() {
            OnTimeType = ScheduleTimeType.SpecificTime;
            OnTime = TimeOnly.MinValue;
            OffTimeType = ScheduleTimeType.SpecificTime;
            OffTime = TimeOnly.MinValue;
            OnDuration = TimeSpan.MinValue;
            Enabled = false;
        }

        public static SceneSchedule Default => new ();
    }
}
