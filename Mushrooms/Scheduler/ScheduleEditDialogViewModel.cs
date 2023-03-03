using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;
using System;
using Mushrooms.Entities;

namespace Mushrooms.Scheduler {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class ScheduleEditDialogViewModel :PropertyChangeBase, IDialogAware {
        public    const string                      cSchedule = "schedule";

        private SceneSchedule                       mSchedule;
        private ScheduleTimeType                    mOnTimeType;
        private ScheduleTimeType                    mOffTimeType;
        private bool                                mIsEnabled;

        public  string                              Title { get; }

        public  DateTime                            OnTime { get; set; }
        public  DateTime                            OffTime { get; set; }
        public  TimeSpan                            Duration { get; set; }

        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }
        public  event Action<IDialogResult>         RequestClose = delegate {  };

        public ScheduleEditDialogViewModel() {
            Title = "Scene Schedule";

            mSchedule = SceneSchedule.Default;
            mOnTimeType = ScheduleTimeType.SpecificTime;
            mOffTimeType = ScheduleTimeType.SpecificTime;

            OnTime = DateTime.MinValue;
            OffTime = DateTime.MaxValue;

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mSchedule = parameters.GetValue<SceneSchedule>( cSchedule ) ?? SceneSchedule.Default;

            var date = new DateTime( 2023, 1, 1 );

            OnTimeType = mSchedule.OnTimeType;
            OnTime = date + mSchedule.OnTime.ToTimeSpan();

            OffTimeType = mSchedule.OffTimeType;
            OffTime = date + mSchedule.OffTime.ToTimeSpan();

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

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void OnOk() {
            var schedule = new SceneSchedule( OnTimeType, TimeOnly.FromDateTime( OnTime ), 
                                              OffTimeType, TimeOnly.FromDateTime( OffTime ), Duration, IsEnabled );
            var parameters = new DialogParameters{{ cSchedule, schedule }};

            RaiseRequestClose( new DialogResult( ButtonResult.Ok, parameters ));
        }

        private void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose.Invoke( dialogResult );
        }
    }
}
