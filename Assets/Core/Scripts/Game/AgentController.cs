using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    public class AgentController : MonoBehaviour
    {
        public NavMeshAgent agent;
        private Camera _mainCamera;

        void Start()
        {
            _mainCamera = Camera.main;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    var agentPath = new NavMeshPath();
                    if (agent.CalculatePath(hit.point, agentPath) && 
                        agentPath.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(Vector3Int.RoundToInt(hit.point));
                        return;
                    }
                    Debug.Log("No Path");
                }
            }
        }
    }
}