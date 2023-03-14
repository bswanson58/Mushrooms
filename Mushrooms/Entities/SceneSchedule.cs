using System;
using Mushrooms.Support;

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
                              bool isEnabled = true ) {
            OnTimeType = onTimeType;
            OnTime = onTime;
            OffTimeType = offTimeTimeType;
            OffTime = offTime;
            OnDuration = onDuration;
            Enabled = isEnabled;
        }

        private SceneSchedule() {
            OnTimeType = ScheduleTimeType.SpecificTime;
            OnTime = TimeOnly.MinValue;
            OffTimeType = ScheduleTimeType.SpecificTime;
            OffTime = TimeOnly.MinValue;
            OnDuration = TimeSpan.MinValue;
            Enabled = false;
        }

        public void SetEnabled( bool state ) {
            Enabled = state;
        }

        public static SceneSchedule Default => new ();
    }

    internal static class SceneScheduleExtensions {
        public static DateTime StartTimeForToday( this SceneSchedule schedule, double latitude, double longitude ) {
            var retValue = DateTime.MaxValue;

            switch ( schedule.OnTimeType ) {
                case ScheduleTimeType.SpecificTime:
                    retValue = DateTime.Today.Add( schedule.OnTime.ToTimeSpan());
                    break;

                case ScheduleTimeType.Sunrise:
                    retValue = Sunrise( latitude, longitude );
                    break;

                case ScheduleTimeType.Sunset:
                    retValue = Sunset( latitude, longitude );
                    break;
            }

            return retValue;
        }

        public static DateTime StopTimeForToday( this SceneSchedule schedule, double latitude, double longitude ) {
            var retValue = DateTime.MaxValue;

            switch ( schedule.OffTimeType ) {
                case ScheduleTimeType.SpecificTime:
                    retValue = DateTime.Today.Add( schedule.OffTime.ToTimeSpan());
                    break;

                case ScheduleTimeType.Sunrise:
                    retValue = Sunrise( latitude, longitude );
                    break;

                case ScheduleTimeType.Sunset:
                    retValue = Sunset( latitude, longitude );
                    break;

                case ScheduleTimeType.Duration:
                    retValue = DateTime.Today.Add( schedule.OnDuration );
                    break;
            }

            return retValue;
        }

        private static DateTime Sunrise( double latitude, double longitude ) {
            var calculator = new CelestialCalculator();
            var data = calculator.CalculateData( latitude, longitude );

            return data.SunRise;
        }

        private static DateTime Sunset( double latitude, double longitude ) {
            var calculator = new CelestialCalculator();
            var data = calculator.CalculateData( latitude, longitude );

            return data.SunSet;
        }

        public static string ScheduleSummary( this SceneSchedule schedule, double latitude, double longitude ) {
            var startTime = schedule.StartTimeForToday( latitude, longitude ).ToShortTimeString();
            var stopTime = schedule.StopTimeForToday( latitude, longitude ).ToShortTimeString();

            switch ( schedule.OnTimeType ) {
                case ScheduleTimeType.Sunrise:
                    startTime = "Sunrise";
                    break;

                case ScheduleTimeType.Sunset:
                    startTime = "Sunset";
                    break;
            }

            switch ( schedule.OffTimeType ) {
                case ScheduleTimeType.Sunrise:
                    stopTime = "Sunrise";
                    break;

                case ScheduleTimeType.Sunset:
                    stopTime = "Sunset";
                    break;
            }

            return $"Scheduled from {startTime} to {stopTime}";
        }
    }
}
