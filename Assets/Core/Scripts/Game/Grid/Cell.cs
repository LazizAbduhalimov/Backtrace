using UnityEngine;

namespace LGrid
{
    public class Cell
    {
        public Vector3Int Position;
        public bool IsOccupied;

        public Cell(Vector3Int position)
        {
            Position = position;
        }
    }
}