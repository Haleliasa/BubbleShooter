#nullable enable

using System;
using UnityEngine;

namespace BubbleShooter.Core {
    public readonly struct PooledOrInst<T> where T : UnityEngine.Object {
        private readonly IDisposableObject<T>? pooled;
        private readonly T? instantiated;

        public static PooledOrInst<T> Create(IObjectPool<T>? pool, T? prefab) {
            return pool != null ? new PooledOrInst<T>(pool.Get())
                : prefab != null ? new PooledOrInst<T>(UnityEngine.Object.Instantiate(prefab))
                : throw new InvalidOperationException();
        }

        public PooledOrInst(IDisposableObject<T> pooled) {
            this.instantiated = null;
            this.pooled = pooled;
        }

        public PooledOrInst(T instantiated) {
            this.instantiated = instantiated;
            this.pooled = null;
        }

        public T Object =>
            this.pooled != null ? this.pooled.Object
            : this.instantiated != null ? this.instantiated
            : throw new InvalidOperationException();

        public IDisposableObject<T>? Pooled => this.pooled;

        public void Destroy() {
            if (this.pooled != null) {
                this.pooled.Dispose();
                return;
            }

            if (this.instantiated != null) {
                if (this.instantiated.TryGetGameObject(out GameObject? gameObj)) {
                    UnityEngine.Object.Destroy(gameObj);
                }
                return;
            }

            throw new InvalidOperationException();
        }
    }
}
