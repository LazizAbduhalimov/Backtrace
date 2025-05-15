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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ChangeSceneAnim(SceneManager.GetActiveScene().buildIndex);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                var index = SceneManager.GetActiveScene().buildIndex + 1;
                ChangeSceneAnim(index);
            }
        }

        private void ChangeSceneAnim(int index)
        {
            RewindManager.Instance.GameOver = true;
            _tween?.Stop();
            _tween = Tween.Alpha(_loader, 1f, _duration, useUnscaledTime: true).OnComplete(() =>
            {
                ChangeScene(index);
            });
        }

        private void ChangeScene(int index)
        {
            if (SceneManager.sceneCountInBuildSettings <= index)
            {
                Tween.Alpha(_thanks, 1f, _duration, useUnscaledTime: true);
                RewindManager.Instance.SetTimeScale(0f);
            }
            else
            {
                SceneManager.LoadScene(index);
            }
        }
    }
}