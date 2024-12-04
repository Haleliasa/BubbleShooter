using UI;
using UnityEngine;

namespace Menu {
    public class MenuBootstrap : MonoBehaviour {
        [SerializeField]
        private MenuController controller;

        [SerializeField]
        private ObjectPool<Dialog<bool>> dialogPool;

        [SerializeField]
        private ObjectPool<DialogButton<bool>> dialogButtonPool;
        
        private void Start() {
            this.controller.Init(
                dialogPool: this.dialogPool,
                dialogButtonPool: this.dialogButtonPool);
        }
    }
}
