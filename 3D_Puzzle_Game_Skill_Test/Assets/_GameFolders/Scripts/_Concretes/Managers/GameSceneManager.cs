using BufoGames.Controller;
using BufoGames.Uis;
using UnityEngine;

namespace BufoGames.Managers
{
    public class GameSceneManager : MonoBehaviour
    {
        [SerializeField] LevelManager _levelManager;
        [SerializeField] UIManager _uiManager;
        [SerializeField] InputManager _inputManager;
        [SerializeField] Camera _mainCamera;

        LevelController _currentLevelController;
        bool _isInitialized;

        void Awake()
        {
            Initialize();
        }

        void OnDestroy()
        {
            Deinitialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_levelManager == null || _uiManager == null || _inputManager == null || _mainCamera == null)
            {
                Debug.LogError($"{nameof(GameSceneManager)} on '{name}' has missing serialized references.");
                return;
            }

            _inputManager.Initialize(_mainCamera);
            _levelManager.Initialize(_inputManager);
            _uiManager.Initialize(this);

            _levelManager.LevelLoaded += HandleLevelLoaded;
            _levelManager.ReloadCurrentLevel();

            _isInitialized = true;
        }

        public void Deinitialize()
        {
            if (!_isInitialized)
            {
                return;
            }

            _levelManager.LevelLoaded -= HandleLevelLoaded;
            UnsubscribeFromCurrentLevelEvents();

            _uiManager.Deinitialize();
            _levelManager.Deinitialize();
            _inputManager.Deinitialize();

            _isInitialized = false;
        }

        public void RequestMoveToNextLevel()
        {
            _levelManager?.MoveToNextLevel();
        }

        public void RequestReloadCurrentLevel()
        {
            _levelManager?.ReloadCurrentLevel();
        }

        void HandleLevelLoaded(LevelController controller, int currentLevel)
        {
            UnsubscribeFromCurrentLevelEvents();
            _currentLevelController = controller;

            if (_currentLevelController != null)
            {
                _currentLevelController.LevelCompletionAnimationStarted += HandleLevelCompletionAnimationStarted;
                _currentLevelController.FireworksTriggered += HandleFireworksTriggered;
                _currentLevelController.LevelCompleted += HandleLevelCompleted;
            }

            _uiManager.SetLevelText(currentLevel);
            _uiManager.ShowSettings(false);
            _uiManager.ShowEndGame(false);
        }

        void HandleLevelCompletionAnimationStarted()
        {
            _uiManager.ShowEndGame(true);
        }

        void HandleFireworksTriggered()
        {
            _uiManager.PlayEndGameVfx();
        }

        void HandleLevelCompleted()
        {
            _uiManager.SetLevelText(_levelManager.CurrentLevel);
        }

        void UnsubscribeFromCurrentLevelEvents()
        {
            if (_currentLevelController == null)
            {
                return;
            }

            _currentLevelController.LevelCompletionAnimationStarted -= HandleLevelCompletionAnimationStarted;
            _currentLevelController.FireworksTriggered -= HandleFireworksTriggered;
            _currentLevelController.LevelCompleted -= HandleLevelCompleted;
            _currentLevelController = null;
        }
    }
}
