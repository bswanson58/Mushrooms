using System;
using System.Reflection;

namespace ReusableBits.Wpf.Platform {
    public static class AssemblyInfo {
        public static string Company =>         GetExecutingAssemblyAttribute<AssemblyCompanyAttribute>( a => a.Company );
        public static string Product =>         GetExecutingAssemblyAttribute<AssemblyProductAttribute>( a => a.Product );
        public static string Copyright =>       GetExecutingAssemblyAttribute<AssemblyCopyrightAttribute>( a => a.Copyright );
        public static string Trademark =>       GetExecutingAssemblyAttribute<AssemblyTrademarkAttribute>( a => a.Trademark );
        public static string Title =>           GetExecutingAssemblyAttribute<AssemblyTitleAttribute>( a => a.Title );
        public static string Description =>     GetExecutingAssemblyAttribute<AssemblyDescriptionAttribute>( a => a.Description );
        public static string Configuration =>   GetExecutingAssemblyAttribute<AssemblyConfigurationAttribute>( a => a.Configuration );
        public static string FileVersion =>     GetExecutingAssemblyAttribute<AssemblyFileVersionAttribute>( a => a.Version );

        public static Version ? Version =>      Assembly.GetExecutingAssembly().GetName().Version;
        public static string VersionFull =>     Version != null ? Version.ToString() : String.Empty;
        public static string VersionMajor =>    Version != null ? Version.Major.ToString() : String.Empty;
        public static string VersionMinor =>    Version != null ? Version.Minor.ToString() : String.Empty;
        public static string VersionBuild =>    Version != null ? Version.Build.ToString() : String.Empty;
        public static string VersionRevision => Version != null ? Version.Revision.ToString() : String.Empty;

        private static string GetExecutingAssemblyAttribute<T>( Func<T, string> value ) where T : Attribute {
            T ? attribute = (T ?)Attribute.GetCustomAttribute( Assembly.GetExecutingAssembly(), typeof( T ));

            if( attribute != null ) {
                return value.Invoke( attribute );
            }

            return String.Empty;
        }
    }
}
