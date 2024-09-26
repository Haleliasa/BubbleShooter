#nullable enable

using Field;
using Shooting;
using System.Collections;
using UnityEngine;

namespace Bubbles {
    public class BubbleProjectileFieldObject : MonoBehaviour, IFieldObject {
        [SerializeField]
        private Projectile projectile = null!;

        [SerializeField]
        private new Rigidbody2D? rigidbody;

        void IFieldObject.Init(Transform position) {
            this.projectile.enabled = false;
            transform.SetParent(position, worldPositionStays: true);
        }

        IEnumerator IFieldObject.Destroy(FieldObjectDestroyType type) {
            yield break;
        }
    }
}
