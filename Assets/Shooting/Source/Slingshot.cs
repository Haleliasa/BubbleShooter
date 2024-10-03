using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Shooting {
    public class Slingshot : MonoBehaviour, IDragHandler, IEndDragHandler {
        [SerializeField]
        private RectTransform sizeRef;

        [Range(0f, 1f)]
        [SerializeField]
        private float minDistance = 0.01f;

        [Range(0f, 1f)]
        [SerializeField]
        private float maxDistance = 0.1f;

        public Vector2 Direction { get; private set; }

        public float Distance { get; private set; }

        public event Action<EventData> Shoot;

        void IDragHandler.OnDrag(PointerEventData eventData) {
            UpdateData(eventData.pressPosition, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData) {
            UpdateData(eventData.pressPosition, eventData.position);
            if (Distance > 0f) {
                Shoot?.Invoke(new EventData(this, Direction, Distance));
            }
            ResetData();
        }

        private void Start() {
            ResetData();
        }

        private void UpdateData(Vector2 start, Vector2 end) {
            Vector2 dir = end - start;
            float dist = dir.magnitude;

            if (Mathf.Approximately(dist, 0f)) {
                ResetData();
                return;
            }

            float relativeDist = dist
                / Mathf.Max(this.sizeRef.rect.width, this.sizeRef.rect.height);

            if (relativeDist < this.minDistance) {
                ResetData();
                return;
            }

            Direction = -dir / dist;
            Distance = Mathf.Lerp(0f, 1f, relativeDist / this.maxDistance);
        }

        private void ResetData() {
            Direction = Vector2.zero;
            Distance = 0f;
        }

        public readonly struct EventData {
            public EventData(Slingshot slingshot, Vector2 direction, float distance) {
                this.slingshot = slingshot;
                this.direction = direction;
                this.distance = distance;
            }

            public readonly Slingshot slingshot;
            public readonly Vector2 direction;
            public readonly float distance;
        }
    }
}
