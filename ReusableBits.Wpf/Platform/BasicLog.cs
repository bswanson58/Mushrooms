using System;

namespace ReusableBits.Wpf.Platform {
    public interface IBasicLog {
        void	LogException( string message, Exception exception );
        void	LogMessage( string message );
    }
}
