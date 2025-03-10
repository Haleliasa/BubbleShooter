using UnityEngine;

namespace BubbleShooter.Core {
    public interface IConcreteObjectPool<T> : IObjectPool<T> where T : Object {
        T GetConcrete();

        void Return(T obj);
    }
}
