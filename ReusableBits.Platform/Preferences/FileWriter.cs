using System.Text.Json;

namespace ReusableBits.Platform.Preferences {
    public interface IFileWriter {
        void	Write<T>( string path, T item );
        T ?		Read<T>( string path );
    }

    public class JsonObjectWriter : IFileWriter {
        public void Write<T>( string path, T item ) {
            if(!Equals( item, default( T ))) {
                var	json = JsonSerializer.Serialize( item );

                using( var file = File.CreateText( path )) {
                    file.Write( json );
                    file.Close();
                }
            }
        }

        public T ? Read<T>( string path ) {
            var retValue = default( T );

            if( File.Exists( path )) {
                using( var file = File.OpenText( path )) {
                    retValue = JsonSerializer.Deserialize<T>( file.ReadToEnd());
                }
            }

            return retValue;
        }
    }
}
