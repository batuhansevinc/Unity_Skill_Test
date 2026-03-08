using BufoGames.Uis;
using UnityEngine;

namespace BufoGames.Managers
{
    public class SplashSceneManager : MonoBehaviour
    {
        [SerializeField] SplashScreenController _splashScreenController;
        [SerializeField] GameManager _gameManager;

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

            if (_splashScreenController == null || _gameManager == null)
            {
                Debug.LogError($"{nameof(SplashSceneManager)} on '{name}' has missing serialized references.");
                return;
            }

            _splashScreenController.Initialize(HandleSplashCompleted);
            _isInitialized = true;
        }

        public void Deinitialize()
        {
            if (!_isInitialized)
            {
                return;
            }

            _splashScreenController.Deinitialize();
            _isInitialized = false;
        }

        void HandleSplashCompleted()
        {
            _gameManager.LoadNextLevel();
        }
    }
}
