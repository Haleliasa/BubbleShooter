using UnityEngine;

namespace Field {
    public interface IFieldObjectFactory {
        IFieldObject Create(Color color);
    }
}
