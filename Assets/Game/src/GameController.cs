#nullable enable

using Bubbles;
using Field;
using Levels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UI.Dialog;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    public class GameController : MonoBehaviour {
        [SerializeField]
        private GameConfig config = null!;

        [SerializeField]
        private Field.Field field = null!;

        [SerializeField]
        private BubbleShooter shooter = null!;

        [SerializeField]
        private TMP_Text? scoreText;

        [Scene]
        [SerializeField]
        private string menuScene = null!;

        private IFieldObjectFactory fieldObjectFactory = null!;
        private ILevelLoader levelLoader = null!;
        private IObjectPool<FieldCell>? fieldCellPool;
        private IObjectPool<ProjectileBubble>? projectilePool;
        private IDialogService dialogService = null!;
        private LevelData? currentLevel;
        private int score;
        private bool subbed = false;

        public void Init(
            IFieldObjectFactory fieldObjectFactory,
            ILevelLoader levelLoader,
            IObjectPool<FieldCell> fieldCellPool,
            IObjectPool<ProjectileBubble> projectilePool,
            IDialogService dialogService
        ) {
            this.fieldObjectFactory = fieldObjectFactory;
            this.levelLoader = levelLoader;
            this.fieldCellPool = fieldCellPool;
            this.projectilePool = projectilePool;
            this.dialogService = dialogService;

            this.StartGame(level: null).FireAndForget();
        }

        public void GoMenu() {
            this.GoMenuInternal(ask: true).FireAndForget();
        }

        private async Task StartGame(LevelData? level) {
            level ??= await this.LoadRandomLevel();

            if (level == null) {
                this.ProcessLevelUnavailable().FireAndForget();
                
                return;
            }

            this.currentLevel = level;
            
            this.field.Init(
                level.Value.items,
                this.config.Colors,
                this.fieldObjectFactory,
                cellPool: this.fieldCellPool
            );

            this.shooter.Init(
                level.Value.items
                    .Select(i => i.colorIndex)
                    .Distinct()
                    .Select(i => this.config.Colors[i]),
                level.Value.shotCount,
                projectilePool: this.projectilePool
            );
            this.shooter.Prepare();

            this.SetScore(0);

            if (!this.subbed) {
                this.Subscribe();
                this.subbed = true;
            }
        }

        private async Task<LevelData?> LoadRandomLevel() {
            
            IReadOnlyList<LevelInfo> levels = await this.levelLoader.LoadLevels();
            if (levels.Count == 0) {
                return null;
            }
            
            LevelInfo levelInfo = levels[Random.Range(0, levels.Count)];
            LevelData? level = await this.levelLoader.LoadLevel(levelInfo.index);
            
            return level;
        }

        private void OnEnable() {
            if (this.subbed) {
                this.Subscribe();
            }
        }

        private void OnDisable() {
            if (this.subbed) {
                this.Unsubscribe();
            }
        }

        private void OnFieldHit(Field.Field.HitData data) {
            this.AddScore(
                (data.matchCount * this.config.MatchPoints)
                + (data.isolatedCount * this.config.IsolatedPoints)
            );
            
            if (data.win) {
                this.ProcessGameFinished(win: true);
            
                return;
            }

            if (this.shooter.ShotCount == 0) {
                this.ProcessGameFinished(win: false);
            
                return;
            }

            this.shooter.Prepare();
        }

        private void OnShooterBubbleDestroying(BubbleShooter shooter) {
            if (this.shooter.ShotCount == 0) {
                this.ProcessGameFinished(win: false);
            
                return;
            }

            this.shooter.Prepare();
        }

        private void ProcessGameFinished(bool win) {
            this.StartCoroutine(this.GameFinishedRoutine(win));
        }

        private IEnumerator GameFinishedRoutine(bool win) {
            yield return new WaitForSeconds(
                win
                ? this.config.WinDialogDelay
                : this.config.LoseDialogDelay
            );

            Task<GameOverOption> dialogResTask = this.dialogService.OpenAsync(
                "Game finished",
                win ? "You win!" : "You lost :(",
                Options(win)
            ).result;

            yield return new WaitUntil(() => dialogResTask.IsCompleted);

            switch (dialogResTask.Result) {
                case GameOverOption.PlayAgain:
                    this.StartGame(level: win ? null : this.currentLevel).FireAndForget();

                    break;

                case GameOverOption.PointsForAd:
                    // TODO: show ad and give extra points

                    break;

                case GameOverOption.GoMenu:
                    this.GoMenuInternal(ask: false).FireAndForget();

                    break;
            }

            static IEnumerable<DialogOption<GameOverOption>> Options(bool win) {
                yield return new DialogOption<GameOverOption>(
                    "Play again",
                    GameOverOption.PlayAgain
                );

                if (win) {
                    yield return new DialogOption<GameOverOption>(
                        "Get extra points",
                        GameOverOption.PointsForAd
                    );
                }

                yield return new DialogOption<GameOverOption>(
                    "Go to menu",
                    GameOverOption.GoMenu
                );
            }
        }

        private async Task ProcessLevelUnavailable() {
            bool dialogRes = await this.dialogService.OpenAsync(
                "Level unavailable",
                "Try again?",
                DialogOptions.YesNo()
            ).result;
            
            if (!dialogRes) {
                this.GoMenuInternal(ask: false).FireAndForget();
            
                return;
            }
            
            this.StartGame(level: null).FireAndForget();
        }

        private async Task GoMenuInternal(bool ask) {
            if (ask) {
                bool dialogRes = await this.dialogService.OpenAsync(
                    "Leave",
                    "Are you sure?",
                    DialogOptions.YesNo()
                ).result;
                
                if (!dialogRes) {
                    return;
                }
            }

            SceneManager.LoadScene(this.menuScene);
        }

        private void AddScore(int deltaScore) {
            this.SetScore(this.score + deltaScore);
        }

        private void SetScore(int score) {
            this.score = score;
            
            if (this.scoreText != null) {
                this.scoreText.text = this.score.ToString();
            }
        }

        private void Subscribe() {
            this.field.Hit += this.OnFieldHit;
            this.shooter.BubbleDestroying += this.OnShooterBubbleDestroying;
        }

        private void Unsubscribe() {
            this.field.Hit -= this.OnFieldHit;
            this.shooter.BubbleDestroying -= this.OnShooterBubbleDestroying;
        }

        private enum GameOverOption {
            PlayAgain = 0,
            PointsForAd = 1,
            GoMenu = 2,
        }
    }
}
