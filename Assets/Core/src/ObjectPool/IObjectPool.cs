using UnityEngine;

public interface IObjectPool<out T> where T : Object {
    IDisposableObject<T> Get();
}
