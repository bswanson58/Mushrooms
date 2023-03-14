using ReusableBits.Wpf.EventAggregator;
using System.Diagnostics;
using System;
using ReusableBits.Platform.Interfaces;

namespace Mushrooms {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MainWindowViewModel : IHandle<Events.DisplayExplorerRequest> {
        private readonly IBasicLog      mLog;

        public MainWindowViewModel( IEventAggregator eventAggregator, IBasicLog log ) {
            mLog = log;

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
    }
}
