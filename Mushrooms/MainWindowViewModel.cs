using ReusableBits.Wpf.EventAggregator;
using System.Diagnostics;
using System;
using System.Windows.Input;
using Mushrooms.Dialogs;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;
using System.Windows;
using Mushrooms.Models;
using ReusableBits.Platform.Preferences;

namespace Mushrooms {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MainWindowViewModel : PropertyChangeBase, IHandle<Events.DisplayExplorerRequest> {
        private readonly IDialogService mDialogService;
        private readonly IPreferences   mPreferences;
        private readonly IBasicLog      mLog;
        private WindowState             mStoredWindowState;
        private Window ?                mShell;

        public  ICommand                Configuration { get; }
        public  ICommand                NotificationIcon { get; }

        public  bool                    DisplayNotificationIcon { get; private set; }

        public MainWindowViewModel( IDialogService dialogService, IEventAggregator eventAggregator, 
                                    IPreferences preferences, IBasicLog log ) {
            mDialogService = dialogService;
            mPreferences = preferences;
            mLog = log;

            DisplayNotificationIcon = false;

            Configuration = new DelegateCommand( OnConfiguration );
            NotificationIcon = new DelegateCommand( OnNotifyIconClick );

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

        public void ShellLoaded( Window shell ) {
            mShell = shell;

            mShell.StateChanged += OnShellStateChanged;
            mShell.IsVisibleChanged += OnShellVisibleChanged;
            mShell.Closing += OnShellClosing;
        }

        private bool ShouldMinimizeToTray() {
            var configuration = mPreferences.Load<MushroomPreferences>();

            return configuration.ShouldMinimizeToTray;
        }

        private void OnShellStateChanged( object ? sender, EventArgs args ) {
            if( mShell != null ) {
                if( ShouldMinimizeToTray()) {
                    if( mShell.WindowState == WindowState.Minimized ) {
                        mShell.Hide();
                    }
                    else {
                        mStoredWindowState = mShell.WindowState;
                    }
                }
            }
        }

        private void OnShellVisibleChanged( object sender, DependencyPropertyChangedEventArgs e ) {
            if( ShouldMinimizeToTray()) {
                DisplayNotificationIcon = mShell?.IsVisible == false;
            }
            else {
                DisplayNotificationIcon = false;
            }

            RaisePropertyChanged( () => DisplayNotificationIcon );
        }

        private void OnShellClosing( object ? sender, System.ComponentModel.CancelEventArgs e ) {
            if( mShell != null ) {
                mShell.StateChanged -= OnShellStateChanged;
                mShell.IsVisibleChanged -= OnShellVisibleChanged;
                mShell.Closing -= OnShellClosing;
            }
        }

        private void OnNotifyIconClick() {
            ActivateShell();
        }

        private void ActivateShell() {
            if( mShell != null ) {
                mShell.Show();
                mShell.WindowState = mStoredWindowState;
                mShell.Activate();
            }
        }
    }
}
