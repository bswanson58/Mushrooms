using ReusableBits.Wpf.EventAggregator;
using System.Diagnostics;
using System;
using System.Windows.Input;
using Mushrooms.Dialogs;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MainWindowViewModel : IHandle<Events.DisplayExplorerRequest> {
        private readonly IDialogService mDialogService;
        private readonly IBasicLog      mLog;

        public  ICommand            Configuration { get; }

        public MainWindowViewModel( IDialogService dialogService, IEventAggregator eventAggregator, IBasicLog log ) {
            mDialogService = dialogService;
            mLog = log;

            Configuration = new DelegateCommand( OnConfiguration );

            eventAggregator.Subscribe( this );
        }

        public void Handle( Events.DisplayExplorerRequest eventArgs ) {
            try {
                var startInfo = new ProcessStartInfo {
                    Arguments = eventArgs.Target,
                    FileName = "explorer.exe"
                };

                Process.Start( startInfo );    
            }
            catch( Exception ex ) {
                mLog.LogException( "OnLaunchRequest:", ex );
            }
        }

        private void OnConfiguration() {
            mDialogService.ShowDialog<ConfigurationView>();
        }
    }
}
