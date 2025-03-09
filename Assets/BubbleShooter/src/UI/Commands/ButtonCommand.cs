#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace BubbleShooter.UI.Commands {
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
