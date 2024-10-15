#nullable enable

using Levels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Field {
    public sealed class Field : MonoBehaviour {
        [SerializeField]
        private FieldCell? cellPrefab;

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

        private PooledOrInst<FieldCell>?[,] cells = null!;
        private IObjectPool<FieldCell>? cellPool;
        private int topRowCount;
        private int topRowWinCount;
        private readonly List<FieldCell> match = new();
        private readonly List<FieldCell> matchAdjacent = new();
        private readonly List<FieldCell> isolatedBuffer = new();
        private readonly List<FieldCell> isolated = new();
        private readonly Queue<(IFieldObject, FieldObjectDestroyType)> toDestroy = new();
        private Coroutine? destroyCoroutine;
        private readonly Queue<FieldCell> bfsQueue = new();
        private bool[,] bfsVisited = null!;

        public event Action<HitData>? Hit;

        public void Init(
            IEnumerable<LevelItem> items,
            IReadOnlyList<Color> colors,
            IFieldObjectFactory objectFactory,
            IObjectPool<FieldCell>? cellPool = null) {
            this.cells ??= new PooledOrInst<FieldCell>?[this.maxSize.y, this.maxSize.x];
            this.cellPool = cellPool;

            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    DestroyCell(new Vector2Int(x, y), FieldObjectDestroyType.Dispose);
                }
            }

            if (this.destroyCoroutine != null) {
                StopCoroutine(this.destroyCoroutine);
                while (this.toDestroy.TryDequeue(
                    out (IFieldObject obj, FieldObjectDestroyType) item)) {
                    item.obj.Destroy(FieldObjectDestroyType.Dispose);
                }
            }

            (float interval, float xStart) = GetIntervalAndXStart();
            this.topRowCount = 0;
            foreach (LevelItem item in items) {
                Color color = colors[item.colorIndex];
                IFieldObject obj = objectFactory.CreateFieldObject(color);
                CreateCell(obj, item.coords, color, interval, xStart);
            }
            this.topRowWinCount = Mathf.FloorToInt(
                this.topRowCount * this.topRowWinFraction);
        }

        public void HitCell(
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
                DestroyCell(cell.Coords, FieldObjectDestroyType.Match);
            }

            FieldCell newCell = CreateCell(obj, newCoords.Value, color, interval, xStart);
            HitData data = ProcessMatches(newCell, color);
            Hit?.Invoke(data);
        }

        private void OnDrawGizmos() {
            (float interval, float xStart) = GetIntervalAndXStart();
            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    Gizmos.DrawWireSphere(
                        GetPosition(new Vector2Int(x, y), interval, xStart),
                        this.cellRadius);
                }
            }
        }

        private HitData ProcessMatches(FieldCell rootCell, Color color) {
            int topRowMatchCount = 0;
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
                        topRowMatchCount++;
                    }
                    return false;
                });
            
            if (this.match.Count < this.minMatch) {
                this.match.Clear();
                this.matchAdjacent.Clear();
                return new HitData(this, matchCount: 0, isolatedCount: 0, win: false);
            }

            bool win = (this.topRowCount - topRowMatchCount)
                <= this.topRowWinCount;
            if (win) {
                for (int y = 0; y < this.maxSize.y; y++) {
                    for (int x = 0; x < this.maxSize.x; x++) {
                        FieldCell? cell = this.cells[y, x]?.Object;
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

            HitData hitData = new(this, this.match.Count, this.isolated.Count, win);

            if (this.destroyCoroutine != null) {
                StopCoroutine(this.destroyCoroutine);
            }

            foreach (FieldCell cell in this.match) {
                this.toDestroy.Enqueue((cell.Object, FieldObjectDestroyType.Match));
                DestroyCell(cell.Coords, destroyObject: null);
            }
            this.match.Clear();

            foreach (FieldCell cell in this.isolated) {
                this.toDestroy.Enqueue((cell.Object, FieldObjectDestroyType.Isolated));
                DestroyCell(cell.Coords, destroyObject: null);
            }
            this.isolated.Clear();

            this.destroyCoroutine = StartCoroutine(DestroyObjects());

            return hitData;
        }

        private IEnumerator DestroyObjects() {
            yield return new WaitForSeconds(this.objectDestroyDelay);
            while (this.toDestroy.TryDequeue(
                out (IFieldObject obj, FieldObjectDestroyType type) item)) {
                item.obj.Destroy(item.type);
                yield return new WaitForSeconds(this.objectDestroyDelay);
            }
            this.destroyCoroutine = null;
        }

        private FieldCell CreateCell(
            IFieldObject obj,
            Vector2Int coords,
            Color color,
            float interval,
            float xStart) {
            PooledOrInst<FieldCell> createdCell =
                PooledOrInst<FieldCell>.Create(this.cellPool, this.cellPrefab);
            FieldCell cell = createdCell.Object;
            cell.transform.SetParent(transform);
            cell.transform.position = GetPosition(coords, interval, xStart);
            cell.Init(this, obj, coords, color);
            this.cells[coords.y, coords.x] = createdCell;
            if (coords.y == 0) {
                this.topRowCount++;
            }
            return cell;
        }

        private void DestroyCell(Vector2Int coords, FieldObjectDestroyType? destroyObject) {
            PooledOrInst<FieldCell>? cell = this.cells[coords.y, coords.x];
            if (!cell.HasValue) {
                return;
            }
            cell.Value.Object.DetachObject(destroyObject);
            cell.Value.Destroy();
            this.cells[coords.y, coords.x] = null;
            if (coords.y == 0) {
                this.topRowCount--;
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
                    FieldCell? adjacentCell = this.cells[c.y, c.x]?.Object;
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

        public readonly struct HitData {
            public HitData(Field field, int matchCount, int isolatedCount, bool win) {
                this.field = field;
                this.matchCount = matchCount;
                this.isolatedCount = isolatedCount;
                this.win = win;
            }

            public readonly Field field;
            public readonly int matchCount;
            public readonly int isolatedCount;
            public readonly bool win;
        }
    }
}
