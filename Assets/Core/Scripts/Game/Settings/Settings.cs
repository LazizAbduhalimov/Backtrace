using System;
using UnityEngine;
using YG;

namespace Game.Settings
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] private int _targetFrameRate = 60;
        private void Start()
        {
            YG2.GameReadyAPI();
            YG2.InterstitialAdvShow();
            Application.targetFrameRate = _targetFrameRate;
        }
    }
}