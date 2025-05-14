using UnityEngine;

namespace Client
{
    public class DecorRewindBody : RewindBodyBase
    {
        public Behaviour[] Components;
        
        public override void StartRewind()
        {
            base.StartRewind();
            if (Components == null) return;
            foreach (var component in Components)
            {
                component.enabled = false;
            }
        }
    
        public override void StopRewind()
        {
            base.StopRewind();
            if (Components == null) return;
            foreach (var component in Components)
            {
                component.enabled = true;
            }
        }
    }
}