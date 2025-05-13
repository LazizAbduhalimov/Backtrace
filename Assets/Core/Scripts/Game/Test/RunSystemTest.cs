using System;
using Leopotam.EcsLite;
using UnityEngine;

namespace Client.Game.Test
{
    public class RunSystemTest : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            Debug.Log("RUN!");
        }
    }
}