using System;
using System.Collections;
using UnityEngine;

namespace BufoGames.Uis
{
    public class EndGamePopupController : MonoBehaviour
    {
        [SerializeField] CanvasGroupController _endGamePanel;
        [SerializeField] NextLevelButton _nextLevelButton;
        [SerializeField] ReTryButton _retryButton;
        [SerializeField] GameObject _particleEffect;
        [SerializeField] AudioSource _confettiSound;
        [SerializeField] float _particleVisibleDuration = 4f;

        Coroutine _particleDisableCoroutine;

        public void Initialize(Action onNextLevelRequested, Action onRetryRequested)
        {
            if (_endGamePanel == null || _nextLevelButton == null || _retryButton == null || _particleEffect == null)
            {
                Debug.LogError($"{nameof(EndGamePopupController)} on '{name}' has missing serialized references.");
                return;
            }

            _endGamePanel.Initialize();
            _nextLevelButton.Initialize(onNextLevelRequested);
            _retryButton.Initialize(onRetryRequested);
            _particleEffect.SetActive(false);
        }

        public void Deinitialize()
        {
            _nextLevelButton?.Deinitialize();
            _retryButton?.Deinitialize();
            _endGamePanel?.Deinitialize();

            if (_particleDisableCoroutine != null)
            {
                StopCoroutine(_particleDisableCoroutine);
                _particleDisableCoroutine = null;
            }

            if (_particleEffect != null)
            {
                _particleEffect.SetActive(false);
            }
        }

        public void SetVisible(bool isVisible)
        {
            _endGamePanel?.SetVisible(isVisible);
        }

        public void PlayCompletionEffect()
        {
            if (_particleEffect == null)
            {
                return;
            }

            _particleEffect.SetActive(true);
            _confettiSound?.Play();

            if (_particleDisableCoroutine != null)
            {
                StopCoroutine(_particleDisableCoroutine);
            }

            _particleDisableCoroutine = StartCoroutine(DisableParticleEffectAfterDelay());
        }

        IEnumerator DisableParticleEffectAfterDelay()
        {
            if (_particleVisibleDuration > 0f)
            {
                yield return new WaitForSeconds(_particleVisibleDuration);
            }

            if (_particleEffect != null)
            {
                _particleEffect.SetActive(false);
            }

            _particleDisableCoroutine = null;
        }
    }
}
