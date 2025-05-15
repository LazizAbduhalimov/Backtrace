using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Game
{
    public class WinCondition : MonoBehaviour
    {
        private float _duration = 0.5f;
        private CanvasGroup _loader;
        private CanvasGroup _thanks;
        private Tween? _tween;

        private void Awake()
        {
            _loader = FindObjectOfType<Loader>().Group;
            _thanks = FindObjectOfType<Thanks>().Group;
            _loader.alpha = 1f;
            _tween = Tween.Alpha(_loader, 0, _duration, Ease.InSine);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                RewindManager.Instance.GameOver = true;
                _tween?.Stop();
                _tween = Tween.Alpha(_loader, 1f, _duration).OnComplete(ChangeScene);
            }
        }

        private void ChangeScene()
        {
            var index = SceneManager.GetActiveScene().buildIndex + 1;
            if (SceneManager.sceneCountInBuildSettings <= index)
            {
                Tween.Alpha(_thanks, 1f, _duration);
            }
            else
            {
                SceneManager.LoadScene(index);
            }
        }
    }
}