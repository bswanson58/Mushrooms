using ReusableBits.Platform.Interfaces;

namespace Mushrooms.Platform {
    internal class ApplicationConstants : IApplicationConstants {
        public string ApplicationName { get; }
        public string CompanyName { get; }
        public string ConfigurationDirectory { get; }
        public string LogFileDirectory { get; }

        public ApplicationConstants() {
            ApplicationName = "Mushrooms";
            CompanyName = "Secret Squirrel Software, Inc.";
            ConfigurationDirectory = "Configuration";
            LogFileDirectory = "Logs";
        }
    }
}
