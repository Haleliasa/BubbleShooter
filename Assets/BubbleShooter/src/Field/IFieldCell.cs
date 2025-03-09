using UnityEngine;

namespace BubbleShooter.Field {
    public interface IFieldCell {
        void Hit(IFieldObject obj, Color color, Vector2 position, bool destroy);
    }
}
