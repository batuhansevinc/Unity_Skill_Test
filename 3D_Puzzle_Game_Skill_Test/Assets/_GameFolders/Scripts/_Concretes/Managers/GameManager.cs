using System.Collections;
using BatuhanSevinc.Abstracts.Patterns;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Assignment01.Managers
{
    public class GameManager : SingletonMonoAndDontDestroy<GameManager>
    {
        [SerializeField] int _maxLevelIndex;
        public bool _isPaused = false;
        bool _isMaxLevelReached = false;
        List<int> _unplayedLevels;
        public int PlayerLevelProgress { get; private set; }

        void Awake()
        {
            SetSingleton(this);
            _unplayedLevels = new List<int>();

            PlayerLevelProgress = PlayerPrefs.GetInt("PlayerLevelProgress", 0);

            Application.targetFrameRate = 60;
        }

        public void LoadNextLevel()
        {
            if (_isMaxLevelReached)
            {
                StartCoroutine(LoadRandomLevelAsync());
            }
            else
            {
                StartCoroutine(LoadNextLevelByIndexAsync(SceneManager.GetActiveScene().buildIndex + 1));
            }
        }

        private IEnumerator LoadNextLevelByIndexAsync(int levelIndex)
        {
            if (levelIndex > _maxLevelIndex) 
            {
                _isMaxLevelReached = true;
                yield return LoadRandomLevelAsync();
            }
            else 
            {
                PlayerLevelProgress++;
                PlayerPrefs.SetInt("PlayerLevelProgress", PlayerLevelProgress);
                yield return SceneManager.LoadSceneAsync(levelIndex);
            }
        }

        private IEnumerator LoadRandomLevelAsync()
        {
            int levelIndex;

            if (_unplayedLevels.Count == 0) 
            {
                for(int i = 1; i <= _maxLevelIndex; i++)
                {
                    _unplayedLevels.Add(i);
                }
            }
            
            levelIndex = _unplayedLevels[Random.Range(0, _unplayedLevels.Count)];
            _unplayedLevels.Remove(levelIndex);

            PlayerLevelProgress++;
            PlayerPrefs.SetInt("PlayerLevelProgress", PlayerLevelProgress);

            yield return SceneManager.LoadSceneAsync(levelIndex);
        }

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void TogglePauseGame()
        {
            _isPaused = !_isPaused;
        }
        
        IEnumerator PauseGame()
        {
            yield return new WaitForSeconds(0.5f);
            Time.timeScale = 0f;
        }

        IEnumerator ResumeGame()
        {
            yield return new WaitForSeconds(0.5f);
            Time.timeScale = 1f;
        }
    }    
}
