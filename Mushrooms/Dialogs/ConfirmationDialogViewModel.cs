using System;
using System.Windows.Input;
using ReusableBits.Wpf.Commands;
using ReusableBits.Wpf.DialogService;
using ReusableBits.Wpf.ViewModelSupport;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ConfirmationDialogViewModel : PropertyChangeBase, IDialogAware {
        public  const string                    cTitle = "title";
        public  const string                    cMessage = "message";

        public  string                          Title { get; private set; }
        public  string                          Message { get; private set; }
        public  event Action<IDialogResult> ?   RequestClose = delegate {  };

        public  ICommand                        Ok { get; }
        public  ICommand                        Cancel { get; }

        public ConfirmationDialogViewModel() {
            Title = String.Empty;
            Message = String.Empty;

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            Title = parameters.GetValue<string>( cTitle ) ?? String.Empty;
            Message = parameters.GetValue<string>( cMessage ) ?? String.Empty;

            RaiseAllPropertiesChanged();
        }

        private void OnOk() {
            RaiseRequestClose( new DialogResult( ButtonResult.Ok ));
        }

        private void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }
    }
}
