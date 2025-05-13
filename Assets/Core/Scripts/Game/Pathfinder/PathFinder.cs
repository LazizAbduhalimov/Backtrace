using System.Collections.Generic;
using System.Linq;
using LGrid;
using UnityEngine;

public class PathFinder
{
    public Vector2Int StartAnchor;
    public Vector2Int LastPreviewedPathFinalAnchor = new (1, 1);
    public static readonly List<DebugFrame> Frames = new ();
    
    public Vector2Int UnitSize;
    public Dictionary<Vector3Int, Cell> Grid;
    
    private static readonly Vector3Int[] Directions = {
        Vector3Int.right, 
        Vector3Int.left, 
        Vector3Int.forward, 
        Vector3Int.back
    };
    
    private static readonly List<Vector3Int> Corners = new()
    {
        new Vector3Int(-1, 0,  1),
        new Vector3Int( 1, 0,  1),
        new Vector3Int(-1, 0, -1),
        new Vector3Int( 1, 0, -1)
    };
    
    private static readonly Vector2Int[] All2By2Anchors = {
        new(1, 1),
        new(2, 1),
        new(1, 2),
        new(2, 2),
    };
    
    private DebugFrame _debugFrame;
    
    public PathFinder(Dictionary<Vector3Int, Cell> grid)
    {
        Grid = grid;
        UnitSize = new Vector2Int(1, 1);
        StartAnchor = new Vector2Int(1, 1);
    }

    public PathFinder(Dictionary<Vector3Int, Cell> grid, Vector2Int unitSize, Vector2Int anchor)
    {
        Grid = grid;
        UnitSize = unitSize;
        StartAnchor = anchor;
    }
    
    public Cell[] GetOccupiedCells(Vector3Int position) => GetOccupiedCells(position, StartAnchor);
    
    public Cell[] GetIntendedCellsToOccupy(Vector3Int position) => GetOccupiedCells(position, LastPreviewedPathFinalAnchor);
    
    public void ResetAnchorToFoundPathLastAnchor()
    {
        StartAnchor = LastPreviewedPathFinalAnchor;
    }

    public bool CanBePlacedAt(Vector3Int start, Vector3Int goal)
    {
        if (start == goal) return false;
        if (IsSizeXByX(1)) return true;
        
        var occupiedCells = GetOccupiedCells(start);
        if (IsSizeXByX(3) && CanPlaceUnit(goal, StartAnchor, occupiedCells))
            return true;
        
        if (occupiedCells.Any(c => c.Position == goal))
            return false;
        
        return All2By2Anchors.Any(a => CanPlaceUnit(goal, a, occupiedCells));
    }
    
    public (List<Cell>, List<Vector3>) FindPath(Vector3Int start, Vector3Int goal)
    {
        var occupiedCells = GetOccupiedCells(start);
        if (!CanBePlacedAt(start, goal))
            return (new List<Cell>(), new List<Vector3>());

        // такой тип нужен для оптимизации чтобы не сортировывать каждый раз
        var openSet = new SortedSet<(float, Vector3Int, Vector2Int)>(Comparer<(float, Vector3Int, Vector2Int)>.Create((a, b) =>
            a.Item1.CompareTo(b.Item1) != 0 ? a.Item1.CompareTo(b.Item1) : a.Item2.GetHashCode().CompareTo(b.Item2.GetHashCode()))) { (Heuristic(start, goal), start, StartAnchor) };

        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var cameFromCenter = new Dictionary<Vector3, Vector3>();
        var gScore = new Dictionary<Vector3Int, float> { [start] = 0 };
        var fScore = new Dictionary<Vector3Int, float> { [start] = Heuristic(start, goal) };
        Frames.Clear();
        while (openSet.Count > 0)
        {
            var debugFrame = new DebugFrame();
            _debugFrame = debugFrame;
            var (f, current, currentAnchor) = openSet.Min;
            var unitCenter = GetUnitCenter(current, currentAnchor);
            openSet.Remove(openSet.Min);
            
            debugFrame.Current = current;
            debugFrame.UnitCenter = unitCenter;

            if (current == goal)
            {
                LastPreviewedPathFinalAnchor = currentAnchor;
                return ReconstructPath(cameFrom, cameFromCenter, current, unitCenter);
            }

            foreach (var neighbor in GetNeighbors(current, unitCenter))
            {
                var isOccupiedByThisUnit = occupiedCells.Any(c => c.Position == neighbor);
                
                var neighborAnchor = GetAnchor(unitCenter, neighbor);
                if (IsSizeXByX(2))
                {
                    if (!isOccupiedByThisUnit)
                    {
                        debugFrame.Neighbours.Add(neighbor);
                        debugFrame.NeighboursAnchor.Add(neighborAnchor);
                        if (!CanPlaceUnit(neighbor, neighborAnchor, occupiedCells))
                            continue;   
                    }
                }
                else
                {
                    if (!CanPlaceUnit(neighbor, neighborAnchor, occupiedCells))
                        continue;
                }

                var tentativeGScore = gScore[current] + (isOccupiedByThisUnit ? 0: 1 );

                if (!gScore.TryGetValue(neighbor, out var neighborGScore) || tentativeGScore < neighborGScore)
                {
                    cameFromCenter[neighbor] = unitCenter;
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal);
                    openSet.Add((fScore[neighbor], neighbor, neighborAnchor));
                }
            }
            Frames.Add(debugFrame);
        }
        
