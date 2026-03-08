using UnityEngine;

namespace BufoGames.Uis
{
    public interface IUIManager
    {
        void Initialize(BufoGames.Managers.GameSceneManager owner);
        void Deinitialize();
        void SetLevelText(int currentLevel);
        void ShowSettings(bool isVisible);
        void ShowEndGame(bool isVisible);
        void PlayEndGameVfx();
    }

    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] HudController _hudController;
        [SerializeField] SettingsPopupController _settingsPopupController;
        [SerializeField] EndGamePopupController _endGamePopupController;

        BufoGames.Managers.GameSceneManager _owner;
        bool _isInitialized;

        public void Initialize(BufoGames.Managers.GameSceneManager owner)
        {
            if (_isInitialized)
            {
                Deinitialize();
            }

            _owner = owner;

            if (_owner == null || _hudController == null || _settingsPopupController == null || _endGamePopupController == null)
            {
                Debug.LogError($"{nameof(UIManager)} on '{name}' has missing serialized references.");
                return;
            }

            _hudController.Initialize();
            _settingsPopupController.Initialize();
            _endGamePopupController.Initialize(_owner.RequestMoveToNextLevel, _owner.RequestReloadCurrentLevel);

            ShowSettings(false);
            ShowEndGame(false);

            _isInitialized = true;
        }

        public void Deinitialize()
        {
            _endGamePopupController?.Deinitialize();
            _settingsPopupController?.Deinitialize();
            _hudController?.Deinitialize();

            _isInitialized = false;
            _owner = null;
        }

        public void SetLevelText(int currentLevel)
        {
            _hudController?.SetLevelText(currentLevel);
        }

        public void ShowSettings(bool isVisible)
        {
            _settingsPopupController?.SetVisible(isVisible);
        }

        public void ShowEndGame(bool isVisible)
        {
            _endGamePopupController?.SetVisible(isVisible);
        }

        public void PlayEndGameVfx()
        {
            _endGamePopupController?.PlayCompletionEffect();
        }
    }
}
