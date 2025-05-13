using System;
using System.Collections.Generic;
using System.Linq;
using LGrid;
using UnityEngine;

namespace Core.Scripts.Game
{
    public class PathDrawer : MonoBehaviour
    {
        public Transform Unit;
        private PathFinder _pathFinder;
        private Vector3Int _startingCell;

        private readonly Dictionary<Vector3Int, Tile> _tiles = new();
        private List<Cell> _path = new ();
        private List<Tile> _unSettableTiles = new();

        private bool isEnabled;

        private void Start()
        {
            var anchor = new Vector2Int(1, 1);
            var unitSize = new Vector2Int(1, 1);
            var tiles = FindObjectsOfType<Tile>();
            foreach (var tile in tiles)
            {
                _tiles.Add(Vector3Int.RoundToInt(tile.transform.position), tile);
            }
            var cellPosition = Vector3Int.RoundToInt(Unit.position);
            // Map.Instance.GetCell(cellPosition);
            // cell.IsOccupied = true;
            _pathFinder = new PathFinder(Map.Instance.Cells, unitSize, anchor);
            Unit.position = _pathFinder.GetUnitCenter(cellPosition, anchor);
            _startingCell = Vector3Int.RoundToInt(Unit.position);
            // Unit.localScale = new Vector3(unitSize.x, 1, unitSize.y);
        }

        private int _frameIndex = 0;
        private int _neighborIndex = 0;

        private void DebugPath()
        {
            if (PathFinder.Frames.Count < 1) return;
            var frame = PathFinder.Frames[_frameIndex];
            Debug.DrawRay(frame.Current, Vector3Int.up, Color.green);
            Debug.DrawRay(frame.UnitCenter, Vector3.up, Color.blue);
            if (frame.Neighbours.Count > _neighborIndex)
            {
                var neigh = frame.Neighbours[_neighborIndex];
                var anchor = frame.NeighboursAnchor[_neighborIndex];
                Debug.DrawRay(neigh.ToVector3().AddX(0.05f), Vector3.up, Color.yellow);
                if (frame.Corners.Count > 0)
                {
                    if (frame.Corners.TryGetValue(_neighborIndex, out var val))
                    {
                        foreach (var entity in val)
                        {
                            Debug.DrawRay(entity.Position, Vector3.up, Color.red);
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                _frameIndex++;
                NormalizedFrameIndex();
                _neighborIndex = 0;
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                _frameIndex--;
                NormalizedFrameIndex();
                _neighborIndex = 0;
            }
            
            if (Input.GetKeyDown(KeyCode.Keypad3))
                _neighborIndex = Mathf.Clamp(_neighborIndex +1, 0, frame.Neighbours.Count-1);
            if (Input.GetKeyDown(KeyCode.Keypad1))
                _neighborIndex = Mathf.Clamp(_neighborIndex -1, 0, frame.Neighbours.Count-1);
        }

        private void NormalizedFrameIndex()
        {
            if ( PathFinder.Frames.Count < 1 ) return;
            while (_frameIndex < 0)
                _frameIndex += PathFinder.Frames.Count;
            _frameIndex %= PathFinder.Frames.Count;
        }
        
        public void Update()
        {
            // DebugPath();
            if (Input.GetMouseButtonDown(1))
            {
                Path();
            }
            if (Input.GetKeyDown(KeyCode.Tab))
                isEnabled = !isEnabled;
            if (!isEnabled) return;
            Path();
        }

        private void Path()
        {
            var mousePosition = Vector3Int.RoundToInt(MapUtils.GetMouseWorldPosition());
            PreviewPath(_startingCell, mousePosition);
            DoPath(_startingCell, mousePosition);
        }
        
        private void DoPath(Vector3Int start, Vector3Int finish)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!_pathFinder.CanBePlacedAt(start , finish)) 
                    return;
                
                var occupiedCells = _pathFinder.GetOccupiedCells(start);
                var intendedCellsToOccupy = _pathFinder.GetIntendedCellsToOccupy(finish);
                
                _pathFinder.ResetAnchorToFoundPathLastAnchor();
                _startingCell = finish;
                Unit.transform.position = _pathFinder.GetUnitCenter(finish, _pathFinder.LastPreviewedPathFinalAnchor);
                foreach (var cell in intendedCellsToOccupy)
                {
                    cell.IsOccupied = true;
                }

                foreach (var cell in occupiedCells)
                {
                    cell.IsOccupied = false;
                }
            }
        }

        private void PreviewPath(Vector3Int start, Vector3Int finish)
        {
            var pathPair = _pathFinder.FindPath(start, finish);
            Debug.Log($"s {_pathFinder.StartAnchor}");
            Debug.Log($"l {_pathFinder.LastPreviewedPathFinalAnchor}");
            var pathCells = pathPair.Item1;
            var pathVertexes = pathPair.Item2;
            var placingCells = _pathFinder.GetIntendedCellsToOccupy(finish);
            foreach (var vertex in pathVertexes)
            {
                Debug.DrawRay(vertex, Vector3.up, Color.yellow);
            }
            foreach (var placingCell in placingCells)
            {
                var color = placingCell.IsOccupied ? Color.magenta : Color.cyan;
                Debug.DrawRay(placingCell.Position, Vector3.up, color);
            }
            if (pathCells.Count == 0)
            {
                if (_tiles.TryGetValue(finish, out var tile))
                {
                    _unSettableTiles.Add(tile);
                    tile.MeshRenderer.material.color = Color.red;
                }
            }
            else
            {
                foreach (var tile in _unSettableTiles)
                {
                    tile.MeshRenderer.material.color = Color.white;
                }
                _unSettableTiles.Clear();
                DrawPath(pathCells);
            }
        }

        private void DrawPath(List<Cell> cells)
        {
            foreach (var cell in _path)
            {
                _tiles[cell.Position].MeshRenderer.material.color = Color.white;
            }
            
            foreach (var cell in cells)
            {
                _tiles[cell.Position].MeshRenderer.material.color = Color.green;
            }

            _path = cells;
        }
    }
}