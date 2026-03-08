using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace BufoGames.Uis
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] Slider loadingSlider;
        [SerializeField] float loadingTime = 5f;

        Tween _loadingTween;

        public void Initialize(System.Action onComplete)
        {
            if (loadingSlider == null)
            {
                Debug.LogError($"{nameof(SplashScreenController)} on '{name}' requires a Slider reference.");
                return;
            }

            Deinitialize();
            loadingSlider.value = 0f;
            _loadingTween = loadingSlider.DOValue(1f, loadingTime).SetEase(Ease.Linear).OnComplete(() => onComplete?.Invoke());
        }

        public void Deinitialize()
        {
            if (_loadingTween != null && _loadingTween.IsActive())
            {
                _loadingTween.Kill();
            }

            _loadingTween = null;
        }
    }
}
