using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Assignment01.Managers;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] Slider loadingSlider;
    [SerializeField] float loadingTime = 5f;

    private void Start()
    {
        if (loadingSlider == null)
        {
            Debug.LogError("Slider is not assigned in the SplashScreenController");
            return;
        }
        
        loadingSlider.value = 0;
        loadingSlider.DOValue(1, loadingTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            GameManager.Instance.LoadNextLevel();
        });
    }
}