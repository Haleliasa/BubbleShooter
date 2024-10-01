#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Field {
    public sealed class Field : MonoBehaviour {
        [SerializeField]
        private FieldCell cellPrefab = null!;

        [Tooltip("cells")]
        [SerializeField]
        private Vector2Int maxSize = new(10, 20);

        [Tooltip("units")]
        [Min(0f)]
        [SerializeField]
        private float cellRadius = 0.5f;

        [Tooltip("units")]
        [Min(0f)]
        [SerializeField]
        private float cellSpacing = 0.1f;

        private FieldCell?[,] cells = null!;

        public void Init(IEnumerable<FieldObjectInfo> objects) {
            this.cells ??= new FieldCell[this.maxSize.y, this.maxSize.x];

            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    FieldCell? cell = this.cells[y, x];
                    if (cell != null) {
                        cell.Object.Destroy(FieldObjectDestroyType.Dispose);
                        Destroy(cell.gameObject);
                        this.cells[y, x] = null;
                    }
                }
            }

            float interval = (this.cellRadius * 2f) + this.cellSpacing;
            float xStart = -(this.maxSize.x - 1) / 2f;
            foreach (FieldObjectInfo objInfo in objects
                .Where(o => o.coords.x >= 0 && o.coords.x < this.maxSize.x
                    && o.coords.y >= 0 && o.coords.y < this.maxSize.y)) {
                FieldCell cell = Instantiate(this.cellPrefab, transform);
                cell.transform.localPosition =
                    new Vector2(xStart + objInfo.coords.x, -objInfo.coords.y) * interval;
                cell.Init(this, objInfo.obj, objInfo.coords, objInfo.color);
                this.cells[objInfo.coords.y, objInfo.coords.x] = cell;
            }
        }

        public void Hit(
            FieldCell cell,
            IFieldObject obj,
            Color color,
            Vector2 position,
            bool destroy) {

        }
    }
}
