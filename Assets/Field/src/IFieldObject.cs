using UnityEngine;

namespace Field {
    public interface IFieldObject {
        void Init(Transform position);

        void Detach();

        void Destroy(FieldObjectDestroyType type);
    }
}
