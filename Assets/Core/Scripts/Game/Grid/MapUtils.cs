using System;
using Client;
using Leopotam.EcsLite;
using UnityEngine;

namespace LGrid
{
    public static class MapUtils
    {
        public static bool TryGetCellOccupier<T, CActive>(Vector3Int cell, EcsWorld world, out T standable) 
            where T: struct, ICellStandable
            where CActive : struct
        {
            foreach (var entity in world.Filter<T>().Inc<CActive>().End())
            {
                ref var data = ref world.GetPool<T>().Get(entity);
                if (data.Coords == cell)
                {
                    standable = data;
                    return true;
                }
            }

            standable = default;
            return false;
        }
        
        public static Vector3 GetMouseWorldPosition()
        {
            var mouseScreenPosition = Input.mousePosition;
            var plane = new Plane(Vector3.up, Vector3.zero);
            var ray = Camera.main.ScreenPointToRay(mouseScreenPosition);
            return plane.Raycast(ray, out var distance) ? ray.GetPoint(distance) : Vector3.zero;
        }
        
        public static Vector3 GetLimitedMousePosition()
        {
            var mousePosition = GetMouseWorldPosition();
            return mousePosition;
        }

        public static Vector3 GetSnappedPosition(Vector3 position)
        {
            foreach (var entity in CommonUtilities.World.Filter<CGrid>().End())
            {
                var grid = CommonUtilities.World.GetPool<CGrid>().Get(entity).Grid;
                return grid.CellToWorld(grid.WorldToCell(position));
            }

            throw new Exception("CGrid is not inited!");
        }

        public static Vector3 GetSnappedMousePosition()
        {
            return GetSnappedPosition(GetMouseWorldPosition());
        }
    }
}