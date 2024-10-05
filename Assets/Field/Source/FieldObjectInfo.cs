using UnityEngine;

namespace Field {
    public struct FieldObjectInfo {
        public IFieldObject obj;
        public Vector2Int coords;
        public Color color;

        public FieldObjectInfo(IFieldObject obj, Vector2Int coords, Color color) {
            this.obj = obj;
            this.coords = coords;
            this.color = color;
        }
    }
}
