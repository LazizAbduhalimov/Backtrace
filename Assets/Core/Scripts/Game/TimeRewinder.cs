using System;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Game
{
    public class TimeRewinder : MonoBehaviour
    {
        public Slider Slider;

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Slider.maxValue = RewindManager.Instance.RewindLength;
            }

            if (Input.GetKey(KeyCode.R))
            {
                Slider.value = RewindManager.Instance.RewindLength;
            }
        }
    }
}