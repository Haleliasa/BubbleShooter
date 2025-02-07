#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ButtonCommand : UiCommand {
        [SerializeField]
        private Button button = null!;

        private void OnEnable() {
            this.button.onClick.AddListener(this.Execute);
        }

        private void OnDisable() {
            this.button.onClick.RemoveListener(this.Execute);
        }
    }
}
