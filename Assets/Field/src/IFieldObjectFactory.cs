using UnityEngine;

namespace BubbleShooter.Field {
    public interface IFieldObjectFactory {
        IFieldObject Create(Color color);
    }
}
