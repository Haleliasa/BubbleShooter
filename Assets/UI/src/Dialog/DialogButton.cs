using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BubbleShooter.UI.Dialog {
    public sealed class DialogButton : MonoBehaviour {
        [SerializeField]
        private Button button;

        [SerializeField]
        private TMP_Text text;

        public string Text => this.text.text;

        public event Action<DialogButton> Clicked;

        public void Init(string text) {
            this.text.text = text;
        }

        private void OnEnable() {
            this.Subscribe();
        }

        private void OnDisable() {
            this.Unsubscribe();
        }

        private void OnClick() {
            Clicked?.Invoke(this);
        }

        private void Subscribe() {
            this.button.onClick.AddListener(this.OnClick);
        }

        private void Unsubscribe() {
            this.button.onClick.RemoveListener(this.OnClick);
        }
    }
}
