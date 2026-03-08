using UnityEngine;

namespace BufoGames.Uis
{
    public class CanvasGroupController : MonoBehaviour
    {
        [SerializeField] CanvasGroup _canvasGroup;

        public bool IsOpen { get; private set; }

        public void Initialize()
        {
            if (_canvasGroup == null)
            {
                Debug.LogError($"{nameof(CanvasGroupController)} on '{name}' requires a CanvasGroup reference.");
                return;
            }

            SyncStateFromCanvas();
        }

        public void Deinitialize()
        {
        }

        public void Show()
        {
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void Toggle()
        {
            SetVisible(!IsOpen);
        }

        public void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null) return;

            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
            IsOpen = isVisible;
        }

        // Kept for backward compatibility with existing prefab method names.
        public void UpdateCanvasGroup()
        {
            Toggle();
        }

        void SyncStateFromCanvas()
        {
            if (_canvasGroup == null)
            {
                IsOpen = false;
                return;
            }

            IsOpen = _canvasGroup.alpha > 0f || _canvasGroup.interactable || _canvasGroup.blocksRaycasts;
        }
    }
}
