using System;
using Client.Game;
using UnityEngine;

namespace Client
{
    public class KeyOpener : MonoBehaviour
    {
        public LayerMask LayerMask;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out var hit, int.MaxValue, LayerMask))
                {
                    if (!hit.transform.TryGetComponent<DoorKeyMb>(out var doorKeyMb)) return;
                    Debug.Log($"Hit {hit.transform.name}");
                    var distance = Vector3.Distance(transform.position, doorKeyMb.OpenPosition.position);
                    Debug.Log(distance);
                    if (distance < 0.1f)
                    {
                        doorKeyMb.Switch();
                    }
                }
            }
        }
    }
}