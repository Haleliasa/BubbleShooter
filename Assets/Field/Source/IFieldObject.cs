using UnityEngine;

namespace Field {
    public interface IFieldObject {
        void Init(Transform position);

        void Destroy(FieldObjectDestroyType type);
    }
}
