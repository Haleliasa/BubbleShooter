#nullable enable

using UnityEngine;

namespace BubbleShooter.UI {
    public class UiPage : MonoBehaviour {
        [SerializeField]
        private new string name = null!;

        public string Name => this.name;

        private IUiController controller = null!;

        public IUiController Controller => this.controller;

        public void Register(IUiController controller) {
            this.controller = controller;

            this.OnRegister();
        }

        public void Open() {
            this.gameObject.SetActive(true);

            this.OnOpen();
        }

        public void Close() {
            this.gameObject.SetActive(false);

            this.OnClose();
        }

        protected virtual void OnRegister() { }

        protected virtual void OnOpen() { }

        protected virtual void OnClose() { }
    }
}
