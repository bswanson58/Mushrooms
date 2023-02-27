using System;

namespace ReusableBits.Wpf.Platform {
    public interface IApplicationConstants {
        String  ApplicationName { get; }
        String  CompanyName { get; }

        String  ConfigurationDirectory { get; }
        String  LogFileDirectory { get; }

    }
}
