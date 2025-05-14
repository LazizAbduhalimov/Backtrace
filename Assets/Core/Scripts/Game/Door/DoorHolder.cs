using System;
using UnityEngine;

namespace Core.Scripts.Game.Door
{
    public class DoorHolder : MonoBehaviour
    {
        [SerializeField] private Material _material;
        [SerializeField] private MeshRenderer[] _meshRenderers;

        private void OnValidate()
        {
            Repaint();
        }

        private void Repaint()
        {
            if (_material == null || _meshRenderers == null || _meshRenderers.Length < 1) return;
            foreach (var renderer in _meshRenderers)
            {
                renderer.material = _material;
            }
        }
    }
}