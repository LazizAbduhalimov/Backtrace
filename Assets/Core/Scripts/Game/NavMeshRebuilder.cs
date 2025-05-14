using System;
using Unity.AI.Navigation;
using UnityEngine;

namespace Client.Game
{
    public class NavMeshRebuilder : MonoBehaviour
    {
        public NavMeshSurface NavMeshSurface;

        private void Start()
        {
            NavMeshSurface.BuildNavMesh();
        }
    }
}