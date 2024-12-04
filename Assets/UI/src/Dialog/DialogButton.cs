using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class DialogButton : DialogButton<bool> { }

    public class DialogButton<T> : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TMP_Text text;

        private bool subbed = false;

        public T Option { get; private set; }

        public event Action<DialogButton<T>> Clicked;

        public void Init(DialogOption<T> option) {
            this.text.text = option.text;
            Option = option.value;
            if (!this.subbed) {
                Subscribe();
                this.subbed = true;
            }
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

        private void OnClick() {
            Clicked?.Invoke(this);
        }

        private void Subscribe() {
            this.button.onClick.AddListener(OnClick);
        }

        private void Unsubscribe() {
            this.button.onClick.RemoveListener(OnClick);
        }
    }
}
