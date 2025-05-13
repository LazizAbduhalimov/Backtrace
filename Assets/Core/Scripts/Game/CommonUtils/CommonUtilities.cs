using System;
using Leopotam.EcsLite;
using UnityEngine;

namespace Client
{
    public static class CommonUtilities
    {
        public static EcsWorld World;
        public static EcsWorld EventsWorld;
        
        private static Camera _mainCamera;

        public static void Init(EcsWorld world, EcsWorld eventsWorld)
        {
            World = world;
            EventsWorld = eventsWorld;
        }

        public static int FastUnpack(this EcsPackedEntity packed)
        {
            if (!packed.Unpack(World, out var entity))
                throw new Exception("Can't unpack entity");
            return entity;
        }

        public static ref T UnpackAndGetFromPool<T>(this EcsPackedEntity packed) where T : struct
        {
            return ref World.GetPool<T>().Get(packed.FastUnpack());
        }
    }
}