using System;
using UnityEngine;

namespace Client.Game
{
    public class WinCondition : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                Debug.Log("Win!");
            }
        }
    }
}