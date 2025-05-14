using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Client.Game.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DumbAI : MonoBehaviour
    {
        [SerializeField] private float _delay = 1f;
        public AIPath[] Path;

        private int _index;
        private float _passedTime;
        private float _delayPassed;
        private NavMeshAgent _navMeshAgent;
        private Vector3 LastDestination;
        private bool a;

        private void Start()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (Path.Length < 1 || _index >= Path.Length) return;
            var path = Path[_index];
            if (_delayPassed < _delay)
            {
                _delayPassed += Time.deltaTime;
                return;
            }
            if (a == false)
            {
                LastDestination = transform.position;
                _navMeshAgent.SetDestination(LastDestination);
                a = true;
            }
            
            var distance = Vector3.Distance(transform.position.WithY(0), LastDestination.WithY(0));
            Debug.DrawRay(transform.position, Vector3.up, Color.cyan);
            Debug.DrawRay(LastDestination, Vector3.up, Color.blue);
            if (distance < 0.01f)
                _passedTime += Time.deltaTime;
            
            if (_passedTime >= path.TimeToNextPoint)
            {
                _passedTime = 0;
                _index++;
                LastDestination = path.Target.position;
                _navMeshAgent.SetDestination(path.Target.position);
            }
        }
    }

    [Serializable]
    public struct AIPath
    {
        public Transform Target;
        public float TimeToNextPoint;
    }
}