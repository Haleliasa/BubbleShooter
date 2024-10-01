using Field;
using System.Collections;
using UnityEngine;

namespace Bubbles {
    [RequireComponent(typeof(Bubble))]
    public class BubbleStaticFieldObject : MonoBehaviour, IFieldObject {
        public Bubble Bubble { get; private set; }

        void IFieldObject.Init(Transform position) {
            transform.SetParent(position, worldPositionStays: false);
        }

        IEnumerator IFieldObject.Destroy(FieldObjectDestroyType type) {
            return Bubble.Destroy(type);
        }

        private void Awake() {
            Bubble = GetComponent<Bubble>();
        }
    }
}
