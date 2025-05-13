using Leopotam.EcsLite;
using UnityEngine;
using Leopotam.EcsLite.Di;

namespace Client {
    public sealed class TimerSystem : IEcsRunSystem
    {
        private readonly EcsCustomInject<Postponer> _postponer;

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _postponer.Value.World.Filter<TimerEvent>().End())
            {
                var pool = _postponer.Value.World.GetPool<TimerEvent>();
                ref var timerEvent = ref pool.Get(entity);

                timerEvent.Time -= Time.deltaTime;
                if (timerEvent.Time <= 0)
                {
                    timerEvent.DelayedAction();
                    pool.Del(entity);
                }
            }
        }
    }
}