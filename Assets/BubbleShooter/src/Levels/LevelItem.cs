using UnityEngine;

namespace BubbleShooter.Levels {
    public readonly struct LevelItem {
        public LevelItem(Vector2Int coords, int colorIndex) {
            this.coords = coords;
            this.colorIndex = colorIndex;
        }

        public readonly Vector2Int coords;
        public readonly int colorIndex;
    }
}
