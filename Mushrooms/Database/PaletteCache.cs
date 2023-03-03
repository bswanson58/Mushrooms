using Mushrooms.Entities;

namespace Mushrooms.Database {
    internal  interface IPaletteProvider : IEntityCache<ScenePalette> { }

    internal class PaletteCache : EntityCache<ScenePalette>, IPaletteProvider {

        public PaletteCache( IPaletteDatabaseProvider entityProvider )
            : base( entityProvider ) { }
    }
}
