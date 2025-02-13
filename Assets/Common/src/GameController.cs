#nullable enable

using Bubbles;
using Levels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI;
using UI.Dialog;
using UI.Pages;
using UnityEditor;
using UnityEngine;

public class GameController : MonoBehaviour, IGameController {
    [SerializeField]
    private GameConfig config = null!;

    private Field.Field field = null!;
    private BubbleShooter shooter = null!;
    private ILevelLoader levelLoader = null!;
    private IObjectPool<ProjectileBubble>? projectilePool;
    private IUiController uiController = null!;
    private IDialogService dialogService = null!;
    private LevelData? currentLevel;
    private int score;
    private bool subbed = false;

    public event Action<int>? ScoreChanged;

    public void Init(
        Field.Field field,
        BubbleShooter shooter,
        ILevelLoader levelLoader,
        IObjectPool<ProjectileBubble> projectilePool,
        IUiController uiController,
        IDialogService dialogService
    ) {
        this.field = field;
        this.shooter = shooter;
        this.levelLoader = levelLoader;
        this.projectilePool = projectilePool;
        this.uiController = uiController;
        this.dialogService = dialogService;

        this.StartGame(level: null).FireAndForget();
    }

    private async Task StartGame(LevelData? level) {
        level ??= await this.LoadRandomLevel();

        if (level == null) {
            this.ProcessLevelUnavailable().FireAndForget();
                
            return;
        }

        this.currentLevel = level;
            
        this.field.Restart(level.Value.items, this.config.Colors);

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
            
        LevelInfo levelInfo = levels[UnityEngine.Random.Range(0, levels.Count)];
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
                this.GoMenu();

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
            this.GoMenu();
            
            return;
        }
            
        this.StartGame(level: null).FireAndForget();
    }

    private void GoMenu() {
        this.uiController.GoTo(UiPages.menu);
    }

    private void AddScore(int deltaScore) {
        this.SetScore(this.score + deltaScore);
    }

    private void SetScore(int score) {
        this.score = score;
        this.ScoreChanged?.Invoke(this.score);
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
