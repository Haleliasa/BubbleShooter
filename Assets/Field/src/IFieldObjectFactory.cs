using UnityEngine;

namespace Field {
    public interface IFieldObjectFactory {
        IFieldObject CreateFieldObject(Color color);
    }
}
