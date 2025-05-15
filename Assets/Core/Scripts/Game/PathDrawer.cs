using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    public class PathDrawer : MonoBehaviour
    {
        public LayerMask LayerMask;
        public NavMeshAgent Agent;
        public LineRenderer LineRenderer;
        public Transform Pointer;

        private Camera _mainCamera;
        private Vector3 _lastHitPosition; 

        private void Start()
        {
            LineRenderer.positionCount = 0;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (RewindManager.Instance.IsRewinding || RewindManager.Instance.IsGamePaused())
            {
                TurnOff();
                return;
            }
            var ray =_mainCamera .ScreenPointToRay(Input.mousePosition);
            DrawPath(_lastHitPosition);
            if (Agent.IsMoving() && !Input.GetMouseButtonDown(0)) return;
            
            if (Physics.Raycast(ray, out var hit, int.MaxValue, LayerMask))
            {
                _lastHitPosition = Vector3Int.RoundToInt(hit.point);
            }
            else if (!Agent.IsMoving())
            {
                TurnOff();
            }
        }

        private void TurnOff()
        {
            LineRenderer.positionCount = 0;
            Pointer.gameObject.SetActive(false);
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
            if (LineRenderer.positionCount > 0)
            {
                Pointer.gameObject.SetActive(true);
                Pointer.position = LineRenderer.GetPosition(LineRenderer.positionCount-1).AddY(0.05f);
            }
        }
    }
}