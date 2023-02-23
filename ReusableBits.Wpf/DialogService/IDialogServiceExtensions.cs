using System;

namespace ReusableBits.Wpf.DialogService {
    /// <summary>
    /// Extensions for the IDialogService
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IDialogServiceExtensions {
#if !HAS_UWP && !HAS_WINUI
        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        /// <param name="name">The name of the dialog to show.</param>
        public static void Show( this IDialogService dialogService, string name ) {
            dialogService.Show( name, new DialogParameters(), _ => { });
        }

        /// <summary>
        /// Shows a non-modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        public static void Show( this IDialogService dialogService, string name, Action<IDialogResult> callback ) {
            dialogService.Show( name, new DialogParameters(), callback );
        }
#endif
        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        /// <param name="name">The name of the dialog to show.</param>
        [Obsolete("User generic method to specify the dialog content class", true)]
        public static void ShowDialog( this IDialogService dialogService, string name ) {
            dialogService.ShowDialog( name, new DialogParameters(), _ => { });
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        public static void ShowDialog<TDialog>( this IDialogService dialogService ) 
            where TDialog : notnull {
            dialogService.ShowDialog<TDialog>( new DialogParameters(), _ => { });
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        /// <param name="name">The name of the dialog to show.</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        [Obsolete("User generic method to specify the dialog content class", true)]
        public static void ShowDialog( this IDialogService dialogService, string name, Action<IDialogResult> callback ) {
            dialogService.ShowDialog( name, new DialogParameters(), callback );
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        /// <param name="callback">The action to perform when the dialog is closed.</param>
        public static void ShowDialog<TDialog>( this IDialogService dialogService, Action<IDialogResult> callback ) 
            where TDialog : notnull {
            dialogService.ShowDialog<TDialog>( new DialogParameters(), callback );
        }

        /// <summary>
        /// Shows a modal dialog.
        /// </summary>
        /// <param name="dialogService">The DialogService</param>
        /// <param name="parameters">The parameters to pass to the dialog.</param>
        public static void ShowDialog<TDialog>( this IDialogService dialogService, DialogParameters parameters ) 
            where TDialog : notnull {
            dialogService.ShowDialog<TDialog>( parameters, _ => { });
        }
    }
}
