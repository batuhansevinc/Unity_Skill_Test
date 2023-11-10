using Assignment01.ScriptableObjects;
using BatuhanSevinc.Enums;
using BatuhanSevinc.Helpers;
using BatuhanSevinc.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Assignment01.Abstracts.Uis
{
    public abstract class BaseSettingsSwitchButton : MonoBehaviour
    {
        [SerializeField] AudioSource _buttonPlaySound;
        [SerializeField] Toggle _toggle; // Changed this to Toggle
        [SerializeField] protected GameObject _onImage;
        [SerializeField] protected GameObject _offImage;
        [SerializeField] protected bool _isOpen = true;

        protected virtual void OnValidate()
        {
            this.GetReference(ref _toggle); // Updated this line for Toggle
        }

        protected virtual void OnEnable()
        {
            _toggle.onValueChanged.AddListener(HandleOnToggleValueChanged); // Updated for Toggle
        }

        protected virtual void OnDisable()
        {
            _toggle.onValueChanged.RemoveListener(HandleOnToggleValueChanged); // Updated for Toggle
        }

        protected virtual void HandleOnToggleValueChanged(bool isOn) // Updated to accept bool parameter
        {
            if (_buttonPlaySound != null)
                _buttonPlaySound.Play();
            _isOpen = isOn; // Updated to use the Toggle's isOn property
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
            SaveData(key, _isOpen);
        }
    }
}
