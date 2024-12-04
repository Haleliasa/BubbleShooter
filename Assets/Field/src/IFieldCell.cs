using UnityEngine;

namespace Field {
    public interface IFieldCell {
        void Hit(IFieldObject obj, Color color, Vector2 position, bool destroy);
    }
}
