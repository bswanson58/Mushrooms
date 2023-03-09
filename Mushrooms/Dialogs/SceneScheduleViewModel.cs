using Mushrooms.Entities;
using ReusableBits.Wpf.DialogService;
using System;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class SceneScheduleViewModel : DialogAwareBase {
        public  const string        cSchedule = "schedule";

        private ScheduleTimeType    mOnTimeType;
        private ScheduleTimeType    mOffTimeType;
        private bool                mIsEnabled;

        public  DateTime            OnTime { get; set; }
        public  DateTime            OffTime { get; set; }
        public  TimeSpan            Duration { get; set; }

        public SceneScheduleViewModel() {
            Title = "Scene Schedule";

            mOnTimeType = ScheduleTimeType.SpecificTime;
            mOffTimeType = ScheduleTimeType.SpecificTime;

            OnTime = DateTime.MinValue;
            OffTime = DateTime.MaxValue;

            mIsEnabled = false;
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            var schedule = parameters.GetValue<SceneSchedule>( cSchedule ) ?? SceneSchedule.Default;

            var date = new DateTime( 2023, 1, 1 );

            OnTimeType = schedule.OnTimeType;
            OnTime = date + schedule.OnTime.ToTimeSpan();

            OffTimeType = schedule.OffTimeType;
            OffTime = date + schedule.OffTime.ToTimeSpan();

            IsEnabled = schedule.Enabled;

            RaiseAllPropertiesChanged();
        }

        public bool IsEnabled {
            get => mIsEnabled;
            set {
                mIsEnabled = value;

                RaisePropertyChanged( () => IsEnabled );
            }
        }

        public ScheduleTimeType OnTimeType {
            get => mOnTimeType;
            set {
                mOnTimeType = value;

                RaisePropertyChanged( () => OnTimeType );
            }
        }

        public ScheduleTimeType OffTimeType {
            get => mOffTimeType;
            set {
                mOffTimeType = value;

                RaisePropertyChanged( () => OffTimeType );
            }
        }

        protected override DialogParameters CreateClosingParameters() {
            var schedule = new SceneSchedule( OnTimeType, TimeOnly.FromDateTime( OnTime ), 
                                              OffTimeType, TimeOnly.FromDateTime( OffTime ), Duration,
                                              IsEnabled );

            return new DialogParameters{{ cSchedule, schedule }};
        }
    }
}
