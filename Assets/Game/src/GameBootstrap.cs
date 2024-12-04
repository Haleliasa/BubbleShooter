using Bubbles;
using Field;
using Levels;
using UI;
using UnityEngine;

namespace Game {
    public class GameBootstrap : MonoBehaviour {
        [SerializeField]
        private GameController controller;

        [SerializeField]
        private ObjectPool<FieldCell> fieldCellPool;

        [SerializeField]
        private ObjectPool<StaticBubble> staticBubblePool;

        [SerializeField]
        private ObjectPool<ProjectileBubble> projectileBubblePool;

        [SerializeField]
        private ObjectPool<Dialog<bool>> dialogPool;

        [SerializeField]
        private ObjectPool<DialogButton<bool>> dialogButtonPool;

        [AddressablesLabel]
        [SerializeField]
        private string[] levelAddressablesLabels;

        private void Start() {
            this.controller.Init(
                new StaticBubbleFactory(this.staticBubblePool),
                new AddressablesLevelLoader(this.levelAddressablesLabels),
                fieldCellPool: this.fieldCellPool,
                projectilePool: this.projectileBubblePool,
                dialogPool: this.dialogPool,
                dialogButtonPool: this.dialogButtonPool);
        }

        private class StaticBubbleFactory : IFieldObjectFactory {
            private readonly IObjectPool<StaticBubble> pool;

            public StaticBubbleFactory(IObjectPool<StaticBubble> pool) {
                this.pool = pool;
            }

            public IFieldObject CreateFieldObject(Color color) {
                IDisposableObject<StaticBubble> bubble = this.pool.Get();
                bubble.Object.Init(color, pooled: bubble);
                return bubble.Object;
            }
        }
    }
}
