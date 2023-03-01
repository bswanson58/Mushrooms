using LiteDB;
using Mushrooms.Models;
using ReusableBits.Platform.Preferences;

namespace Mushrooms.Database {
    internal interface IPlanProvider : IEntityProvider<ScenePlan> { }

    internal class PlanProvider : LiteDatabaseProvider<ScenePlan>, IPlanProvider {
        public PlanProvider( IEnvironment environment )
            : base( environment ) { }

        protected override void InitializeDatabase( LiteDatabase db ) {
            BsonMapper.Global.Entity<ScenePlan>().Id( e => e.Id );
        }
    }
}
