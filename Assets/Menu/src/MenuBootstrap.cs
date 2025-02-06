using GoogleMobileAds.Api;
using UI.Dialog;
using UnityEngine;

namespace Menu {
    public class MenuBootstrap : MonoBehaviour {
        [SerializeField]
        private MenuController controller;

        [SerializeField]
        private Canvas canvas;

        [SerializeField]
        private ObjectPool<Dialog> dialogPool;

        [SerializeField]
        private ObjectPool<DialogButton> dialogButtonPool;

        private readonly ILogger logger = Debug.unityLogger;
        private const string logTag = nameof(MenuBootstrap);

        private void Start() {
            MobileAds.Initialize(status => this.logger.Log(logTag, "Ads initialized", this));

            this.controller.Init(
                new PooledDialogService(this.dialogPool, this.dialogButtonPool, this.canvas.transform)
            );
        }
    }
}
