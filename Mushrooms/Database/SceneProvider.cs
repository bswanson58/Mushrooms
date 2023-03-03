using LiteDB;
using Mushrooms.Entities;
using ReusableBits.Platform.Preferences;

namespace Mushrooms.Database {
    internal interface ISceneProvider : IEntityProvider<Scene> { }

    internal class SceneProvider : LiteDatabaseProvider<Scene>, ISceneProvider {
        public SceneProvider( IEnvironment environment )
            : base( environment ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<Scene>().Id( e => e.Id );
        }
    }
}
