using System;
using UnityEngine;
using YG;

namespace Game.Settings
{
    public class Settings : MonoBehaviour
    {
        [SerializeField] private int _targetFrameRate = 60;

        private void Awake()
        {
            YG2.GameReadyAPI();
        }

        private void Start()
        {
            YG2.InterstitialAdvShow();
            Application.targetFrameRate = _targetFrameRate;
        }
    }
}