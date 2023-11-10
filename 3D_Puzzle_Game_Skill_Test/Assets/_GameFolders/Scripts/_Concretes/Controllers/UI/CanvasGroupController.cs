using BatuhanSevinc.Helpers;
using BatuhanSevinc.ScriptableObjects;
using UnityEngine;

namespace Assignment01.Uis
{
    public class CanvasGroupController : MonoBehaviour
    {
        [SerializeField] GameEvent _activeOpenButtonEvent;

        [SerializeField] CanvasGroup _canvasGroup;
        //[SerializeField] AudioSource _popUpOpenSound;
        //[SerializeField] AudioSource _popUpCloseSound;

        public bool IsOpen { get; private set; }

        void OnValidate()
        {
            this.GetReference(ref _canvasGroup);
        }

        public void UpdateCanvasGroup()
        {
            _canvasGroup.alpha = (_canvasGroup.alpha == 0) ? 1f : 0f;
            _canvasGroup.interactable = !_canvasGroup.interactable;
            _canvasGroup.blocksRaycasts = !_canvasGroup.blocksRaycasts;
        }
    }
}