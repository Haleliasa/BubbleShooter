#nullable enable

using UI.Dialog;
using UnityEditor;
using UnityEngine;

namespace UI.Pages {
    public class MenuPage : UiPage {
        [SerializeField]
        private UiCommand newGameCommand = null!;

        [SerializeField]
        private UiCommand aboutCommand = null!;

        [SerializeField]
        private UiCommand quitCommand = null!;

        private IDialogService dialogService = null!;

        public void Init(IDialogService dialogService) {
            this.dialogService = dialogService;
        }

        protected override void OnOpen() {
            this.Subscribe();
        }

        protected override void OnClose() {
            this.Unsubscribe();
        }

        private void StartNewGame(UiCommand command) {
            this.Controller.GoTo(UiPages.game);
        }

        private void GoAbout(UiCommand command) {
            this.Controller.GoTo(UiPages.about);
        }

        private async void Quit(UiCommand command) {
            bool dialogRes = await this.dialogService.OpenAsync(
                "Quit",
                "Are you sure?",
                DialogOptions.YesNo()
            ).result;

            if (!dialogRes) {
                return;
            }

#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Subscribe() {
            this.newGameCommand.Executed += this.StartNewGame;
            this.aboutCommand.Executed += this.GoAbout;
            this.quitCommand.Executed += this.Quit;
        }

        private void Unsubscribe() {
            this.newGameCommand.Executed -= this.StartNewGame;
            this.aboutCommand.Executed -= this.GoAbout;
            this.quitCommand.Executed -= this.Quit;
        }
    }
}
