using LiteDB;
using Mushrooms.Entities;
using ReusableBits.Platform.Preferences;

namespace Mushrooms.Database {
    internal interface ISceneDatabaseProvider : IEntityProvider<Scene> { }

    internal class SceneProvider : LiteDatabaseProvider<Scene>, ISceneDatabaseProvider {
        public SceneProvider( IEnvironment environment )
            : base( environment ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Scene>().Id( e => e.Id );
            BsonMapper.Global.EnumAsInteger = true;
        }
    }
}
