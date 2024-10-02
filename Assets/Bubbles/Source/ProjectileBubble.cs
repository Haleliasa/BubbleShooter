#nullable enable

using Field;
using Shooting;
using UnityEngine;

namespace Bubbles {
    [RequireComponent(typeof(Bubble))]
    [RequireComponent(typeof(Projectile))]
    public class ProjectileBubble : MonoBehaviour, IFieldObject {
        [Tooltip("sec")]
        [Min(0f)]
        [SerializeField]
        private float fieldAttachDuration = 0.1f;

        private Bubble bubble = null!;
        private Projectile projectile = null!;
        private (Vector2, Vector2)? attachPath;
        private float attachTime;

        public void Init(Color color) {
            if (this.bubble == null) {
                this.bubble = GetComponent<Bubble>();
            }
            this.bubble.Init(color);

            if (this.projectile == null) {
                this.projectile = GetComponent<Projectile>();
            }
            this.projectile.enabled = true;
        }

        void IFieldObject.Init(Transform position) {
            this.projectile.enabled = false;
            transform.SetParent(position, worldPositionStays: true);
            this.attachPath = (this.bubble.Position, position.position);
            this.attachTime = 0f;
        }

        void IFieldObject.Destroy(FieldObjectDestroyType type) {
            this.attachPath = null;
            this.bubble.Destroy(type);
        }

        private void FixedUpdate() {
            if (this.attachPath.HasValue) {
                this.attachTime += Time.fixedDeltaTime;
                (Vector2 from, Vector2 to) = this.attachPath.Value;
                if (this.attachTime < this.fieldAttachDuration) {
                    this.bubble.Move(
                        Vector2.Lerp(from, to, this.attachTime / this.fieldAttachDuration));
                } else {
                    this.attachPath = null;
                    this.bubble.Pin(to);
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            HitFieldCell(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            HitFieldCell(collision.gameObject);
        }

        private void HitFieldCell(GameObject obj) {
            if (!this.projectile.enabled) {
                return;
            }
            IFieldCell? cell = obj.GetComponentInChildren<IFieldCell>();
            cell?.Hit(this, this.bubble.Color, this.bubble.Position, destroy: false);
        }
    }
}
