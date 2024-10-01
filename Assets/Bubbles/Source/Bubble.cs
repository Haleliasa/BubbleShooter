using System.Collections;
using UnityEngine;

namespace Bubbles {
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpringJoint2D))]
    public class Bubble : MonoBehaviour {
        private new Rigidbody2D rigidbody;
        private SpringJoint2D springJoint;

        public IEnumerator Pop() {
            yield break;
        }

        public IEnumerator Fall() {
            yield break;
        }

        private void Awake() {
            this.rigidbody = GetComponent<Rigidbody2D>();
            this.springJoint = GetComponent<SpringJoint2D>();
        }
    }
}
