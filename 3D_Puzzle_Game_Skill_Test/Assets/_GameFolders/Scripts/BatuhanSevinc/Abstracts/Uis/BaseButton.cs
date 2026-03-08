using UnityEngine;
using UnityEngine.UI;

namespace BatuhanSevinc.Uis
{
    public class BaseButton : MonoBehaviour
    {
        [SerializeField] protected Button _button;
        System.Action _clickedCallback;
        bool _isInitialized;

        protected virtual void HandleOnButtonClicked()
        {
            _clickedCallback?.Invoke();
        }

        public virtual void Initialize(System.Action callback)
        {
            if (_isInitialized)
            {
                Deinitialize();
            }

            if (_button == null)
            {
                Debug.LogError($"{nameof(BaseButton)} on '{name}' requires a Button reference.");
                return;
            }

            _clickedCallback = callback;
            _button.onClick.AddListener(HandleOnButtonClicked);
            _isInitialized = true;
        }

        public virtual void Deinitialize()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleOnButtonClicked);
            }

            _clickedCallback = null;
            _isInitialized = false;
        }
    }    
}
