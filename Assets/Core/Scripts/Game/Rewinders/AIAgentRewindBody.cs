using System.Collections.Generic;
using Client.Game;
using Client.Game.AI;
using UnityEngine;
using UnityEngine.AI;

namespace Client
{
    public class AIAgentRewindBody : RewindBodyBase
    {
        private NavMeshAgent _agent;
        private DumbAI _dumbAI;
        private int _framesRewinded;
        
        protected override void Init()
        {
            base.Init();
            _agent = GetComponent<NavMeshAgent>();
            _dumbAI = GetComponent<DumbAI>();
        }

        protected override void RewindCore()
        {
            if (IsRewinding)
            {
                var framePerStepRewind = RewindManager.Instance.FramePerStepRewind;
                for (var r = 0; r < framePerStepRewind; r++)
                {
                    RewindStep();
                    var index = _dumbAI.Index - 1;
                    if (index < 0) return;
                    Debug.Log(index);
                    var path = _dumbAI.Path[index];
                    Debug.DrawRay(transform.position, Vector3.up, Color.red, 2f);
                    Debug.DrawRay(path.Target.position, Vector3.up, Color.blue, 2f);
                    Debug.Log(_dumbAI.Index-2);
                    var distance = Vector3.Distance(transform.position, path.Target.position);
                    _dumbAI.PassedTime -= Time.fixedDeltaTime;
                    
                    if (_dumbAI.PassedTime <= 0.05f)
                    {
                        // if (path.ObjectToInteract != null)
                        //     path.ObjectToInteract.Switch();
                        _dumbAI.Index--;
                        _dumbAI.PassedTime = _dumbAI.Path[_dumbAI.Index].TimeToNextPoint;
                    }
                }
                return;
            }
            RecordStep();
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
            _dumbAI.Target = null;
            _dumbAI.State = AIState.Patrol;
            Debug.Log("Stopped rewinding");
        }
    }
}