using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UI.Dialog {
    public class DefaultDialogService : IDialogService {
        private readonly Dialog dialogPrefab;
        private readonly DialogButton dialogButtonPrefab;
        private readonly Transform container;

        public DefaultDialogService(
            Dialog dialogPrefab,
            DialogButton dialogButtonPrefab,
            Transform container
        ) {
            this.dialogPrefab = dialogPrefab;
            this.dialogButtonPrefab = dialogButtonPrefab;
            this.container = container;
        }

        public Dialog Open(
            string title,
            string body,
            IEnumerable<string> options
        ) {
            Dialog dialog = this.InstantiateDialog();
            dialog.Open(
                title,
                body,
                options,
                this.container,
                this.InstantiateDialogButton
            );

            return dialog;
        }

        public void Open(
            Dialog dialog,
            string title,
            string body,
            IEnumerable<string> options
        ) {
            dialog.Open(
                title,
                body,
                options,
                this.container,
                this.InstantiateDialogButton
            );
        }

        public DialogResult<string> OpenAsync(
            string title,
            string body,
            IEnumerable<string> options
        ) {
            Dialog dialog = this.InstantiateDialog();
            Task<string> result = dialog.OpenAsync(
                title,
                body,
                options,
                this.container,
                this.InstantiateDialogButton
            );

            return new DialogResult<string>(dialog, result);
        }

        public Task<string> OpenAsync(
            Dialog dialog,
            string title,
            string body,
            IEnumerable<string> options
        ) {
            return dialog.OpenAsync(
                title,
                body,
                options,
                this.container,
                this.InstantiateDialogButton
            );
        }

        public DialogResult<T> OpenAsync<T>(
            string title,
            string body,
            IEnumerable<DialogOption<T>> options
        ) {
            Dialog dialog = this.InstantiateDialog();
            Task<T> result = dialog.OpenAsync(
                title,
                body,
                options,
                this.container,
                this.InstantiateDialogButton
            );

            return new DialogResult<T>(dialog, result);
        }

        public Task<T> OpenAsync<T>(
            Dialog dialog,
            string title,
            string body,
            IEnumerable<DialogOption<T>> options
        ) {
            return dialog.OpenAsync(
                title,
                body,
                options,
                this.container,
                this.InstantiateDialogButton
            );
        }

        private Dialog InstantiateDialog() {
            return UnityEngine.Object.Instantiate(this.dialogPrefab);
        }

        private DialogButton InstantiateDialogButton(Dialog dialog) {
            return UnityEngine.Object.Instantiate(this.dialogButtonPrefab);
        }
    }
}
