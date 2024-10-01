using UnityEngine;

namespace Field {
    public struct FieldObjectInfo {
        public IFieldObject obj;
        public Color color;
        public Vector2Int coords;

        public FieldObjectInfo(IFieldObject obj, Color color, Vector2Int coords) {
            this.obj = obj;
            this.color = color;
            this.coords = coords;
        }
    }
}
