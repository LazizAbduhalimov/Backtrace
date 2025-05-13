using System;
using Leopotam.EcsLite;
using UnityEngine;

namespace Client.Game.Test
{
    public class InitSystemTest : IEcsInitSystem
    {
        public void Init(IEcsSystems systems)
        {
            Debug.Log("Init!");
        }
    }
}