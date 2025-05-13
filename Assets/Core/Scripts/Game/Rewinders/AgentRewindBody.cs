using System.Collections.Generic;
using Client.Game;
using UnityEngine;
using UnityEngine.AI;

namespace Client
{
    public class AgentRewindBody : RewindBodyBase
    {
        private NavMeshAgent _agent;
    
        protected override void Init()
        {
            base.Init();
            _agent = GetComponent<NavMeshAgent>();
        }

        public override void StartRewind()
        {
            base.StartRewind();
    
            _agent.ResetPath();
            _agent.isStopped = true;
            Debug.Log("Rewinding");
        }
    
        public override void StopRewind()
        {
            base.StopRewind();
    
            _agent.ResetPath();
            _agent.isStopped = false;
            Debug.Log("Stopped rewinding");
        }
    }
}