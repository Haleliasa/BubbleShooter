#nullable enable

using BubbleShooter.UI.Dialog;
using UnityEngine;

namespace BubbleShooter.UI.Pages {
    public class GamePage : UiPage {
        [SerializeField]
        private UiCommand menuCommand = null!;

        [SerializeField]
        private UiText scoreText = null!;

        private IGameController gameController = null!;
        private IDialogService dialogService = null!;

        public void Init(IGameController gameController, IDialogService dialogService) {
            this.gameController = gameController;
            this.dialogService = dialogService;
        }

        protected override void OnOpen() {
            this.Subscribe();

            this.gameController.StartGame();
        }

        protected override void OnClose() {
            this.Unsubscribe();
        }

        private async void GoMenu(UiCommand command) {
            bool dialogRes = await this.dialogService.OpenAsync(
                "Leave",
                "Are you sure?",
                DialogOptions.YesNo()
            ).result;

            if (!dialogRes) {
                return;
            }

            this.Controller.GoTo(UiPages.menu);
        }

        private void SetScore(int score) {
            this.scoreText.Text = score.ToString();
        }

        private void Subscribe() {
            this.menuCommand.Executed += this.GoMenu;
            this.gameController.ScoreChanged += this.SetScore;
        }

        private void Unsubscribe() {
            this.menuCommand.Executed -= this.GoMenu;
            this.gameController.ScoreChanged -= this.SetScore;
        }
    }
}
