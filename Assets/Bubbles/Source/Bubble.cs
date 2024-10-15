#nullable enable

using System;
using System.Collections;
using UnityEngine;

namespace Bubbles {
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpringJoint2D))]
    public class Bubble : MonoBehaviour {
        [SerializeField]
        private new SpriteRenderer renderer = null!;

        [SerializeField]
        private LayerMask floorLayers;

        [Tooltip("sec")]
        [Min(0f)]
        [SerializeField]
        private float popDuration = 0.5f;

        private new Rigidbody2D rigidbody = null!;
        private SpringJoint2D springJoint = null!;
        private Vector2? rendererSize;
        private IDisposable? pooled;

        public Color Color { get; private set; }

        public Vector2 Position => this.rigidbody.position;

        public event Action<Bubble>? Destroying;

        public void Init(Color color, IDisposable? pooled = null) {
            if (this.rigidbody == null) {
                this.rigidbody = GetComponent<Rigidbody2D>();
                this.springJoint = GetComponent<SpringJoint2D>();
            }
            this.rigidbody.isKinematic = true;
            this.springJoint.enabled = false;

            Color = color;
            this.renderer.color = color;

            if (this.rendererSize.HasValue) {
                this.renderer.transform.localScale = this.rendererSize.Value;
            } else {
                this.rendererSize = this.renderer.transform.localScale;
            }

            this.pooled = pooled;
        }

        public void Move(Vector2 position) {
            this.rigidbody.isKinematic = true;
            this.rigidbody.MovePosition(position);
            this.springJoint.enabled = false;
        }

        public void Pin(Vector2 position) {
            this.rigidbody.isKinematic = false;
            this.rigidbody.position = position;
            this.springJoint.enabled = true;
            this.springJoint.connectedAnchor = this.rigidbody.position;
            this.springJoint.distance = 0f;
        }

        public void Pop() {
            StartCoroutine(PopRoutine());
        }

        public void Fall() {
            this.rigidbody.isKinematic = false;
            this.springJoint.enabled = false;
        }

        public void Destroy() {
            Destroying?.Invoke(this);
            if (this.pooled != null) {
                this.pooled.Dispose();
            } else {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) {
            PopIfFloor(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            PopIfFloor(collision.gameObject);
        }

        private void PopIfFloor(GameObject obj) {
            if (obj.CheckLayer(this.floorLayers)) {
                Pop();
            }
        }

        private IEnumerator PopRoutine() {
            Vector2 scale = this.renderer.transform.localScale;
            float time = 0f;
            while (time < this.popDuration) {
                this.renderer.transform.localScale =
                    Vector2.Lerp(scale, Vector2.zero, time / this.popDuration);
                yield return null;
                time += Time.deltaTime;
            }
            Destroy();
        }
    }
}
