#nullable enable

using Field;
using Shooting;
using System;
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

        public Bubble Bubble => this.bubble;

        public Projectile Projectile => this.projectile;

        public void Init(Color color, IDisposable? pooled = null) {
            if (this.bubble == null) {
                this.bubble = GetComponent<Bubble>();
            }
            this.bubble.Init(color, pooled: pooled);

            if (this.projectile == null) {
                this.projectile = GetComponent<Projectile>();
            }
        }

        void IFieldObject.Init(Transform position) {
            this.projectile.Stop();
            transform.SetParent(position, worldPositionStays: true);
            this.attachPath = (this.bubble.Position, position.position);
            this.attachTime = 0f;
        }

        void IFieldObject.Detach() {
            this.bubble.transform.SetParent(null, worldPositionStays: true);
        }

        void IFieldObject.Destroy(FieldObjectDestroyType type) {
            this.attachPath = null;
            this.bubble.Destroy(type);
        }

        private void FixedUpdate() {
            if (this.attachPath.HasValue) {
                (Vector2 from, Vector2 to) = this.attachPath.Value;
                Attach(from, to, ref this.attachTime, Time.fixedDeltaTime, out bool finished);
                if (finished) {
                    this.attachPath = null;
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
            if (!this.projectile.IsMoving) {
                return;
            }
            IFieldCell? cell = obj.GetComponentInChildren<IFieldCell>();
            cell?.Hit(
                this,
                this.bubble.Color,
                this.bubble.Position,
                destroy: Mathf.Approximately(this.projectile.Power, 1f));
        }

        private void Attach(
            Vector2 from,
            Vector2 to,
            ref float time,
            float deltaTime,
            out bool finished) {
            time += deltaTime;
            if (time < this.fieldAttachDuration) {
                this.bubble.Move(Vector2.Lerp(from, to, time / this.fieldAttachDuration));
                finished = false;
            } else {
                this.bubble.Pin(to);
                finished = true;
            }
        }
    }
}
