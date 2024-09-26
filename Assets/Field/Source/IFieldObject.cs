using System.Collections;
using UnityEngine;

namespace Field {
    public interface IFieldObject {
        void Init(Transform position);

        IEnumerator Destroy(FieldObjectDestroyType type);
    }
}
