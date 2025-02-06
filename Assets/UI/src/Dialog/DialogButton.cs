using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Dialog {
    public sealed class DialogButton : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TMP_Text text;

        private bool subbed = false;

        public string Text => this.text.text;

        public event Action<DialogButton> Clicked;

        public void Init(string text) {
            this.text.text = text;

            if (!this.subbed) {
                this.Subscribe();
                this.subbed = true;
            }
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

        private void Subscribe() {
            this.button.onClick.AddListener(this.OnClick);
        }

        private void Unsubscribe() {
            this.button.onClick.RemoveListener(this.OnClick);
        }

        private void OnClick() {
            Clicked?.Invoke(this);
        }
    }
}
