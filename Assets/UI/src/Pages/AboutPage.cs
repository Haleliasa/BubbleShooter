#nullable enable

using UnityEngine;

namespace UI.Pages {
    public class AboutPage : UiPage {
        [SerializeField]
        private UiCommand socialCommand = null!;

        [SerializeField]
        private string socialUrl = "";

        [SerializeField]
        private UiCommand menuCommand = null!;

        protected override void OnOpen() {
            this.Subscribe();
        }

        protected override void OnClose() {
            this.Unsubscribe();
        }

        private void OpenSocial(UiCommand command) {
            Application.OpenURL(this.socialUrl);
        }

        private void GoMenu(UiCommand command) {
            this.Controller.GoTo(UiPages.menu);
        }

        private void Subscribe() {
            this.socialCommand.Executed += this.OpenSocial;
            this.menuCommand.Executed += this.GoMenu;
        }

        private void Unsubscribe() {
            this.socialCommand.Executed -= this.OpenSocial;
            this.menuCommand.Executed -= this.GoMenu;
        }
    }
}
