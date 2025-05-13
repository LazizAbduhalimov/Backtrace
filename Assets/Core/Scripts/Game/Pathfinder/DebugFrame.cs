using System.Collections.Generic;
using LGrid;
using UnityEngine;

public class DebugFrame
{
    public Vector3 UnitCenter;
    public Vector3Int Current;
    public List<Vector3Int> Neighbours = new ();
    public List<Vector2Int> NeighboursAnchor = new();
    public Dictionary<int, List<Cell>> Corners = new ();
}