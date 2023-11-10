using System;
using Assignment01.Controller;
using BatuhanSevinc.Abstracts.Patterns;
using BatuhanSevinc.ScriptableObjects;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assignment01.Managers
{
    public class LevelManager : SingletonMonoDestroy<LevelManager>
    {
        [Header("Settings")]
        [SerializeField] bool _isTesting = true;
        [SerializeField] string _id;

        [Header("Levels")]
        [SerializeField] int _currentLevel;
        [SerializeField, ReadOnly] GameObject _currentLevelPrefab;
        [SerializeField] GameObject[] _levelPrefabs;
        [Header("Camera Targets")]
        public Transform upTarget;
        public Transform downTarget;
        public Transform leftTarget;
        public Transform rightTarget;

        public int CurrentLevel => _currentLevel;

        private void Awake()
        {
            SetSingleton(this);
        }

        private void Start()
        {
            InitializeGame();
        }

        private void InitializeGame()
        {
            if (_isTesting) 
            {
                LoadLevelByIndex(_currentLevel - 1);
                return;
            }

            var saveLoadManager = PlayerPrefs.GetInt(_id, 1);
            _currentLevel = saveLoadManager;
            LoadLevelByIndex(_currentLevel - 1);
        }


        private bool IsNextLevelExists()
        {
            return _currentLevel + 1 <= _levelPrefabs.Length;
        }

        private bool IsPreviousLevelExists()
        {
            return _currentLevel - 1 >= 1;
        }

        public void MoveToNextLevel()
        {
            if (IsNextLevelExists())
            {
                _currentLevel++;
                SaveCurrentLevel();
                LoadLevelByIndex(_currentLevel - 1);
            }
        }

        public void MoveToPreviousLevel()
        {
            if (IsPreviousLevelExists())
            {
                _currentLevel--;
                SaveCurrentLevel();
                LoadLevelByIndex(_currentLevel - 1);
            }
        }

        private void SaveCurrentLevel()
        {
            PlayerPrefs.SetInt(_id, _currentLevel);
        }

        private void LoadLevelByIndex(int index)
        {
            if (_currentLevelPrefab != null)
            {
                Destroy(_currentLevelPrefab);
            }

            if (index >= 0 && index < _levelPrefabs.Length)
            {
                _currentLevelPrefab = Instantiate(_levelPrefabs[index]);
                LevelController levelController = _currentLevelPrefab.GetComponent<LevelController>();
                levelController.InitializeGrid();
                AdjustCameraTargets(levelController);
            }
        }
        private void AdjustCameraTargets(LevelController levelController)
        {
            float maxX = (levelController.GetGridSize() - 1) * levelController.GetXInterval();
            float maxZ = (levelController.GetGridSize() - 1) * levelController.GetZInterval();

            upTarget.position = new Vector3(maxX / 2f, 0, maxZ + levelController.GetZInterval());
            downTarget.position = new Vector3(maxX / 2f, 0, -levelController.GetZInterval());
            leftTarget.position = new Vector3(-levelController.GetXInterval(), 0, maxZ / 2f);
            rightTarget.position = new Vector3(maxX + levelController.GetXInterval(), 0, maxZ / 2f);
        }
    }
}