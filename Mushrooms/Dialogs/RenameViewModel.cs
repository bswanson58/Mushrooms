using System;
using ReusableBits.Wpf.DialogService;

namespace Mushrooms.Dialogs {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class RenameViewModel : DialogAwareBase {
        public const string         cTitle = "title";
        public const string         cName = "name";

        private string              mName;

        public RenameViewModel() {
            mName = String.Empty;
        }

        public override void OnDialogOpened( IDialogParameters parameters ) {
            Title = parameters.GetValue<string>( cTitle ) ?? String.Empty;
            Name = parameters.GetValue<string>( cName ) ?? String.Empty;
        }

        public string Name {
            get => mName;
            set {
                mName = value;

                RaisePropertyChanged( () => Name );
            }
        }

        protected override bool CanAccept() => 
            !String.IsNullOrWhiteSpace( mName );

        protected override DialogParameters CreateClosingParameters() =>
            new () { { cName, mName } };
    }
}
