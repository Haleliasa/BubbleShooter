using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Game {
    public class Slingshot : MonoBehaviour, IDragHandler, IEndDragHandler {
        [SerializeField]
        private RectTransform container;

        [SerializeField]
        private LineRenderer line;

        [SerializeField]
        private new Camera camera;

        [Range(0f, 1f)]
        [SerializeField]
        private float minDistance = 0.01f;

        [Range(0f, 1f)]
        [SerializeField]
        private float maxDistance = 0.1f;

        public Vector2 Direction { get; private set; }

        public float Distance { get; private set; }

        public event Action<ShotData> Shot;

        void IDragHandler.OnDrag(PointerEventData eventData) {
            UpdateData(eventData.pressPosition, eventData.position);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
            UpdateData(eventData.pressPosition, eventData.position);
            if (Distance > 0f) {
                Shot?.Invoke(new ShotData(this, Direction, Distance));
            }
            ResetData();
        }

        private void Start() {
            ResetData();
        }

        private void UpdateData(Vector2 screenFrom, Vector2 screenTo) {
            Vector2 vec = ToContainer(screenTo) - ToContainer(screenFrom);
            float dist = vec.magnitude;

            if (Mathf.Approximately(dist, 0f)) {
                ResetData();
                return;
            }

            float size = GetContainerSize();
            float relativeDist = dist / size;

            if (relativeDist < this.minDistance) {
                ResetData();
                return;
            }

            float lineFraction = 1f;
            if (relativeDist > this.maxDistance) {
                lineFraction = this.maxDistance / relativeDist;
                relativeDist = this.maxDistance;
            }

            Direction = -vec / dist;
            Distance = relativeDist / this.maxDistance;
            
            this.line.positionCount = 2;
            this.line.SetPosition(0, ToWorld(screenFrom));
            this.line.SetPosition(1, ToWorld(screenFrom + ((screenTo - screenFrom) * lineFraction)));
        }

        private void ResetData() {
            Direction = Vector2.zero;
            Distance = 0f;
            this.line.positionCount = 0;
        }

        private Vector2 ToContainer(Vector2 screenPos) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                this.container,
                screenPos,
                this.camera,
                out Vector2 rectPos);
            return rectPos;
        }

        private float GetContainerSize() {
            return Mathf.Max(this.container.rect.width, this.container.rect.height);
        }

        private Vector3 ToWorld(Vector3 screenPos) {
            screenPos = this.camera.ScreenToWorldPoint(screenPos);
            screenPos.z = 0f;
            return screenPos;
        }

        public readonly struct ShotData {
            public ShotData(Slingshot slingshot, Vector2 direction, float distance) {
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
