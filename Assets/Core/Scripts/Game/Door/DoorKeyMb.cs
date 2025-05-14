using UnityEngine;

namespace Client.Game
{
    public class DoorKeyMb : MonoBehaviour
    {
        public Transform OpenPosition;
        public DoorMb[] DoorsMb;
        
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
                foreach (var doorMb in DoorsMb)
                {
                    doorMb.SwitchDoor();
                }
            }
        }
    }
}