using UnityEngine;

namespace BubbleShooter.Core {
    public interface IObjectPool<out T> where T : Object {
        IDisposableObject<T> Get();
    }
}
