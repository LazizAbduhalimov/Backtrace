using LGrid;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core.Scripts.Game
{
    public class MapIniterForAStart : MonoBehaviour
    {
        public Tile Tile;
        public LayerMask LayerMask;
        
        public void Start()
        {
            var obstacles = FindObjectsOfType<Obstacle>();
            var tiles = FindObjectsOfType<Tile>();
            foreach (var tile in tiles)
            {
                Map.Instance.CreateCell(Vector3Int.RoundToInt(tile.transform.position));
            }
            
            foreach (var obstacle in obstacles)
            {
                var position = Vector3Int.RoundToInt(obstacle.transform.position);
                if (Map.Instance.IsCellExists(position, out var cellOccupied))
                {
                    cellOccupied.IsOccupied = true;
                }
            }

            foreach (var cell in Map.Instance.Cells)
            {
                Debug.DrawRay(cell.Key, Vector3.up, Color.gray, 5f);
            }
        }

        [ContextMenu("Create map")]
        public void CreateMap100x100() => CreateMap(100, 100);

        private void CreateMap(int x, int z)
        {
            var tilesParent = new GameObject("Tiles").transform;
            for (var i = -x; i <= x; i++)
            {
                for (var j = -z; j <= z; j++)
                {
                    var cellPosition = new Vector3Int(i, 0, j);
                    Debug.DrawRay(cellPosition, Vector3.down, Color.red, 5f);
                    if (!Physics.Raycast(cellPosition, Vector3.down, 0.5f, LayerMask)) continue;
                    Instantiate(Tile, cellPosition, Quaternion.identity, tilesParent);
                }    
            }
        }
    }
}