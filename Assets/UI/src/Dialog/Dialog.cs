#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace UI {
    public class Dialog : Dialog<bool> {
        public static IEnumerable<DialogOption<bool>> YesNoOptions() {
            yield return new DialogOption<bool>("Yes", true);
            yield return new DialogOption<bool>("No", false);
        }

        public static IEnumerable<DialogOption<bool>> OkOption() {
            yield return new DialogOption<bool>("OK", true);
        }
    }

    public class Dialog<T> : MonoBehaviour {
        [SerializeField]
        private TMP_Text title = null!;

        [SerializeField]
        private TMP_Text body = null!;

        [SerializeField]
        private Transform buttonContainer = null!;

        [SerializeField]
        private DialogButton<T>? buttonPrefab;

        private TaskCompletionSource<T> resultTaskSource = null!;
        private readonly List<PooledOrInst<DialogButton<T>>> buttons = new();
        private IDisposable? pooled;

        public Task<T> Result => this.resultTaskSource.Task;

        public static Dialog<T> Show(
            IObjectPool<Dialog<T>>? pool,
            Dialog<T>? prefab,
            string title,
            string body,
            IEnumerable<DialogOption<T>> options,
            Transform container,
            IObjectPool<DialogButton<T>>? buttonPool = null) {
            PooledOrInst<Dialog<T>> createdDialog =
                PooledOrInst<Dialog<T>>.Create(pool, prefab);
            Dialog<T> dialog = createdDialog.Object;
            dialog.Show(title, body, options, container, buttonPool, createdDialog.Pooled);
            return dialog;
        }

        private void OnEnable() {
            Subscribe();
        }

        private void OnDisable() {
            Unsubscribe();
        }

        private void Show(
            string title,
            string body,
            IEnumerable<DialogOption<T>> options,
            Transform container,
            IObjectPool<DialogButton<T>>? buttonPool,
            IDisposable? pooled) {
            transform.SetParent(container, worldPositionStays: false);
            this.title.text = title;
            this.body.text = body;
            foreach (DialogOption<T> option in options) {
                PooledOrInst<DialogButton<T>> createdButton =
                    PooledOrInst<DialogButton<T>>.Create(buttonPool, this.buttonPrefab);
                DialogButton<T> button = createdButton.Object;
                button.transform.SetParent(this.buttonContainer, worldPositionStays: false);
                button.Init(option);
                button.Clicked += OnButtonClicked;
                this.buttons.Add(createdButton);
            }
            this.pooled = pooled;
            this.resultTaskSource = new TaskCompletionSource<T>();
        }

        private void OnButtonClicked(DialogButton<T> clickedButton) {
            this.resultTaskSource.TrySetResult(clickedButton.Option);
            foreach (PooledOrInst<DialogButton<T>> button in this.buttons) {
                button.Destroy();
            }
            if (this.pooled != null) {
                this.pooled.Dispose();
            } else {
                Destroy(gameObject);
            }
        }

        private void Subscribe() {
            foreach (PooledOrInst<DialogButton<T>> button in this.buttons) {
                button.Object.Clicked += OnButtonClicked;
            }
        }

        private void Unsubscribe() {
            foreach (PooledOrInst<DialogButton<T>> button in this.buttons) {
                button.Object.Clicked -= OnButtonClicked;
            }
        }
    }
}
