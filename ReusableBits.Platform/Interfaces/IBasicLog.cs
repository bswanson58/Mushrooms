namespace ReusableBits.Platform.Interfaces {
    public interface IBasicLog {
        void LogException( string message, Exception exception );
        void LogMessage( string message );
    }
}
