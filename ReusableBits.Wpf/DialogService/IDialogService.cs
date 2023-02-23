using System;

namespace ReusableBits.Wpf.DialogService {
    /// <summary>
    /// Interface to show modal and non-modal dialogs.
    /// </summary>
    public interface IDialogService {
#if !HAS_UWP && !HAS_WINUI
        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        void Show( string name, IDialogParameters parameters, Action<IDialogResult> callback );

        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        void Show( string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName );
#endif

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        [Obsolete("User generic method to specify the dialog content class", true)]
        void ShowDialog( string name, IDialogParameters parameters, Action<IDialogResult> callback );

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        void ShowDialog<TDialog>( IDialogParameters parameters, Action<IDialogResult> callback ) 
            where TDialog : notnull;


        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        /// <param name="windowName">The name of the hosting window registered with the IContainerRegistry.</param>
        [Obsolete("User generic method to specify the dialog content class", true)]
        void ShowDialog( string name, IDialogParameters parameters, Action<IDialogResult> callback, string windowName );

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        void ShowDialog<TDialog, TWindow>( IDialogParameters parameters, Action<IDialogResult> callback ) 
            where TDialog : notnull 
            where TWindow : notnull;
    }
}
