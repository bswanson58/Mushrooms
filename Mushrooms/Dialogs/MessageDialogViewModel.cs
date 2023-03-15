using ReusableBits.Wpf.DialogService;
using System;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MessageDialogViewModel : DialogAwareBase {
        public  const string                    cTitle = "title";
        public  const string                    cMessage = "message";

        public  string                          Message { get; private set; }

        public MessageDialogViewModel() {
            Title = String.Empty;
            Message = String.Empty;
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            Title = parameters.GetValue<string>( cTitle ) ?? String.Empty;
            Message = parameters.GetValue<string>( cMessage ) ?? String.Empty;

            RaiseAllPropertiesChanged();
        }
    }
}
