using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Client.Game.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DumbAI : MonoBehaviour
    {
        [SerializeField] private FieldOfView _fieldOfView;
        [SerializeField] private float _delay = 1f;
        [SerializeField] private bool _dissapearWhenDoneParoling;
        public List<AIPath> Path;

        public int Index;
        public float PassedTime;
        private float _delayPassed;
        private NavMeshAgent _navMeshAgent;
        private Vector3 _lastDestination;
        private bool _startedToWalk;
        public AIState State;
        public Transform Target;

        private void Start()
        {
            var newTarget = new GameObject("Origin");
            newTarget.transform.position = transform.position;
            Path.Insert(0, new AIPath() {Target = newTarget.transform, TimeToNextPoint = _delay});
            _navMeshAgent = GetComponent<NavMeshAgent>();
            State = AIState.Patrol;
        }

        private void Update()
        {
            if (_fieldOfView.VisibleTargets.Count > 0)
            {
                State = AIState.Engaged;
                _navMeshAgent.speed = 5f;
            }

            if (State == AIState.Engaged && _fieldOfView.VisibleTargets.Count > 0) Target = _fieldOfView.VisibleTargets.First();
            if (Target != null) _navMeshAgent.SetDestination(Target.position);
            if (State == AIState.Patrol) Patrol();
        }

        private void Patrol()
        {
            if (Index >= Path.Count && _dissapearWhenDoneParoling)
                gameObject.SetActive(false);
            if (Path.Count < 1 || Index >= Path.Count) return;
            var path = Path[Index];
            
            var distance = Vector3.Distance(transform.position.WithY(0), path.Target.position);
            Debug.DrawRay(transform.position, Vector3.up, Color.cyan);
            Debug.DrawRay(path.Target.position, Vector3.up, Color.blue);
            if (distance < 0.01f)
                PassedTime += Time.deltaTime;
            // else 
            
            if (PassedTime >= path.TimeToNextPoint)
            {
                if (path.ObjectToInteract != null)
                    path.ObjectToInteract.Switch();
                PassedTime -= path.TimeToNextPoint;
                Index++;
                _navMeshAgent.SetDestination(Path[Index].Target.position);
            }
        }
    }

    [Serializable]
    public struct AIPath
    {
        public Transform Target;
        public float TimeToNextPoint;
        public DoorKeyMb ObjectToInteract;
        [HideInInspector] public float WalkTime;
    }

    public enum AIState
    {
        Patrol,
        Engaged
    }
}