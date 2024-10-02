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
                        DestroyCell(cell, FieldObjectDestroyType.Dispose);
                        this.cells[y, x] = null;
                    }
                }
            }

            (float interval, float xStart) = GetIntervalAndXStart();
            foreach (FieldObjectInfo objInfo in objects
                .Where(o => o.coords.x >= 0 && o.coords.x < this.maxSize.x
                    && o.coords.y >= 0 && o.coords.y < this.maxSize.y)) {
                CreateCell(objInfo, interval, xStart);
            }
        }

        public void Hit(
            FieldCell cell,
            IFieldObject obj,
            Color color,
            Vector2 position,
            bool destroy) {
            (float interval, float xStart) = GetIntervalAndXStart();
            Vector2Int newCoords = cell.Coords;
            if (destroy) {
                DestroyCell(cell, FieldObjectDestroyType.Normal);
            } else {
                float minDist = float.MaxValue;
                foreach (Vector2Int coords in GetFreeAdjacentCoords(cell.Coords)) {
                    Vector2 coordsPos = GetPosition(coords, interval, xStart);
                    float dist = (position - coordsPos).sqrMagnitude;
                    if (dist < minDist) {
                        minDist = dist;
                        newCoords = coords;
                    }
                }
            }
            CreateCell(new FieldObjectInfo(obj, color, newCoords), interval, xStart);
            // TODO: add color matching here
        }

        private void CreateCell(FieldObjectInfo objInfo, float interval, float xStart) {
            FieldCell cell = Instantiate(this.cellPrefab, transform);
            cell.transform.position = GetPosition(objInfo.coords, interval, xStart);
            cell.Init(this, objInfo.obj, objInfo.coords, objInfo.color);
            this.cells[objInfo.coords.y, objInfo.coords.x] = cell;
        }

        private void DestroyCell(FieldCell cell, FieldObjectDestroyType type) {
            cell.Object.Destroy(type);
            Destroy(cell.gameObject);
        }

        private IEnumerable<Vector2Int> GetFreeAdjacentCoords(Vector2Int coords) {
            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    if (dx == 0 && dy == 0) {
                        continue;
                    }
                    if (dy != 0) {
                        if ((coords.y & 1) == 0) {
                            if (dx == -1) {
                                continue;
                            }
                        } else {
                            if (dx == 1) {
                                continue;
                            }
                        }
                    }
                    Vector2Int adjacent = coords + new Vector2Int(dx, dy);
                    if (adjacent.x < 0
                        || adjacent.x >= this.maxSize.x
                        || adjacent.y < 0
                        || adjacent.y >= this.maxSize.y
                        || this.cells[adjacent.y, adjacent.x] != null) {
                        continue;
                    }
                    yield return adjacent;
                }
            }
        }

        private Vector2 GetPosition(Vector2Int coords, float interval, float xStart) {
            float xOffset = 0.25f * ((coords.y & 1) == 0 ? 1 : -1);
            return transform.TransformPoint(
                interval * new Vector2(xStart + coords.x + xOffset, -coords.y));
        }

        private (float, float) GetIntervalAndXStart() {
            float interval = (this.cellRadius * 2f) + this.cellSpacing;
            float xStart = -(this.maxSize.x - 1) / 2f;
            return (interval, xStart);
        }
    }
}
