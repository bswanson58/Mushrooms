using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

// Adapted from the Prism library:
// source: https://github.com/PrismLibrary/Prism
// usage: https://prismlibrary.com/docs/wpf/dialog-service.html

namespace ReusableBits.Wpf.DialogService {
    /// <summary>
    /// Implements <see cref="IDialogService"/> to show modal and non-modal dialogs.
    /// </summary>
    /// <remarks>
    /// The dialog's ViewModel must implement IDialogAware.
    /// </remarks>
    public class DialogService : IDialogService {
        private readonly IDialogServiceContainer    mContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogService"/> class.
        /// </summary>
        /// <param name="container">The <see cref="IDialogServiceContainer" /></param>
        public DialogService( IDialogServiceContainer container ) {
            mContainer = container;
        }

        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        public void Show( string name, IDialogParameters parameters, Action<IDialogResult> callback ) {
            ShowDialogInternal( name, parameters, callback, false );
        }

        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        public void Show( string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName ) {
            ShowDialogInternal( name, parameters, callback, false, windowName );
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        public void ShowDialog( string name, IDialogParameters parameters, Action<IDialogResult> callback ) {
            ShowDialogInternal( name, parameters, callback, true );
        }

        public void ShowDialog<TDialog>( IDialogParameters parameters, Action<IDialogResult> callback ) where TDialog : notnull {
            ShowDialogInternal<TDialog, IDialogWindow>( parameters, callback, true );
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        public void ShowDialog( string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName ) {
            ShowDialogInternal( name, parameters, callback, true, windowName );
        }

        public void ShowDialog<TDialog, TWindow>( IDialogParameters parameters, Action<IDialogResult> callback ) 
            where TDialog : notnull 
            where TWindow : notnull {
            ShowDialogInternal<TDialog, TWindow>( parameters, callback, true );
        }

        void ShowDialogInternal( string name, IDialogParameters ? parameters, Action<IDialogResult> callback, bool isModal, string ? windowName = null ) {
            parameters ??= new DialogParameters();
            windowName ??= String.Empty;

            IDialogWindow dialogWindow = CreateDialogWindow( windowName );
            ConfigureDialogWindowEvents( dialogWindow, callback );
            ConfigureDialogWindowContent( name, dialogWindow, parameters );

            ShowDialogWindow( dialogWindow, isModal );
        }

        void ShowDialogInternal<TDialog, TWindow>( IDialogParameters ? parameters, Action<IDialogResult> callback, bool isModal ) 
            where TDialog : notnull 
            where TWindow : notnull {
            parameters ??= new DialogParameters();

            IDialogWindow ? dialogWindow = CreateDialogWindow<TWindow>();

            if( dialogWindow != null ) {
                ConfigureDialogWindowEvents( dialogWindow, callback );
                ConfigureDialogWindowContent<TDialog>( dialogWindow, parameters );

                ShowDialogWindow( dialogWindow, isModal );
            }
        }

        /// <summary>
        /// Shows the dialog window.
        /// </summary>
        /// <param name="dialogWindow">The dialog window to show.</param>
        /// <param name="isModal">If true; dialog is shown as a modal</param>
        protected virtual void ShowDialogWindow( IDialogWindow dialogWindow, bool isModal ) {
            if( isModal )
                dialogWindow.ShowDialog();
            else
                dialogWindow.Show();
        }

        /// <summary>
        /// Create a new <see cref="IDialogWindow"/>.
        /// </summary>
        /// <param name="name">The name of the hosting window registered with the IContainerRegistry.</param>
        /// <returns>The created <see cref="IDialogWindow"/>.</returns>
        protected virtual IDialogWindow CreateDialogWindow( string name ) {
            return string.IsNullOrWhiteSpace( name ) ? 
                mContainer.Resolve<IDialogWindow>() : 
                mContainer.Resolve<IDialogWindow>( name );
        }

        protected virtual IDialogWindow ? CreateDialogWindow<TWindow>() where TWindow : notnull =>
            mContainer.Resolve<TWindow>() as IDialogWindow;

        /// <summary>
        /// Configure <see cref="IDialogWindow"/> content.
        /// </summary>
        /// <param name="dialogName">The name of the dialog to show.</param>
        /// <param name="window">The hosting window.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        protected virtual void ConfigureDialogWindowContent( string dialogName, IDialogWindow window, IDialogParameters parameters ) {
            var content = mContainer.Resolve<object>(dialogName);
            if( !( content is FrameworkElement dialogContent ) )
                throw new NullReferenceException( "A dialog's content must be a FrameworkElement" );

            MvvmHelpers.AutowireViewModel( dialogContent );

            if(!( dialogContent.DataContext is IDialogAware viewModel ))
                throw new NullReferenceException( "A dialog's ViewModel must implement the IDialogAware interface" );

            ConfigureDialogWindowProperties( window, dialogContent, viewModel );

            MvvmHelpers.ViewAndViewModelAction<IDialogAware>( viewModel, d => d.OnDialogOpened( parameters ) );
        }

        protected virtual void ConfigureDialogWindowContent<TDialog>( IDialogWindow window, IDialogParameters parameters ) where TDialog : notnull {
            var content = mContainer.Resolve<TDialog>();
            if( !( content is FrameworkElement dialogContent ) )
                throw new NullReferenceException( "A dialog's content must be a FrameworkElement" );

            MvvmHelpers.AutowireViewModel( dialogContent );

            if(!( dialogContent.DataContext is IDialogAware viewModel ))
                throw new NullReferenceException( "A dialog's ViewModel must implement the IDialogAware interface" );

            ConfigureDialogWindowProperties( window, dialogContent, viewModel );

            MvvmHelpers.ViewAndViewModelAction<IDialogAware>( viewModel, d => d.OnDialogOpened( parameters ) );
        }

        /// <summary>
        /// Configure <see cref="IDialogWindow"/> and <see cref="IDialogAware"/> events.
        /// </summary>
        /// <param name="dialogWindow">The hosting window.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        protected virtual void ConfigureDialogWindowEvents( IDialogWindow dialogWindow, Action<IDialogResult> callback ) {
            void RequestCloseHandler( IDialogResult o ) {
                dialogWindow.Result = o;
                dialogWindow.Close();
            }

            void LoadedHandler( object o, RoutedEventArgs e ) {
                dialogWindow.Loaded -= LoadedHandler;

                if( dialogWindow.GetDialogViewModel() is { } vm ) {
                    vm.RequestClose += RequestCloseHandler;
                }
            }

            dialogWindow.Loaded += LoadedHandler;

            void ClosingHandler( object ? o, CancelEventArgs e ) {
                if( !dialogWindow.GetDialogViewModel()?.CanCloseDialog() == true )
                    e.Cancel = true;
            }

            dialogWindow.Closing += ClosingHandler;

            void ClosedHandler( object ? o, EventArgs e ) {
                dialogWindow.Closed -= ClosedHandler;
                dialogWindow.Closing -= ClosingHandler;

                if( dialogWindow.GetDialogViewModel() is  { } vm ) {
                    vm.RequestClose -= RequestCloseHandler;

                    vm.OnDialogClosed();
                }

                dialogWindow.Result = new DialogResult();

                callback.Invoke( dialogWindow.Result );

                dialogWindow.DataContext = null;
                dialogWindow.Content = null;
            }

            dialogWindow.Closed += ClosedHandler;
        }

        /// <summary>
        /// Configure <see cref="IDialogWindow"/> properties.
        /// </summary>
        /// <param name="window">The hosting window.</param>
        /// <param name="dialogContent">The dialog to show.</param>
        /// <param name="viewModel">The dialog's ViewModel.</param>
        protected virtual void ConfigureDialogWindowProperties( IDialogWindow window, FrameworkElement dialogContent, IDialogAware viewModel ) {
            var windowStyle = Dialog.GetWindowStyle( dialogContent );

            if( windowStyle != null ) {
                window.Style = windowStyle;
            }

            window.Content = dialogContent;
            window.DataContext = viewModel; //we want the host window and the dialog to share the same data context

            window.Owner ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault( x => x.IsActive );
        }
    }
}
