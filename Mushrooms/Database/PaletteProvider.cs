using LiteDB;
using Mushrooms.Entities;
using ReusableBits.Platform.Preferences;

namespace Mushrooms.Database {
    internal interface IPaletteProvider : IEntityProvider<ScenePalette> { }

    internal class PaletteProvider : LiteDatabaseProvider<ScenePalette>, IPaletteProvider {
        public PaletteProvider( IEnvironment environment )
            : base( environment ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<ScenePalette>().Id( e => e.Id );
        }
    }
}
