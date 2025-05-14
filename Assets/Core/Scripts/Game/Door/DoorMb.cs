using System;
using PrimeTween;
using UnityEngine;

namespace Client
{
    public class DoorMb : MonoBehaviour
    {
        [SerializeField] private bool _isOpen;
        
        private Tween? _tween;
        private Vector3 _defaultPosition;

        private void Start()
        {
            _defaultPosition = transform.position;
            if (_isOpen)
            {
                Open(false);
                _tween?.Complete();
            }
        }

        public void SwitchDoor()
        {
            _isOpen = !_isOpen;
            if (IsOpen()) Close();
            else Open();
        }
        
        public void Open(bool useSound = true)
        {
            if (useSound)
                SoundManager.Instance.PlayFX(AllSfxSounds.Door);
            _tween?.Stop();
            _tween = Tween.PositionAtSpeed(
                transform, _defaultPosition.AddY(-2.25f), 5, Ease.OutSine);
            Debug.Log($"Opening door {name}");
        }

        public void Close(bool useSound = true)
        {
            if (useSound)
                SoundManager.Instance.PlayFX(AllSfxSounds.Door);
            _tween?.Stop();
            _tween = Tween.PositionAtSpeed(
                transform, _defaultPosition, 5, Ease.OutSine);
            Debug.Log($"Closing door {name}");
        }

        public bool IsOpen()
        {
            return transform.position != _defaultPosition;
        }
    }
}