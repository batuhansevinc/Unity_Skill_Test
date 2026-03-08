using UnityEngine;

namespace BufoGames.Uis
{
    public class SettingsPopupController : MonoBehaviour
    {
        [SerializeField] CanvasGroupController _settingsPanel;
        [SerializeField] SettingsButton _openButton;
        [SerializeField] CloseSettingsButton _closeButton;
        [SerializeField] SoundSettingsButton _soundSettingsButton;
        [SerializeField] VibrationSettingsButton _vibrationSettingsButton;

        public void Initialize()
        {
            if (_settingsPanel == null || _openButton == null || _closeButton == null
                || _soundSettingsButton == null || _vibrationSettingsButton == null)
            {
                Debug.LogError($"{nameof(SettingsPopupController)} on '{name}' has missing serialized references.");
                return;
            }

            _settingsPanel.Initialize();
            _openButton.Initialize(Show);
            _closeButton.Initialize(Hide);
            _soundSettingsButton.Initialize();
            _vibrationSettingsButton.Initialize();
        }

        public void Deinitialize()
        {
            _openButton?.Deinitialize();
            _closeButton?.Deinitialize();
            _soundSettingsButton?.Deinitialize();
            _vibrationSettingsButton?.Deinitialize();
            _settingsPanel?.Deinitialize();
        }

        public void Show()
        {
            _settingsPanel?.SetVisible(true);
        }

        public void Hide()
        {
            _settingsPanel?.SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            _settingsPanel?.SetVisible(isVisible);
        }
    }
}
