using ReusableBits.Platform.Interfaces;

namespace ReusableBits.Platform.Preferences {
    public interface IPreferences {
        T		Load<T>() where T : new();
        T		Load<T>( string path ) where T : new();
        void	Save<T>( T preferences );
        void	Save<T>( T preferences, string path );
    }

    public class PreferencesManager : IPreferences {
        private readonly IEnvironment	mEnvironment;
        private readonly IFileWriter	mWriter;
        private readonly IBasicLog      mLog;

        public PreferencesManager( IEnvironment noiseEnvironment, IFileWriter writer, IBasicLog log   ) {
            mEnvironment = noiseEnvironment;
            mWriter = writer;
            mLog = log;
        }

        protected virtual T CreateDefault<T>() =>
            Activator.CreateInstance<T>();

        public T Load<T>() where T : new() {
            return Load<T>( Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ));
        }

        public T Load<T>( string path ) where T : new() {
            var retValue = default( T );

            try {
                retValue = mWriter.Read<T>( path );
            }
            catch( Exception ex ) {
                mLog.LogException( $"Loading Preferences failed for '{typeof( T ).Name}'", ex );
            }

            return retValue ?? CreateDefault<T>();
        }

        public void Save<T>( T preferences ) {
            Save( preferences, Path.Combine( mEnvironment.PreferencesDirectory(), typeof( T ).Name ));
        }

        public void Save<T>( T preferences, string path ) {
            if( preferences != null ) {
                try {
                    mWriter.Write( path, preferences );
                }
                catch( Exception ex ) {
                    mLog.LogException( $"Saving Preferences failed for '{typeof( T ).Name}'", ex );
                }
            }
        }
    }
}
