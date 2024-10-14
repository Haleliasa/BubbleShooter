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
        private TMP_Text title;

        [SerializeField]
        private TMP_Text body;

        [SerializeField]
        private Transform buttonContainer;

        [SerializeField]
        private DialogButton<T> buttonPrefab;

        private TaskCompletionSource<T> resultTaskSource;
        private readonly List<DialogButton<T>> buttons = new();

        public Task<T> Result => this.resultTaskSource.Task;
    
        public static Dialog<T> Show(
            Dialog<T> prefab,
            string title,
            string body,
            IEnumerable<DialogOption<T>> options,
            Transform container) {
            Dialog<T> dialog = Instantiate(prefab);
            dialog.Show(title, body, options, container);
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
            Transform container) {
            this.title.text = title;
            this.body.text = body;
            foreach (DialogOption<T> option in options) {
                DialogButton<T> button = Instantiate(this.buttonPrefab);
                button.transform.SetParent(this.buttonContainer, worldPositionStays: false);
                button.Init(option);
                button.Clicked += OnButtonClicked;
                this.buttons.Add(button);
            }
            transform.SetParent(container, worldPositionStays: false);
            this.resultTaskSource = new TaskCompletionSource<T>();
        }

        private void OnButtonClicked(DialogButton<T> button) {
            this.resultTaskSource.TrySetResult(button.Option);
            Destroy(gameObject);
        }

        private void Subscribe() {
            foreach (DialogButton<T> button in this.buttons) {
                button.Clicked += OnButtonClicked;
            }
        }

        private void Unsubscribe() {
            foreach (DialogButton<T> button in this.buttons) {
                button.Clicked -= OnButtonClicked;
            }
        }
    }
}
