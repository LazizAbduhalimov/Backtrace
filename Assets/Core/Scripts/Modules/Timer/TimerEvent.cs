using System;

namespace Game {
    public struct TimerEvent
    {
        public float Time;
        public Action DelayedAction;

        public TimerEvent Invoke(Action action, float time)
        {
            Time = time;
            DelayedAction = action;
            return this;
        }
    }
}