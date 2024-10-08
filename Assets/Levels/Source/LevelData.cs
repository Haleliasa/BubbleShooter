using System.Collections.Generic;

namespace Levels {
    public readonly struct LevelData {
        public LevelData(IEnumerable<LevelItem> items, int shotCount) {
            this.items = items;
            this.shotCount = shotCount;
        }

        public readonly IEnumerable<LevelItem> items;
        public readonly int shotCount;
    }
}
