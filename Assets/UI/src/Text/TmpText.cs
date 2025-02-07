#nullable enable

using TMPro;
using UnityEngine;

namespace UI.Text {
    public class TmpText : UiText {
        [SerializeField]
        private TMP_Text text = null!;

        public override string Text {
            get => this.text.text;
            set => this.text.text = value;
        }
    }
}
