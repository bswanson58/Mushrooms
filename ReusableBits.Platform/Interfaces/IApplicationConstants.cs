namespace ReusableBits.Platform.Interfaces {
    public interface IApplicationConstants {
        String  ApplicationName { get; }
        String  CompanyName { get; }

        String  ConfigurationDirectory { get; }
        String  LogFileDirectory { get; }

    }
}
