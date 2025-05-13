using System.Collections.Generic;
using Client.Game;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Client
{
    [RequireComponent(typeof(TransformSwayer))]
    public class DecorRewindBody : RewindBodyBase
    {
        public Behaviour[] Components;
        
        public override void StartRewind()
        {
            base.StartRewind();
            foreach (var component in Components)
            {
                component.enabled = false;
            }
        }
    
        public override void StopRewind()
        {
            base.StopRewind();
            foreach (var component in Components)
            {
                component.enabled = true;
            }
        }
    }
}