using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentController : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Camera _mainCamera;

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _mainCamera = Camera.main;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    var agentPath = new NavMeshPath();
                    if (_agent.CalculatePath(hit.point, agentPath) && 
                        agentPath.status == NavMeshPathStatus.PathComplete)
                    {
                        _agent.SetDestination(Vector3Int.RoundToInt(hit.point));
                        return;
                    }
                    Debug.Log("No Path");
                }
            }
        }
    }
}