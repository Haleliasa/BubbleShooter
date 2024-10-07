#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Field {
    public sealed class Field : MonoBehaviour {
        [SerializeField]
        private FieldCell cellPrefab = null!;

        [Tooltip("cells")]
        [SerializeField]
        private Vector2Int maxSize = new(10, 10);

        [Tooltip("units")]
        [Min(0f)]
        [SerializeField]
        private float cellRadius = 0.5f;

        [Tooltip("units")]
        [Min(0f)]
        [SerializeField]
        private float cellSpacing = 0.1f;

        [Min(2)]
        [SerializeField]
        private int minMatch = 3;

        [Range(0f, 1f)]
        [SerializeField]
        private float topRowWinFraction = 0.3f;

        [Tooltip("sec")]
        [Min(0f)]
        [SerializeField]
        private float objectDestroyDelay = 0.1f;

        private FieldCell?[,] cells = null!;
        private int topRowCellCount;
        private int topRowWinCellCount;
        private readonly List<FieldCell> match = new();
        private readonly List<FieldCell> matchAdjacent = new();
        private readonly List<FieldCell> isolatedBuffer = new();
        private readonly List<FieldCell> isolated = new();
        private readonly List<(IFieldObject, FieldObjectDestroyType)> toDestroy = new();
        private readonly Queue<FieldCell> bfsQueue = new();
        private bool[,] bfsVisited = null!;

        public void Init(IEnumerable<FieldObjectInfo> objects) {
            this.cells ??= new FieldCell[this.maxSize.y, this.maxSize.x];

            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    DestroyCell(new Vector2Int(x, y), FieldObjectDestroyType.Dispose);
                }
            }

            (float interval, float xStart) = GetIntervalAndXStart();
            this.topRowCellCount = 0;
            foreach (FieldObjectInfo objInfo in objects) {
                CreateCell(objInfo.obj, objInfo.coords, objInfo.color, interval, xStart);
            }
            this.topRowWinCellCount = Mathf.FloorToInt(
                this.topRowCellCount * this.topRowWinFraction);
        }

        public void Hit(
            FieldCell cell,
            IFieldObject obj,
            Color color,
            Vector2 position,
            bool destroy) {
            (float interval, float xStart) = GetIntervalAndXStart();
            Vector2Int? newCoords = null;

            if (!destroy) {
                float minDist = float.MaxValue;
                foreach (Vector2Int coords in GetAdjacentCoords(cell.Coords).Where(IsFree)) {
                    Vector2 coordsPos = GetPosition(coords, interval, xStart);
                    float dist = (position - coordsPos).sqrMagnitude;
                    if (dist < minDist) {
                        minDist = dist;
                        newCoords = coords;
                    }
                }
            }

            if (destroy || !newCoords.HasValue) {
                newCoords = cell.Coords;
                DestroyCell(cell, FieldObjectDestroyType.Normal);
            }

            FieldCell newCell = CreateCell(obj, newCoords.Value, color, interval, xStart);
            ProcessMatches(newCell, color);
        }

        private void OnDrawGizmos() {
            (float interval, float xStart) = GetIntervalAndXStart();
            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    Gizmos.DrawSphere(
                        GetPosition(new Vector2Int(x, y), interval, xStart),
                        this.cellRadius);
                }
            }
        }

        private void ProcessMatches(FieldCell rootCell, Color color) {
            int topRowMatchCellCount = 0;
            Bfs(
                rootCell,
                cell => {
                    bool match = cell.Color == color;
                    if (!match
                        && cell.Coords.y > 0
                        && !this.matchAdjacent.Contains(cell)) {
                        this.matchAdjacent.Add(cell);
                    }
                    return match;
                },
                cell => {
                    this.match.Add(cell);
                    if (cell.Coords.y == 0) {
                        topRowMatchCellCount++;
                    }
                    return false;
                });
            
            if (this.match.Count < this.minMatch) {
                this.match.Clear();
                this.matchAdjacent.Clear();
                return;
            }

            this.toDestroy.AddRange(this.match.Select(cell =>
                (cell.Object, FieldObjectDestroyType.Normal)));

            bool win = (this.topRowCellCount - topRowMatchCellCount)
                <= this.topRowWinCellCount;
            if (win) {
                for (int y = 0; y < this.maxSize.y; y++) {
                    for (int x = 0; x < this.maxSize.x; x++) {
                        FieldCell? cell = this.cells[y, x];
                        if (cell != null
                            && !this.match.Contains(cell)) {
                            this.isolated.Add(cell);
                        }
                    }
                }
            } else {
                while (this.matchAdjacent.Count > 0) {
                    bool includesTopRow = Bfs(
                        this.matchAdjacent[^1],
                        cell => !this.match.Contains(cell),
                        cell => {
                            if (cell.Coords.y == 0) {
                                return true;
                            }
                            this.matchAdjacent.Remove(cell);
                            this.isolatedBuffer.Add(cell);
                            return false;
                        });
                    // found isolated cells
                    if (!includesTopRow) {
                        this.isolated.AddRange(this.isolatedBuffer);
                    }
                    this.isolatedBuffer.Clear();
                }
            }
            this.matchAdjacent.Clear();

            this.toDestroy.AddRange(this.isolated.Select(cell =>
                (cell.Object, FieldObjectDestroyType.Isolated)));

            foreach (FieldCell cell in this.match.Concat(this.isolated)) {
                DestroyCell(cell, destroyObject: null);
            }
            this.match.Clear();
            this.isolated.Clear();
            StartCoroutine(DestroyObjects());
        }

        private IEnumerator DestroyObjects() {
            foreach ((IFieldObject obj, FieldObjectDestroyType type) in this.toDestroy) {
                obj.Destroy(type);
                yield return new WaitForSeconds(this.objectDestroyDelay);
            }
            this.toDestroy.Clear();
        }

        private FieldCell CreateCell(
            IFieldObject obj,
            Vector2Int coords,
            Color color,
            float interval,
            float xStart) {
            FieldCell cell = Instantiate(this.cellPrefab);
            cell.transform.SetParent(transform);
            cell.transform.position = GetPosition(coords, interval, xStart);
            cell.Init(this, obj, coords, color);
            this.cells[coords.y, coords.x] = cell;
            if (coords.y == 0) {
                this.topRowCellCount++;
            }
            return cell;
        }

        private void DestroyCell(Vector2Int coords, FieldObjectDestroyType? destroyObject) {
            FieldCell? cell = this.cells[coords.y, coords.x];
            if (cell != null) {
                DestroyCell(cell, destroyObject);
            }
        }

        private void DestroyCell(FieldCell cell, FieldObjectDestroyType? destroyObject) {
            cell.DetachObject(destroyObject);
            Destroy(cell.gameObject);
            this.cells[cell.Coords.y, cell.Coords.x] = null;
            if (cell.Coords.y == 0) {
                this.topRowCellCount--;
            }
        }

        private bool Bfs(
            FieldCell cell,
            Func<FieldCell, bool> predicate,
            Func<FieldCell, bool> visit) {
            this.bfsVisited ??= new bool[this.maxSize.y, this.maxSize.x];
            do {
                if (this.bfsVisited[cell.Coords.y, cell.Coords.x]) {
                    continue;
                }

                if (visit.Invoke(cell)) {
                    this.bfsQueue.Clear();
                    Array.Clear(this.bfsVisited, 0, this.bfsVisited.Length);
                    return true;
                }
                this.bfsVisited[cell.Coords.y, cell.Coords.x] = true;

                foreach (Vector2Int c in GetAdjacentCoords(cell.Coords)) {
                    FieldCell? adjacentCell = this.cells[c.y, c.x];
                    if (adjacentCell != null
                        && !this.bfsVisited[adjacentCell.Coords.y, adjacentCell.Coords.x]
                        && predicate.Invoke(adjacentCell)) {
                        this.bfsQueue.Enqueue(adjacentCell);
                    }
                }
            }
            while (this.bfsQueue.TryDequeue(out cell));
            Array.Clear(this.bfsVisited, 0, this.bfsVisited.Length);
            return false;
        }

        private IEnumerable<Vector2Int> GetAdjacentCoords(Vector2Int coords) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dx = -1; dx <= 1; dx++) {
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
                        || adjacent.y >= this.maxSize.y) {
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

        private bool IsFree(Vector2Int coords) {
            return this.cells[coords.y, coords.x] == null;
        }

        private (float, float) GetIntervalAndXStart() {
            float interval = (this.cellRadius * 2f) + this.cellSpacing;
            float xStart = -(this.maxSize.x - 1) / 2f;
            return (interval, xStart);
        }
    }
}
