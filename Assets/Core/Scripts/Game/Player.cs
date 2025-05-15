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
        public static bool IsDead = false;
        public PostProcessVolume PostProcessVolume;
        public CanvasGroup TextUI;
        public float GreyAmount;
        public float Duration = 0.1f;
        private ColorGrading ColorGrading;
        private Tween? _tween;
        private Tween? _ui;
        
        private void Start()
        {
            ColorGrading = PostProcessVolume.profile.GetSetting<ColorGrading>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<DumbAI>(out var dumbAI))
            {
                IsDead = true;
                _tween?.Complete();
                _tween = Tween.Custom(0, GreyAmount, Duration,
                    value => ColorGrading.saturation.value = value, useUnscaledTime: true);
                _ui?.Complete();
                _ui = Tween.Alpha(TextUI, 1, duration: 0.1f, useUnscaledTime: true);
                SoundManager.Instance.PlayFX(AllSfxSounds.Dead);
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
                IsDead = false;
                _tween?.Complete();
                _tween = Tween.Custom(GreyAmount, 0, Duration,
                    value => ColorGrading.saturation.value = value, useUnscaledTime: true);
                _ui?.Complete();
                _ui = Tween.Alpha(TextUI, 0, duration: 0.1f, useUnscaledTime: false);
            }
        }
    }
}