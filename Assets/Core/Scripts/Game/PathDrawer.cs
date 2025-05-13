using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    public class PathDrawer : MonoBehaviour
    {
        public NavMeshAgent Agent;
        public LineRenderer LineRenderer;

        private Camera _mainCamera;
        private Vector3 _lastHitPosition; 

        private void Start()
        {
            LineRenderer.positionCount = 0;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            DrawPath(_lastHitPosition);
            if (Agent.IsMoving() && !Input.GetMouseButtonDown(0)) return;
            
            if (Physics.Raycast(ray, out var hit))
            {
                _lastHitPosition = Vector3Int.RoundToInt(hit.point);
            }
            else if (!Agent.IsMoving())
            {
                LineRenderer.positionCount = 0;
            }
        }

        private void DrawPath(Vector3 target)
        {
            var navPath = new NavMeshPath();
            if (!Agent.CalculatePath(target, navPath) || navPath.status != NavMeshPathStatus.PathComplete) return;
            LineRenderer.positionCount = navPath.corners.Length;
            for (var i = 0; i < navPath.corners.Length; i++)
            {
                LineRenderer.SetPosition(i, navPath.corners[i]);
            }
        }
    }
}