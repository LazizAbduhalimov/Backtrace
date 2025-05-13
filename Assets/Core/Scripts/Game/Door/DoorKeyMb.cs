using System;
using UnityEngine;
using UnityEngine.AI;

namespace Client.Game
{
    public class DoorKeyMb : MonoBehaviour
    {
        public Transform OpenPosition;
        public DoorMb DoorMb;
        
        private Player _player;

        private void Start()
        {
            _player = FindObjectOfType<Player>();
        }

        private void OnMouseDown()
        {
            var distance = Vector3.Distance(_player.transform.position, OpenPosition.position);
            if (distance < 0.1f)
            {
                DoorMb.SwitchDoor();
            }
        }
    }
}