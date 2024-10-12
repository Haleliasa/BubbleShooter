using Bubbles;
using Field;
using Levels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEditor;
using UnityEngine;

namespace Game {
    public class GameController : MonoBehaviour {
        [SerializeField]
        private GameConfig config;

        [SerializeField]
        private Field.Field field;

        [SerializeField]
        private BubbleShooter shooter;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private TMP_Text scoreText;

        private IFieldObjectFactory fieldObjectFactory;
        private ILevelLoader levelLoader;
        private int score;
        private bool subbed = false;

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
            this.shooter.Init(
                level.Value.items
                    .Select(i => i.colorIndex)
                    .Distinct()
                    .Select(i => this.config.Colors[i]),
                level.Value.shotCount);
            this.shooter.Prepare();
            SetScore(0);

            if (!this.subbed) {
                Subscribe();
                this.subbed = true;
            }
        }

        private void OnEnable() {
            if (this.subbed) {
                Subscribe();
            }
        }

        private void OnDisable() {
            if (this.subbed) {
                Unsubscribe();
            }
        }

        private void OnFieldHit(Field.Field.HitData data) {
            AddScore((data.matchCount * this.config.MatchPoints)
                + (data.isolatedCount * this.config.IsolatedPoints));
            if (data.win) {
                ProcessGameOver(win: true);
                return;
            }
            if (this.shooter.ShotCount == 0) {
                ProcessGameOver(win: false);
                return;
            }
            this.shooter.Prepare();
        }

        private void OnShooterBubbleDestroying(BubbleShooter shooter) {
            if (this.shooter.ShotCount == 0) {
                ProcessGameOver(win: false);
                return;
            }
            this.shooter.Prepare();
        }

        private void ProcessGameOver(bool win) {
            StartCoroutine(GameOverRoutine(win));
        }

        private IEnumerator GameOverRoutine(bool win) {
            yield return new WaitForSeconds(
                win
                ? this.config.WinDialogDelay
                : this.config.LoseDialogDelay);

            Dialog<bool> dialog = Dialog.Show(
                this.config.DialogPrefab,
                this.canvas.transform,
                win ? "You win!" : "You lost :(",
                "Play again?",
                Dialog.YesNoOptions());
            Task<bool> resultTask = dialog.Result;
            yield return new WaitUntil(() => resultTask.IsCompleted);

            if (!resultTask.Result) {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                yield break;
            }

            _ = StartGame();
        }

        private void AddScore(int deltaScore) {
            SetScore(this.score + deltaScore);
        }

        private void SetScore(int score) {
            this.score = score;
            this.scoreText.text = this.score.ToString();
        }

        private void Subscribe() {
            this.field.Hit += OnFieldHit;
            this.shooter.BubbleDestroying += OnShooterBubbleDestroying;
        }

        private void Unsubscribe() {
            this.field.Hit -= OnFieldHit;
            this.shooter.BubbleDestroying -= OnShooterBubbleDestroying;
        }
    }
}
