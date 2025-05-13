using System;
using Leopotam.EcsLite;

namespace Client
{
    public class DelayedAction
    {
        public EcsWorld World;
        public float DelayTime { get; private set; }

        public DelayedAction(EcsWorld world, Action action, float delayTime)
        {
            World = world;
            DelayTime = delayTime;
            CreateDelayedAction(action, delayTime);
        }

        public DelayedAction Chain(Action action, float delayTime)
        {
            DelayTime += delayTime;
            CreateDelayedAction(action, delayTime);
            return this;
        }

        private void CreateDelayedAction(Action action, float delayTime)
        {
            World.GetPool<TimerEvent>().Add(World.NewEntity()).Invoke(action, delayTime);
        }
    }
}