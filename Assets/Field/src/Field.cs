﻿#nullable enable

using BubbleShooter.Levels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BubbleShooter.Field {
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

        private IFieldCellFactory cellFactory = null!;
        private IFieldObjectFactory objectFactory = null!;

        private FieldCell?[,] cells = null!;
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

        public void Init(IFieldCellFactory cellFactory, IFieldObjectFactory objectFactory) {
            this.cellFactory = cellFactory;
            this.objectFactory = objectFactory;
        }

        public void StartNew(IEnumerable<LevelItem> items, IReadOnlyList<Color> colors) {
            this.cells ??= new FieldCell?[this.maxSize.y, this.maxSize.x];

            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    this.DestroyCell(new Vector2Int(x, y), FieldObjectDestroyType.Dispose);
                }
            }

            if (this.destroyCoroutine != null) {
                this.StopCoroutine(this.destroyCoroutine);

                while (this.toDestroy.TryDequeue(
                    out (IFieldObject obj, FieldObjectDestroyType) item
                )) {
                    item.obj.Destroy(FieldObjectDestroyType.Dispose);
                }
            }

            (float interval, float xStart) = this.GetIntervalAndXStart();
            this.topRowCount = 0;

            foreach (LevelItem item in items) {
                Color color = colors[item.colorIndex];
                IFieldObject obj = this.objectFactory.Create(color);
                this.CreateCell(obj, item.coords, color, interval, xStart);
            }

            this.topRowWinCount = Mathf.FloorToInt(this.topRowCount * this.topRowWinFraction);
        }

        public void HitCell(
            FieldCell cell,
            IFieldObject obj,
            Color color,
            Vector2 position,
            bool destroy
        ) {
            (float interval, float xStart) = this.GetIntervalAndXStart();
            Vector2Int? newCoords = null;

            if (!destroy) {
                float minDist = float.MaxValue;

                foreach (Vector2Int coords in this.GetAdjacentCoords(cell.Coords).Where(this.IsFree)) {
                    Vector2 coordsPos = this.GetPosition(coords, interval, xStart);
                    float dist = (position - coordsPos).sqrMagnitude;

                    if (dist < minDist) {
                        minDist = dist;
                        newCoords = coords;
                    }
                }
            }

            if (destroy || !newCoords.HasValue) {
                newCoords = cell.Coords;
                this.DestroyCell(cell.Coords, FieldObjectDestroyType.Match);
            }

            FieldCell newCell = this.CreateCell(obj, newCoords.Value, color, interval, xStart);
            HitData data = this.ProcessMatches(newCell, color);
            Hit?.Invoke(data);
        }

        private void OnDrawGizmos() {
            (float interval, float xStart) = this.GetIntervalAndXStart();

            for (int y = 0; y < this.maxSize.y; y++) {
                for (int x = 0; x < this.maxSize.x; x++) {
                    Gizmos.DrawWireSphere(
                        this.GetPosition(new Vector2Int(x, y), interval, xStart),
                        this.cellRadius
                    );
                }
            }
        }

        private HitData ProcessMatches(FieldCell rootCell, Color color) {
            int topRowMatchCount = 0;
            this.Bfs(
                rootCell,
                cell => {
                    bool match = cell.Color == color;

                    if (!match
                        && cell.Coords.y > 0
                        && !this.matchAdjacent.Contains(cell)
                    ) {
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
                }
            );
            
            if (this.match.Count < this.minMatch) {
                this.match.Clear();
                this.matchAdjacent.Clear();
                
                return new HitData(this, matchCount: 0, isolatedCount: 0, win: false);
            }

            bool win = (this.topRowCount - topRowMatchCount) <= this.topRowWinCount;
            
            if (win) {
                for (int y = 0; y < this.maxSize.y; y++) {
                    for (int x = 0; x < this.maxSize.x; x++) {
                        FieldCell? cell = this.cells[y, x];
                        
                        if (cell != null
                            && !this.match.Contains(cell)
                        ) {
                            this.isolated.Add(cell);
                        }
                    }
                }
            } else {
                while (this.matchAdjacent.Count > 0) {
                    bool includesTopRow = this.Bfs(
                        this.matchAdjacent[^1],
                        cell => !this.match.Contains(cell),
                        cell => {
                            if (cell.Coords.y == 0) {
                                return true;
                            }
                            
                            this.matchAdjacent.Remove(cell);
                            this.isolatedBuffer.Add(cell);
                            
                            return false;
                        }
                    );

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
                this.StopCoroutine(this.destroyCoroutine);
            }

            foreach (FieldCell cell in this.match) {
                this.toDestroy.Enqueue((cell.Object, FieldObjectDestroyType.Match));
                this.DestroyCell(cell.Coords, destroyObject: null);
            }

            this.match.Clear();

            foreach (FieldCell cell in this.isolated) {
                this.toDestroy.Enqueue((cell.Object, FieldObjectDestroyType.Isolated));
                this.DestroyCell(cell.Coords, destroyObject: null);
            }

            this.isolated.Clear();

            this.destroyCoroutine = this.StartCoroutine(this.DestroyObjects());

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
            float xStart
        ) {
            FieldCell cell = this.cellFactory.Create();
            cell.transform.SetParent(this.transform);
            cell.transform.position = this.GetPosition(coords, interval, xStart);
            cell.Init(this, obj, coords, color);
            this.cells[coords.y, coords.x] = cell;

            if (coords.y == 0) {
                this.topRowCount++;
            }

            return cell;
        }

        private void DestroyCell(Vector2Int coords, FieldObjectDestroyType? destroyObject) {
            FieldCell? cell = this.cells[coords.y, coords.x];

            if (cell == null) {
                return;
            }

            cell.DetachObject(destroyObject);
            this.cellFactory.Destroy(cell);
            this.cells[coords.y, coords.x] = null;

            if (coords.y == 0) {
                this.topRowCount--;
            }
        }

        private bool Bfs(
            FieldCell cell,
            Func<FieldCell, bool> predicate,
            Func<FieldCell, bool> visit
        ) {
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

                foreach (Vector2Int c in this.GetAdjacentCoords(cell.Coords)) {
                    FieldCell? adjacentCell = this.cells[c.y, c.x];

                    if (adjacentCell != null
                        && !this.bfsVisited[adjacentCell.Coords.y, adjacentCell.Coords.x]
                        && predicate.Invoke(adjacentCell)
                    ) {
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
                        || adjacent.y >= this.maxSize.y
                    ) {
                        continue;
                    }

                    yield return adjacent;
                }
            }
        }

        private Vector2 GetPosition(Vector2Int coords, float interval, float xStart) {
            float xOffset = 0.25f * ((coords.y & 1) == 0 ? 1 : -1);
            
            return this.transform.TransformPoint(
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
