using System;
using System.Collections;
using UnityEngine;

namespace Bubbles {
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpringJoint2D))]
    public class Bubble : MonoBehaviour {
        [SerializeField]
        private new SpriteRenderer renderer;

        [SerializeField]
        private LayerMask floorLayers;

        [Tooltip("sec")]
        [Min(0f)]
        [SerializeField]
        private float popDuration = 0.5f;

        private new Rigidbody2D rigidbody;
        private SpringJoint2D springJoint;

        public Color Color { get; private set; }

        public Vector2 Position => this.rigidbody.position;

        public event Action<Bubble> Destroying;

        public void Init(Color color) {
            if (this.rigidbody == null) {
                this.rigidbody = GetComponent<Rigidbody2D>();
            }
            this.rigidbody.isKinematic = true;

            if (this.springJoint == null) {
                this.springJoint = GetComponent<SpringJoint2D>();
            }
            this.springJoint.enabled = false;

            Color = color;
            this.renderer.color = color;
        }

        public void Move(Vector2 position) {
            this.rigidbody.isKinematic = true;
            this.springJoint.enabled = false;
            this.rigidbody.MovePosition(position);
        }

        public void Pin(Vector2 position) {
            this.rigidbody.isKinematic = false;
            this.springJoint.enabled = true;
            this.rigidbody.position = position;
            this.springJoint.connectedAnchor = this.rigidbody.position;
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
            Destroy(gameObject);
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
