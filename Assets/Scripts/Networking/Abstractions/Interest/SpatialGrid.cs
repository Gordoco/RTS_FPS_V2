using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTS_FPS_V2.Networking
{
    public sealed class SpatialGrid
    {
        readonly float cellSize;
        readonly Dictionary<Vector2Int, HashSet<ulong>> cells = new Dictionary<Vector2Int, HashSet<ulong>>();

        public SpatialGrid(float cellSize)
        {
            this.cellSize = Mathf.Max(0.01f, cellSize);
        }

        public Vector2Int WorldToCell(Vector3 worldPosition) =>
            new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / cellSize),
                Mathf.FloorToInt(worldPosition.z / cellSize));

        public void Insert(ulong entityId, Vector3 worldPosition)
        {
            var cell = WorldToCell(worldPosition);
            if (!cells.TryGetValue(cell, out var set))
            {
                set = new HashSet<ulong>();
                cells[cell] = set;
            }

            set.Add(entityId);
        }

        public void Remove(ulong entityId, Vector3 worldPosition)
        {
            var cell = WorldToCell(worldPosition);
            if (!cells.TryGetValue(cell, out var set))
            {
                return;
            }

            set.Remove(entityId);
            if (set.Count == 0)
            {
                cells.Remove(cell);
            }
        }

        public void Update(ulong entityId, Vector3 oldPosition, Vector3 newPosition)
        {
            var oldCell = WorldToCell(oldPosition);
            var newCell = WorldToCell(newPosition);
            if (oldCell == newCell)
            {
                return;
            }

            Remove(entityId, oldPosition);
            Insert(entityId, newPosition);
        }

        public bool IsRelevant(Vector3 entityPosition, Vector3 observerPosition, int cellRadius)
        {
            var entityCell = WorldToCell(entityPosition);
            var observerCell = WorldToCell(observerPosition);
            return Mathf.Abs(entityCell.x - observerCell.x) <= cellRadius
                && Mathf.Abs(entityCell.y - observerCell.y) <= cellRadius;
        }

        public bool IsAdjacentCell(Vector3 entityPosition, Vector3 observerPosition) =>
            IsRelevant(entityPosition, observerPosition, 1);
    }
}
