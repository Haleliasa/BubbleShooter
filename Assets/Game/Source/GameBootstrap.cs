using Bubbles;
using Field;
using Levels;
using UnityEngine;

namespace Game {
    public class GameBootstrap : MonoBehaviour {
        [SerializeField]
        private GameController gameController;

        [SerializeField]
        private StaticBubble staticBubblePrefab;

        [AddressablesLabel]
        [SerializeField]
        private string[] levelAddressablesLabels;

        private void Start() {
            this.gameController.Init(
                new StaticBubbleFactory(this.staticBubblePrefab),
                new AddressablesLevelLoader(this.levelAddressablesLabels));
        }

        private class StaticBubbleFactory : IFieldObjectFactory {
            private readonly StaticBubble prefab;

            public StaticBubbleFactory(StaticBubble prefab) {
                this.prefab = prefab;
            }

            public IFieldObject CreateFieldObject(Color color) {
                StaticBubble bubble = Instantiate(this.prefab);
                bubble.Init(color);
                return bubble;
            }
        }
    }
}
