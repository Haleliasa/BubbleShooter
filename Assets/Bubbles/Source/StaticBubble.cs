using Field;
using UnityEngine;

namespace Bubbles {
    [RequireComponent(typeof(Bubble))]
    public class StaticBubble : MonoBehaviour, IFieldObject {
        private Bubble bubble;

        public void Init(Color color) {
            if (this.bubble == null) {
                this.bubble = GetComponent<Bubble>();
            }
            this.bubble.Init(color);
        }

        void IFieldObject.Init(Transform position) {
            transform.SetParent(position, worldPositionStays: false);
            this.bubble.Pin(position.position);
        }

        void IFieldObject.Destroy(FieldObjectDestroyType type) {
            this.bubble.Destroy(type);
        }
    }
}
