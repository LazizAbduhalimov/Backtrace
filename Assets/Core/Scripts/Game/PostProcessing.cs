using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Client.Game
{
    public class PostProcessing : MonoBehaviour
    {
        [SerializeField] private float _smoothTime;
        [SerializeField, MinMax(0, 1)] private float _maxValue;
        private ChromaticAberration _chromaticAberration;
        private float _passedTime;

        private void Start()
        {
            var postProcessVolume = GetComponent<PostProcessVolume>();
            _chromaticAberration = postProcessVolume.profile.GetSetting<ChromaticAberration>();
        }

        private void Update()
        {
            if (RewindManager.Instance.IsRewinding)
            {
                _passedTime += Time.deltaTime;
            }
            else 
            {
                _passedTime -= Time.deltaTime;
            }

            _passedTime = Mathf.Clamp(_passedTime, 0, _smoothTime);
            _chromaticAberration.intensity.value = _passedTime / _smoothTime * _maxValue;
        }
    }
}