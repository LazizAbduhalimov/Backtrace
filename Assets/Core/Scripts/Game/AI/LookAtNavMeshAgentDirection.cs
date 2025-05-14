using System;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class LookAtNavMeshAgentDirection : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _rotationSpeed;
        private NavMeshAgent _agent;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.updateRotation = false;
        }

        private void Update()
        {
            if (_agent.velocity.sqrMagnitude > 0.1f)
            {
                var direction = _agent.velocity.normalized;
                var targetRotation = Quaternion.LookRotation(direction);
                if (_rotationSpeed > 0)
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                        Time.deltaTime * _rotationSpeed);
            }
            else
            {
                var forward = transform.forward;
                forward.y = 0f;

                // Выбираем ближайшую ось (X или Z)
                if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
                    forward = forward.x >= 0 ? Vector3.right : Vector3.left;
                else
                    forward = forward.z >= 0 ? Vector3.forward : Vector3.back;

                if (_rotationSpeed > 0)
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward),
                    Time.deltaTime * _rotationSpeed);
            }
        }
    }
}