using System;

public interface IDisposableObject<out T> : IDisposable where T : UnityEngine.Object {
    T Object { get; }
}
