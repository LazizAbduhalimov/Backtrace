using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentController : MonoBehaviour
    {
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

                if (Physics.Raycast(ray, out var hit))
                {
                    var layerName = LayerMask.LayerToName(hit.transform.gameObject.layer);
                    if (layerName != "Ground" && layerName != "Door")
                    {
                        Debug.Log("No Path");
                        return;
                    }
                    var agentPath = new NavMeshPath();
                    if (_agent.CalculatePath(hit.point, agentPath) && 
                        agentPath.status == NavMeshPathStatus.PathComplete)
                    {
                        var destination = Vector3Int.RoundToInt(hit.point);
                        if (_agent.destination != destination)
                            SoundManager.Instance.PlayFX(AllSfxSounds.Move);
                        _agent.SetDestination(destination);
                        // SoundManager.Instance.PlayFX(AllSfxSounds);
                        return;
                    }
                    Debug.Log("No Path");
                }
            }
        }
    }
}