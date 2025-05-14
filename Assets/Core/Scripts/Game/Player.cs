using System;
using Client.Game.AI;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Client.Game
{
    public class Player : MonoBehaviour
    {
        public PostProcessVolume PostProcessVolume;
        public float GreyAmount;
        public float Duration = 0.1f;
        private ColorGrading ColorGrading;
        private Tween? _tween;
        
        private void Start()
        {
            ColorGrading = PostProcessVolume.profile.GetSetting<ColorGrading>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DumbAI>(out var dumbAI))
            {
                _tween?.Complete();
                _tween = Tween.Custom(0, GreyAmount, Duration,
                    value => ColorGrading.saturation.value = value, useUnscaledTime: true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.TryGetComponent<DumbAI>(out var dumbAI))
            {
                RewindManager.Instance.SetTimeScale(0f);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<DumbAI>(out var dumbAI))
            {
                _tween?.Complete();
                _tween = Tween.Custom(GreyAmount, 0, Duration,
                    value => ColorGrading.saturation.value = value, useUnscaledTime: true);
                Debug.Log("UnKilled");
            }
        }
    }
}