using UnityEngine;

namespace Field {
    public class FieldCell : MonoBehaviour, IFieldCell {
        private Field field;

        public Vector2Int Coords { get; private set; }

        public IFieldObject Object { get; private set; }

        public void Init(Vector2Int coords, IFieldObject obj, Field field) {
            Coords = coords;
            Object = obj;
            Object.Init(transform);
            this.field = field;
        }

        void IFieldCell.Hit(IFieldObject obj, bool destroy) {
            this.field.Hit(this, destroy);
        }
    }
}
