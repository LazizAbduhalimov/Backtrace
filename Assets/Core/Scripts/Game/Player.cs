using System;
using Client.Game.AI;
using Unity.VisualScripting;
using UnityEngine;

namespace Client.Game
{
    public class Player : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DumbAI>(out var dumbAI))
            {
                Debug.Log("Killed");
            }
        }
    }
}