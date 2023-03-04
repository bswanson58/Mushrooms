using System;
using System.IO;
using Mushrooms.Entities;
using ReusableBits.Platform.Interfaces;
using ReusableBits.Platform.Preferences;

namespace Mushrooms.Database {
    internal interface IPictureCache {
        void    SavePicture( ScenePalette forPalette, string fileName );
        string  GetPalettePictureFile( ScenePalette forPalette );
    }

    internal class PictureCache : IPictureCache {
        private readonly IEnvironment   mEnvironment;
        private readonly IBasicLog      mLog;

        public PictureCache( IEnvironment environment, IBasicLog log ) {
            mEnvironment = environment;
            mLog = log;
        }

        public void SavePicture( ScenePalette forPalette, string fileName ) {
            try {
                if( File.Exists( fileName )) {
                    var targetFile = 
                        Path.ChangeExtension( 
                            Path.Combine( mEnvironment.PictureDirectory(), forPalette.Id ), 
                            Path.GetExtension( fileName ));

                    File.Copy( fileName, targetFile );
                }
            }
            catch( Exception ex ) {
                mLog.LogException( $"Saving picture for palette '{forPalette.Name}' from: {fileName}", ex );
            }
        }

        public string GetPalettePictureFile( ScenePalette forPalette ) {
            var retValue = String.Empty;
            var files = Directory.GetFiles( mEnvironment.PictureDirectory(), $"{forPalette.Id}.*" );

            if( files.Length == 1 ) {
                retValue = files[0];
            }

            return retValue;
        }
    }
}
