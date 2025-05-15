using System;
using UnityEngine;

namespace Client.Game
{
    public class DoorKeyMb : MonoBehaviour
    {
        public Transform OpenPosition;
        public DoorMb[] DoorsMb;

        public void Switch()
        {
            foreach (var doorMb in DoorsMb)
            {
                doorMb.SwitchDoor();
            }
        }
    }
}