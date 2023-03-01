using ReusableBits.Platform.Interfaces;

namespace Mushrooms.Platform {
    internal class ApplicationConstants : IApplicationConstants {
        public string ApplicationName { get; }
        public string CompanyName { get; }
        public string ConfigurationDirectory { get; }
        public string DatabaseDirectory { get; }
        public string LogFileDirectory { get; }

        public ApplicationConstants() {
            ApplicationName = "Mushrooms";
            CompanyName = "Secret Squirrel Software";
            ConfigurationDirectory = "Configuration";
            DatabaseDirectory = "Database";
            LogFileDirectory = "Logs";
        }
    }
}
