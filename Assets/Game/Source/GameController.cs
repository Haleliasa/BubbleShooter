using Bubbles;
using Field;
using Levels;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Game {
    public class GameController : MonoBehaviour {
        [SerializeField]
        private Field.Field field;

        [SerializeField]
        private BubbleShooter shooter;

        [SerializeField]
        private ColorConfig colorConfig;

        private ILevelLoader levelLoader;
        private IFieldObjectFactory fieldObjectFactory;

        public void Init(
            ILevelLoader levelLoader,
            IFieldObjectFactory fieldObjectFactory) {
            this.levelLoader = levelLoader;
            this.fieldObjectFactory = fieldObjectFactory;
            _ = StartGame();
        }

        private async Task StartGame() {
            IReadOnlyList<LevelInfo> levels = await this.levelLoader.LoadLevels();
            LevelInfo levelInfo = levels[Random.Range(0, levels.Count)];
            LevelData? level = await this.levelLoader.LoadLevel(levelInfo.index);

            if (!level.HasValue) {
                return;
            }

            this.field.Init(level.Value.items, this.colorConfig.Colors, this.fieldObjectFactory);
            this.shooter.Init(this.colorConfig.Colors, level.Value.shotCount);
        }
    }
}
