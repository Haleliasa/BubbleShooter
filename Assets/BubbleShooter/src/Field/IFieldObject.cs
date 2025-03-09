using UnityEngine;

namespace BubbleShooter.Field {
    public interface IFieldObject {
        void Init(Transform position);

        void Detach();

        void Destroy(FieldObjectDestroyType type);
    }
}
