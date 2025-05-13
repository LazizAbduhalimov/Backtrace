using System;
using Leopotam.EcsLite;

namespace Client
{
    public class Postponer
    {
        public EcsWorld World;
        
        public Postponer(EcsWorld world)
        {
            World = world;
        }
        
        public ref TimerEvent Delay(Action action, float delayTime)
        {
            ref var timer = ref World.GetPool<TimerEvent>().Add(World.NewEntity());
            timer.Invoke(action, delayTime);
            return ref timer;
        }
        
        public DelayedAction SequenceDelay(Action action, float delayTime)
        {
            return new DelayedAction(World, action, delayTime);
        }
    }
}