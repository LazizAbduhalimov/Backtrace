using System;
using Unity.AI.Navigation;
using UnityEngine;

namespace Client.Game
{
    public class NavMeshRebuilder : MonoBehaviour
    {
        public bool RebuildAtStart;
        public NavMeshSurface NavMeshSurface;

        private void Awake()
        {
            if (RebuildAtStart)
                NavMeshSurface.BuildNavMesh();
        }
    }
}