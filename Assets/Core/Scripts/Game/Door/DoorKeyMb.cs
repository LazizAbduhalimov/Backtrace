using System;
using UnityEngine;

namespace Game
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