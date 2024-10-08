using Bubbles;
using Field;
using Game;
using UnityEngine;

public class GameBootstrap : MonoBehaviour {
    [SerializeField]
    private GameController gameController;

    [SerializeField]
    private StaticBubble staticBubblePrefab;

    private void Start() {
        //this.gameController.Init(
        //    defaultLevelLoader,
        //    new StaticBubbleFactory(this.staticBubblePrefab));
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
