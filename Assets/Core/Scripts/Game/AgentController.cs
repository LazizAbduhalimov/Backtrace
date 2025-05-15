using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentController : MonoBehaviour
    {
        public LayerMask LayerMask;
        public int Button = 0;
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
            if (Input.GetMouseButtonDown(Button))
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, int.MaxValue, LayerMask))
                {
                    var agentPath = new NavMeshPath();
                    if (_agent.CalculatePath(Vector3Int.RoundToInt(hit.point), agentPath) && 
                        agentPath.status == NavMeshPathStatus.PathComplete)
                    {
                        var destination = Vector3Int.RoundToInt(hit.point);
                        SoundManager.Instance.PlayFX(AllSfxSounds.Move);
                        _agent.SetDestination(destination);
                        return;
                    }
                    Debug.Log("No Path");
                }
            }
        }
    }
}