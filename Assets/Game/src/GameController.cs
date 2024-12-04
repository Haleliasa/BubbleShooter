#nullable enable

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
        private Canvas canvas = null!;

        [SerializeField]
        private TMP_Text? scoreText;

        [SerializeField]
        private Dialog<bool>? dialogPrefab;

        [Scene]
        [SerializeField]
        private string menuScene = null!;

        private IFieldObjectFactory fieldObjectFactory = null!;
        private ILevelLoader levelLoader = null!;
        private IObjectPool<FieldCell>? fieldCellPool;
        private IObjectPool<ProjectileBubble>? projectilePool;
        private IObjectPool<Dialog<bool>>? dialogPool;
        private IObjectPool<DialogButton<bool>>? dialogButtonPool;
        private LevelData? currentLevel;
        private int score;
        private bool subbed = false;

        public void Init(
            IFieldObjectFactory fieldObjectFactory,
            ILevelLoader levelLoader,
            IObjectPool<FieldCell>? fieldCellPool = null,
            IObjectPool<ProjectileBubble>? projectilePool = null,
            IObjectPool<Dialog<bool>>? dialogPool = null,
            IObjectPool<DialogButton<bool>>? dialogButtonPool = null) {
            this.fieldObjectFactory = fieldObjectFactory;
            this.levelLoader = levelLoader;
            this.fieldCellPool = fieldCellPool;
            this.projectilePool = projectilePool;
            this.dialogPool = dialogPool;
            this.dialogButtonPool = dialogButtonPool;
            StartGame(level: null).FireAndForget();
        }

        public void GoMenu() {
            GoMenuInternal(ask: true).FireAndForget();
        }

        private async Task StartGame(LevelData? level) {
            level ??= await LoadRandomLevel();

            if (level == null) {
                ProcessLevelUnavailable().FireAndForget();
                return;
            }

            this.currentLevel = level;
            this.field.Init(
                level.Value.items,
                this.config.Colors,
                this.fieldObjectFactory,
                cellPool: this.fieldCellPool);
            this.shooter.Init(
                level.Value.items
                    .Select(i => i.colorIndex)
                    .Distinct()
                    .Select(i => this.config.Colors[i]),
                level.Value.shotCount,
                projectilePool: this.projectilePool);
            this.shooter.Prepare();
            SetScore(0);

            if (!this.subbed) {
                Subscribe();
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
                this.dialogPool,
                this.dialogPrefab,
                win ? "You win!" : "You lost :(",
                "Play again?",
                Dialog.YesNoOptions(),
                this.canvas.transform,
                buttonPool: this.dialogButtonPool);
            Task<bool> resultTask = dialog.Result;
            yield return new WaitUntil(() => resultTask.IsCompleted);

            if (!resultTask.Result) {
                GoMenuInternal(ask: false).FireAndForget();
                yield break;
            }

            StartGame(level: win ? null : this.currentLevel).FireAndForget();
        }

        private async Task ProcessLevelUnavailable() {
            bool result = await Dialog.Show(
                this.dialogPool,
                this.dialogPrefab,
                "Level unavailable",
                "Try again?",
                Dialog.YesNoOptions(),
                this.canvas.transform,
                buttonPool: this.dialogButtonPool).Result;
            if (!result) {
                GoMenuInternal(ask: false).FireAndForget();
                return;
            }
            StartGame(level: null).FireAndForget();
        }

        private async Task GoMenuInternal(bool ask) {
            if (ask) {
                bool result = await Dialog.Show(
                    this.dialogPool,
                    this.dialogPrefab,
                    "Leave",
                    "Are you sure?",
                    Dialog.YesNoOptions(),
                    this.canvas.transform,
                    buttonPool: this.dialogButtonPool).Result;
                if (!result) {
                    return;
                }
            }
            SceneManager.LoadScene(this.menuScene);
        }

        private void AddScore(int deltaScore) {
            SetScore(this.score + deltaScore);
        }

        private void SetScore(int score) {
            this.score = score;
            if (this.scoreText != null) {
                this.scoreText.text = this.score.ToString();
            }
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
