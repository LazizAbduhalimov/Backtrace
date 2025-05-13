using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    public class AgentController : MonoBehaviour
    {
        public NavMeshAgent agent;
        public LineRenderer lineRenderer;

        void Start()
        {
            // Настройка LineRenderer (можешь подстроить под себя)
            lineRenderer.positionCount = 0;
            lineRenderer.startColor = Color.green;
            lineRenderer.endColor = Color.green;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        }

        void Update()
        {
            // Обработка клика
            if (Input.GetMouseButtonDown(0))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit))
                {
                    agent.SetDestination(Vector3Int.RoundToInt(hit.point));
                }
            }

            // Обновление линии пути
            DrawPath();
        }

        void DrawPath()
        {
            if (agent.path.corners.Length < 2)
            {
                lineRenderer.positionCount = 0;
                return;
            }

            lineRenderer.positionCount = agent.path.corners.Length;
            for (var i = 0; i < agent.path.corners.Length; i++)
            {
                lineRenderer.SetPosition(i, agent.path.corners[i]);
            }
        }
    }
}