        return (new List<Cell>(), new List<Vector3>());
    }

    private (List<Cell>, List<Vector3>) ReconstructPath(
        Dictionary<Vector3Int, Vector3Int> cameFrom, 
        Dictionary<Vector3, Vector3> cameFromCenter, 
        Vector3Int current, 
        Vector3 unitCenter)
    {
        var pathCenter = new List<Vector3> { unitCenter };
        var path = new List<Cell> { Grid[current] };
        
        while (cameFrom.TryGetValue(current, out var prev))
        {
            pathCenter.Add(cameFromCenter[current]);
            current = prev;
            path.Add(Grid[current]);
        }
        path.Reverse();
        pathCenter.Reverse();
        return (path, pathCenter);
    }

    private Vector2Int GetAnchor(Vector3 origin, Vector3 point)
    {
        if (!IsSizeXByX(2))
            return StartAnchor;
        
        var quadrant = GetQuadrant(origin, point);
        return quadrant switch
        {
            1 => new Vector2Int(2, 2),
            2 => new Vector2Int(1, 2),
            3 => new Vector2Int(1, 1),
            4 => new Vector2Int(2, 1),
            _ => new Vector2Int()
        };
    }

    private List<Vector3Int> GetNeighbors(Vector3Int cell, Vector3 unitCenter)
    {
        var neighbors = new List<Vector3Int>(8);
        neighbors.AddRange(Directions
                    .Select(dir => cell + dir)
                    .Where(neighbor => Grid.ContainsKey(neighbor)));

        // чекаем каждую диагональ на проходимость
        foreach (var diagonalCorner in Corners)
            TryAddDiagonalNeighbor(neighbors, cell, unitCenter, diagonalCorner);

        return neighbors;
    }
    
    private void TryAddDiagonalNeighbor(
        ICollection<Vector3Int> neighbors, Vector3Int cell, Vector3 unitCenter, Vector3Int direction)
    {
        var diagonal = cell + direction;
        var direction1 = Vector3Int.RoundToInt(cell.ToVector3().AddX(direction.x));
        var direction2 = Vector3Int.RoundToInt(cell.ToVector3().AddZ(direction.z));

        var anchorDiagonal = GetAnchor(cell, diagonal);
        var anchor1 = GetAnchor(cell, direction1);
        var anchor2 = GetAnchor(cell, direction2);
        
        // если двигаемся диагонально (дальше от центра) проверяем нет ли премятствий у смежных углов
        // у нас 2 на 2 жирный объект и проверяется не только соседи якоря но и соседи соседей нашего якоря ;)
        // сложно и муторно крч, но так меньше лагов
        if ((diagonal - unitCenter).sqrMagnitude > 4)
        {
            var adjacentCornersCells = GetAdjacentCornersCells(cell, diagonal);
            if (adjacentCornersCells.Any(c => c.IsOccupied))
                return;
        }
        
        if (CanPlaceUnit(diagonal, anchorDiagonal) && CanPlaceUnit(direction1, anchor1) && CanPlaceUnit(direction2, anchor2))
            neighbors.Add(diagonal);
    }
    
    private IEnumerable<Vector3Int> GetAdjacentCorners(Vector3Int corner)
    {
        // Возвращаем только те углы, что НЕ равны текущему и НЕ противоположны ему
        return Corners.Where(c => c != corner && (c.x == corner.x || c.z == corner.z));
    }

    private Cell[] GetAdjacentCornersCells(Vector3Int origin, Vector3Int destination)
    {
        var corner = destination - origin;
        var adjacentCorners = GetAdjacentCorners(corner);
        var cells = new List<Cell>();
        foreach (var adjacentCorner in adjacentCorners)
        {
            if (Map.Instance.IsCellExists(origin + adjacentCorner, out var cell))
                cells.Add(cell);
        }
        return cells.ToArray();
    }

    private bool CanPlaceUnit(Vector3Int position, Vector2Int anchor, Cell[] excludedCells = null)
    {
        for (var x = 0; x < UnitSize.x; x++)
        {
            for (var z = 0; z < UnitSize.y; z++)
            {
                var checkPos = new Vector3Int(position.x + x - (anchor.x - 1), position.y, position.z + z - (anchor.y - 1));
                if (!Grid.TryGetValue(checkPos, out var cell))
                    return false;

                if (cell.IsOccupied && (excludedCells == null || !excludedCells.Contains(cell)))
                    return false;
            }
        }
        return true;
    }

    private Cell[] GetOccupiedCells(Vector3Int position, Vector2Int anchor)
    {
        var placedCells = new Cell[UnitSize.x * UnitSize.y];
        var i = 0;
        for (var x = 0; x < UnitSize.x; x++)
        {
            for (var z = 0; z < UnitSize.y; z++)
            {
                var checkPos = new Vector3Int(position.x + x - (anchor.x - 1), position.y, position.z + z - (anchor.y - 1));
                if (Map.Instance.IsCellExists(checkPos, out var cell))
                    placedCells[i++] = cell;
            }
        }
        return placedCells;
    }

    public Vector3 GetUnitCenter(Vector3Int cellPosition, Vector2Int anchor)
    {
        var centerX = cellPosition.x - (anchor.x - 1) + (UnitSize.x - 1) / 2f;
        var centerZ = cellPosition.z - (anchor.y - 1) + (UnitSize.y - 1) / 2f;
        return new Vector3(centerX, cellPosition.y, centerZ);
    }
    
    private static int GetQuadrant(Vector3 origin, Vector3 point)
    {
        var dx = point.x - origin.x;
        var dz = point.z - origin.z;
        if (dx >= 0 && dz >= 0) return 1;
        if (dx < 0 && dz >= 0) return 2;
        if (dx < 0 && dz < 0) return 3;
        return 4;
    }
    
    private bool IsSizeXByX(int x) => UnitSize.x == x && UnitSize.y == x;
    
    private float Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z);
    }
}
