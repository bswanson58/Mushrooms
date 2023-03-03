using Mushrooms.Entities;

namespace Mushrooms.Database {
    internal interface ISceneProvider : IEntityCache<Scene> { }

    internal class SceneCache : EntityCache<Scene>, ISceneProvider {

        public SceneCache( ISceneDatabaseProvider entityProvider )
            : base( entityProvider ) { }
    }
}
