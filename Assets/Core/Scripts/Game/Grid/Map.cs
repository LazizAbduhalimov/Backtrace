using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LGrid
{
    public class Map
    {
        public static Map Instance => _instance ??= new Map();
        private static Map _instance;
        
        public Dictionary<Vector3Int, Cell> Cells = new();

        public bool HasFreeCell(out KeyValuePair<Vector3Int, Cell> value)
        {
            value = new KeyValuePair<Vector3Int, Cell>();
            foreach (var pair in Cells.Where(pair => !pair.Value.IsOccupied))
            {
                value = pair;
                return true;
            }
            return false;
        }

        public Cell CreateCell(Vector3Int position)
        {
            if (IsCellExists(position, out var cell)) return cell;

            var newCell = new Cell(position);
            Cells.Add(position, newCell);
            return newCell;
        }
        
        public bool IsCellExists(Vector3Int position, out Cell cell)
        {
            cell = null;
            return Cells.TryGetValue(position, out cell);
        }
        
        public bool IsCellExists(Vector3 position, out Cell cell)
        {
            cell = null;
            return Cells.TryGetValue(Vector3Int.RoundToInt(position), out cell);
        }

        public Cell GetCell(Vector3 position)
        {
            IsCellExists(position, out var cell);
            return cell;
        }
        
        public void RemoveCell(Vector3 position)
        {
            var positionInt = Vector3Int.RoundToInt(position);
            if (Cells.ContainsKey(positionInt))
            {
                Cells.Remove(positionInt);
            }
        }

        public void Clear()
        {
            Cells.Clear();
        }
    }
}