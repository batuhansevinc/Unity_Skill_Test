using BatuhanSevinc.Enums;
using BatuhanSevinc.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace BufoGames.Abstracts.Uis
{
    public abstract class BaseSettingsSwitchButton : MonoBehaviour
    {
        [SerializeField] AudioSource _buttonPlaySound;
        [SerializeField] protected Toggle _toggle;
        [SerializeField] protected GameObject _onImage;
        [SerializeField] protected GameObject _offImage;
        [SerializeField] protected bool _isOpen = true;
        bool _isInitialized;

        public virtual void Initialize()
        {
            if (_isInitialized)
            {
                Deinitialize();
            }

            if (_toggle == null || _onImage == null || _offImage == null)
            {
                Debug.LogError($"{nameof(BaseSettingsSwitchButton)} on '{name}' has missing serialized references.");
                return;
            }

            _toggle.onValueChanged.AddListener(HandleOnToggleValueChanged);
            _isInitialized = true;
        }

        public virtual void Deinitialize()
        {
            if (_toggle != null)
            {
                _toggle.onValueChanged.RemoveListener(HandleOnToggleValueChanged);
            }

            _isInitialized = false;
        }

        protected virtual void HandleOnToggleValueChanged(bool isOn)
        {
            if (_buttonPlaySound != null)
                _buttonPlaySound.Play();
            _isOpen = isOn;
        }

        protected void SaveData(string key, bool value)
        {
            var saveLoadManager = SaveLoadManager.CreateInstance(SaveLoadType.PlayerPrefs);
            saveLoadManager.SaveDataProcess(key, value);
        }

        protected void LoadData(string key)
        {
            var saveLoadManager = SaveLoadManager.CreateInstance(SaveLoadType.PlayerPrefs);
            if (saveLoadManager.HasKeyAvailable(key))
            {
                _isOpen = saveLoadManager.LoadDataProcess<bool>(key);
            }
        }

        protected void SetValue(bool value, string key, System.Action unmuteSoundCallback = null,
            System.Action muteSoundCallback = null)
        {
            _isOpen = value;

            if (value)
            {
                _offImage.SetActive(false);
                _onImage.SetActive(true);
                unmuteSoundCallback?.Invoke();
            }
            else
            {
                _offImage.SetActive(true);
                _onImage.SetActive(false);
                muteSoundCallback?.Invoke();
            }

            if (_toggle != null && _toggle.isOn != value)
            {
                _toggle.SetIsOnWithoutNotify(value);
            }

            SaveData(key, _isOpen);
        }
    }
}
