using Bubbles;
using Field;
using Levels;
using System.IO;
using UnityEngine;

namespace Game {
    public class GameBootstrap : MonoBehaviour {
        [SerializeField]
        private GameController gameController;

        [SerializeField]
        private StaticBubble staticBubblePrefab;

        private void Start() {
            this.gameController.Init(
                new StaticBubbleFactory(this.staticBubblePrefab),
                new DefaultLevelLoader(
                    Application.dataPath,
                    Path.Combine(Application.dataPath, "Levels"),
                    Path.Combine(Application.dataPath, "levels")));
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
