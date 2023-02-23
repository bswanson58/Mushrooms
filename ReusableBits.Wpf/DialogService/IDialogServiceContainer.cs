namespace ReusableBits.Wpf.DialogService {
    /// <summary>
    /// Container support for the DialogService class
    /// </summary>
    public interface IDialogServiceContainer {
        T   Resolve<T>() where T : notnull;
        T   Resolve<T>( string name ) where T : notnull;
    }
}
