using Bubbles;
using Field;
using Levels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Game {
    public class GameController : MonoBehaviour {
        [SerializeField]
        private GameConfig config;

        [SerializeField]
        private Field.Field field;

        [SerializeField]
        private BubbleShooter shooter;

        private IFieldObjectFactory fieldObjectFactory;
        private ILevelLoader levelLoader;

        public void Init(
            IFieldObjectFactory fieldObjectFactory,
            ILevelLoader levelLoader) {
            this.fieldObjectFactory = fieldObjectFactory;
            this.levelLoader = levelLoader;
            _ = StartGame();
        }

        private async Task StartGame() {
            IReadOnlyList<LevelInfo> levels = await this.levelLoader.LoadLevels();

            if (levels.Count == 0) {
                Debug.Log("level not found");
                return;
            }

            LevelInfo levelInfo = levels[Random.Range(0, levels.Count)];
            LevelData? level = await this.levelLoader.LoadLevel(levelInfo.index);

            if (!level.HasValue) {
                Debug.Log("level not found");
                return;
            }

            this.field.Init(level.Value.items, this.config.Colors, this.fieldObjectFactory);
            this.field.Hit += OnHit;
            this.shooter.Init(
                level.Value.items
                    .Select(i => i.colorIndex)
                    .Distinct()
                    .Select(i => this.config.Colors[i]),
                level.Value.shotCount);
        }

        private void OnDisable() {
            this.field.Hit -= OnHit;
        }

        private void OnHit(Field.Field.HitData data) {
            Debug.Log($"+{data.matchCount * 2} points");
            Debug.Log($"+{data.isolatedCount} points");
            if (data.win) {
                Debug.Log("win");
                this.field.Hit -= OnHit;
                _ = StartGame();
            } else if (this.shooter.ShotCount == 0) {
                Debug.Log("lost");
                this.field.Hit -= OnHit;
                _ = StartGame();
            }
        }
    }
}
