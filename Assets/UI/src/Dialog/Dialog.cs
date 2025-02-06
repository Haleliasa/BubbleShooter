#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UI.Dialog {
    public sealed class Dialog : MonoBehaviour {
        [SerializeField]
        private TMP_Text title = null!;

        [SerializeField]
        private TMP_Text body = null!;

        [SerializeField]
        private Transform buttonContainer = null!;

        private readonly List<DialogButton> buttons = new();
        private Action<Dialog>? close;
        private ITypedResultSource? typedResultSource;
        private TaskCompletionSource<string>? resultSource;

        private readonly ILogger logger = Debug.unityLogger;
        private const string logTag = nameof(Dialog);

        public void Open(
            string title,
            string body,
            IEnumerable<string> options,
            Transform container,
            Func<Dialog, DialogButton> createButton,
            Action<Dialog>? close = null,
            Action<Dialog, DialogButton>? destroyButton = null
        ) {
            this.OpenInternal(title, body, options, container, createButton, close, destroyButton);
        }

        public Task<string> OpenAsync(
            string title,
            string body,
            IEnumerable<string> options,
            Transform container,
            Func<Dialog, DialogButton> createButton,
            Action<Dialog>? close = null,
            Action<Dialog, DialogButton>? destroyButton = null
        ) {
            this.OpenInternal(title, body, options, container, createButton, close, destroyButton);

            this.resultSource = new TaskCompletionSource<string>();

            return this.resultSource.Task;
        }

        public Task<T> OpenAsync<T>(
            string title,
            string body,
            IEnumerable<DialogOption<T>> options,
            Transform container,
            Func<Dialog, DialogButton> createButton,
            Action<Dialog>? close = null,
            Action<Dialog, DialogButton>? destroyButton = null
        ) {
            this.OpenInternal(
                title,
                body,
                options.Select(option => option.text),
                container,
                createButton,
                close,
                destroyButton
            );

            ResultSource<T> resultSource = new(options.Select(option => option.value));
            this.typedResultSource = resultSource;

            return resultSource.result;
        }

        public void Close() {
            if (this.close == null) {
                this.logger.LogWarning(logTag, "Closed before opened", this);

                return;
            }

            foreach (DialogButton button in this.buttons) {
                button.Clicked -= this.SelectOption;
            }

            this.typedResultSource?.Cancel();
            this.typedResultSource = null;

            this.resultSource?.SetCanceled();
            this.resultSource = null;

            this.close(this);
        }

        private void OpenInternal(
            string title,
            string body,
            IEnumerable<string> options,
            Transform container,
            Func<Dialog, DialogButton> createButton,
            Action<Dialog>? close,
            Action<Dialog, DialogButton>? destroyButton
        ) {
            this.gameObject.SetActive(true);
            this.transform.SetParent(container, worldPositionStays: false);
            this.title.text = title;
            this.body.text = body;

            this.ClearButtons(destroyButton);

            foreach (string option in options) {
                DialogButton button = createButton(this);
                button.transform.SetParent(this.buttonContainer, worldPositionStays: false);
                button.Init(option);
                button.Clicked += this.SelectOption;
                this.buttons.Add(button);
            }

            this.close = close ?? CloseDefault;

            static void CloseDefault(Dialog dialog) {
                dialog.gameObject.SetActive(false);
            }
        }

        private void SelectOption(DialogButton button) {
            int index = this.buttons.IndexOf(button);

            if (index < 0) {
                this.logger.LogError(logTag, "Unknown option selected", this);

                return;
            }

            this.typedResultSource?.SetOption(index);
            this.typedResultSource = null;

            this.resultSource?.SetResult(button.Text);
            this.resultSource = null;

            this.Close();
        }

        private void ClearButtons(Action<Dialog, DialogButton>? destroyButton) {
            if (this.buttons.Count == 0) {
                return;
            }

            destroyButton ??= DestroyButtonDefault;

            foreach (DialogButton button in this.buttons) {
                button.Clicked -= this.SelectOption;
                destroyButton(this, button);
            }

            this.buttons.Clear();

            static void DestroyButtonDefault(Dialog dialog, DialogButton button) {
                Destroy(button.gameObject);
            }
        }

        private class ResultSource<T> : ITypedResultSource {
            public readonly Task<T> result;

            private readonly T[] options;
            private readonly TaskCompletionSource<T> resultSource = new();

            public ResultSource(IEnumerable<T> options) {
                this.options = options.ToArray();

                this.resultSource = new TaskCompletionSource<T>();
                this.result = this.resultSource.Task;
            }

            public void SetOption(int index) {
                this.resultSource.SetResult(this.options[index]);
            }

            public void Cancel() {
                this.resultSource.SetCanceled();
            }
        }

        private interface ITypedResultSource {
            void SetOption(int index);

            void Cancel();
        }
    }
}
