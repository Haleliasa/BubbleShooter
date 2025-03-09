#nullable enable

using BubbleShooter.Field;
using System;
using UnityEngine;

namespace BubbleShooter.Bubbles {
    [RequireComponent(typeof(Bubble))]
    public class StaticBubble : MonoBehaviour, IFieldObject {
        private Bubble bubble = null!;

        public void Init(Color color, IDisposable? pooled = null) {
            if (this.bubble == null) {
                this.bubble = GetComponent<Bubble>();
            }
            this.bubble.Init(color, pooled: pooled);
        }

        void IFieldObject.Init(Transform position) {
            transform.SetParent(position);
            transform.localPosition = Vector3.zero;
            this.bubble.Pin(position.position);
        }

        void IFieldObject.Detach() {
            this.bubble.transform.SetParent(null, worldPositionStays: true);
        }

        void IFieldObject.Destroy(FieldObjectDestroyType type) {
            this.bubble.Destroy(type);
        }
    }
}
