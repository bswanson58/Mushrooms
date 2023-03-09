using System.ComponentModel;
using System;
using Mushrooms.Resources;
using ReusableBits.Wpf.Platform;

namespace Mushrooms {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            Closing += OnWindowClosing;
        }

        private void OnWindowClosing( object ? sender, CancelEventArgs args ) {
            Settings.Default.WindowPlacement = this.GetPlacement();
            Settings.Default.Save();
        }

        protected override void OnSourceInitialized( EventArgs e ) {
            base.OnSourceInitialized( e );

            this.SetPlacement( Settings.Default.WindowPlacement );
        }
    }
}
