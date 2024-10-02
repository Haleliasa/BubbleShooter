#nullable enable

using UnityEngine;

namespace Shooting {
    public class Projectile : MonoBehaviour {
        [SerializeField]
        private new Rigidbody2D? rigidbody;

        private void FixedUpdate() {
            Vector2 pos =
                this.rigidbody != null
                ? this.rigidbody.position
                : transform.position;

            pos += Vector2.up * (20f * Time.fixedDeltaTime);

            if (this.rigidbody != null) {
                this.rigidbody.MovePosition(pos);
            } else {
                transform.position = pos;
            }
        }
    }
}
