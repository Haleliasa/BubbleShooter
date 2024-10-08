using UnityEngine;

namespace Field {
    public sealed class FieldCell : MonoBehaviour, IFieldCell {
        private Field field;

        public IFieldObject Object { get; private set; }

        public Vector2Int Coords { get; private set; }

        public Color Color { get; private set; }

        public void Init(Field field, IFieldObject obj, Vector2Int coords, Color color) {
            this.field = field;
            Object = obj;
            Object.Init(transform);
            Coords = coords;
            Color = color;
        }

        public void DetachObject(FieldObjectDestroyType? destroy) {
            Object.Detach();
            if (destroy.HasValue) {
                Object.Destroy(destroy.Value);
            }
        }

        void IFieldCell.Hit(IFieldObject obj, Color color, Vector2 position, bool destroy) {
            this.field.HitCell(this, obj, color, position, destroy);
        }
    }
}
