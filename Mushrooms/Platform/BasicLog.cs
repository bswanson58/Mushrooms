using System;
using Microsoft.Extensions.Logging;
using ReusableBits.Platform.Interfaces;

namespace Mushrooms.Platform {
    internal class BasicLog : IBasicLog {
        private readonly ILogger<App>   mLogger;

        public BasicLog( ILogger<App> logger ) {
            mLogger = logger;
        }

        public void LogException( string message, Exception exception ) {
            mLogger.LogCritical( exception, message );
        }

        public void LogMessage( string message ) {
            mLogger.LogInformation( message );
        }
    }
}
