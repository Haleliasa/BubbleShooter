using BubbleShooter.Core;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BubbleShooter.UI.Dialog {
    public class PooledDialogService : IDialogService {
        private readonly IConcreteObjectPool<Dialog> dialogPool;
        private readonly IConcreteObjectPool<DialogButton> dialogButtonPool;
        private readonly Transform container;

        public PooledDialogService(
            IConcreteObjectPool<Dialog> dialogPool,
            IConcreteObjectPool<DialogButton> dialogButtonPool,
            Transform container
        ) {
            this.dialogPool = dialogPool;
            this.dialogButtonPool = dialogButtonPool;
            this.container = container;
        }

        public Dialog Open(
            string title,
            string body,
            IEnumerable<string> options
        ) {
            Dialog dialog = this.GetDialog();
            dialog.Open(
                title,
                body,
                options,
                this.container,
                this.GetDialogButton,
                close: this.ReturnDialog,
                destroyButton: this.ReturnDialogButton
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
                this.GetDialogButton,
                close: this.ReturnDialog,
                destroyButton: this.ReturnDialogButton
            );
        }

        public DialogResult<string> OpenAsync(
            string title,
            string body,
            IEnumerable<string> options
        ) {
            Dialog dialog = this.GetDialog();
            Task<string> result = dialog.OpenAsync(
                title,
                body,
                options,
                this.container,
                this.GetDialogButton,
                close: this.ReturnDialog,
                destroyButton: this.ReturnDialogButton
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
                this.GetDialogButton,
                close: this.ReturnDialog,
                destroyButton: this.ReturnDialogButton
            );
        }

        public DialogResult<T> OpenAsync<T>(
            string title,
            string body,
            IEnumerable<DialogOption<T>> options
        ) {
            Dialog dialog = this.GetDialog();
            Task<T> result = dialog.OpenAsync(
                title,
                body,
                options,
                this.container,
                this.GetDialogButton,
                close: this.ReturnDialog,
                destroyButton: this.ReturnDialogButton
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
                this.GetDialogButton,
                close: this.ReturnDialog,
                destroyButton: this.ReturnDialogButton
            );
        }

        private Dialog GetDialog() {
            return this.dialogPool.GetConcrete();
        }

        private void ReturnDialog(Dialog dialog) {
            this.dialogPool.Return(dialog);
        }

        private DialogButton GetDialogButton(Dialog dialog) {
            return this.dialogButtonPool.GetConcrete();
        }

        private void ReturnDialogButton(Dialog dialog, DialogButton button) {
            this.dialogButtonPool.Return(button);
        }
    }
}
