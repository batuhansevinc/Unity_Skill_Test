using BatuhanSevinc.Abstracts.Patterns;
using BatuhanSevinc.ScriptableObjects;
using BufoGames.Controller;
using BufoGames.Data;
using BufoGames.Generation;
using UnityEngine;

namespace BufoGames.Managers
{
    public class LevelManager : SingletonMonoDestroy<LevelManager>
    {
        [SerializeField] private bool isTesting = true;
        [SerializeField] private string id = "CurrentLevel";
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private ThemeDataSO defaultTheme;
        [SerializeField] private GameEvent levelCompletedEvent;
        [SerializeField] private GameEvent fireworksEvent;
        [SerializeField] private GameEvent startEndGameAnimationsEvent;
        
        public Transform upTarget;
        public Transform downTarget;
        public Transform leftTarget;
        public Transform rightTarget;

        private LevelGenerator levelGenerator;
        private GameObject currentLevelInstance;
        private LevelController currentLevelController;
        private int currentLevelIndex;
        
        public int CurrentLevel => currentLevelIndex + 1;
        public int TotalLevels => levelDatabase != null ? levelDatabase.GetTotalLevelCount() : 0;
        public LevelController CurrentLevelController => currentLevelController;

        private void Awake()
        {
            SetSingleton(this);
            EnsureInputManager();
            InitializeGenerator();
        }
        
        private void Start()
        {
            InitializeGame();
        }
        
        private void EnsureInputManager()
        {
            var inputManager = FindObjectOfType<InputManager>();
            if (inputManager == null)
            {
                GameObject inputObj = new GameObject("InputManager");
                inputObj.AddComponent<InputManager>();
            }
        }

        private void InitializeGenerator()
        {
            GameObject generatorObj = new GameObject("LevelGenerator");
            generatorObj.transform.SetParent(transform);
            levelGenerator = generatorObj.AddComponent<LevelGenerator>();
            
            if (defaultTheme != null)
            {
                levelGenerator.SetDefaultTheme(defaultTheme);
            }
        }
        
        private void InitializeGame()
        {
            if (levelDatabase == null || levelDatabase.GetTotalLevelCount() == 0) return;
            if (levelGenerator == null) return;
            
            if (isTesting)
            {
                currentLevelIndex = currentLevel - 1;
            }
            else
            {
                currentLevelIndex = PlayerPrefs.GetInt(id, 0);
            }
            
            LoadLevel(currentLevelIndex);
        }

        public void LoadLevel(int levelIndex)
        {
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
                currentLevelInstance = null;
                currentLevelController = null;
            }
            
            if (levelIndex < 0 || levelIndex >= TotalLevels) return;
            
            LevelDataSO levelData = levelDatabase.GetLevel(levelIndex);
            if (levelData == null) return;
            
            currentLevelInstance = levelGenerator.GenerateLevel(levelData);
            if (currentLevelInstance == null) return;
            
            currentLevelController = currentLevelInstance.GetComponent<LevelController>();
            
            if (currentLevelController != null)
            {
                currentLevelController.SetGameEvents(levelCompletedEvent, fireworksEvent, startEndGameAnimationsEvent);
                AdjustCameraTargets(currentLevelController);
            }
            
            currentLevelIndex = levelIndex;
        }
        
        public void MoveToNextLevel()
        {
            if (currentLevelIndex + 1 < TotalLevels)
            {
                currentLevelIndex++;
            }
            else
            {
                currentLevelIndex = 0;
            }
            SaveProgress();
            LoadLevel(currentLevelIndex);
        }

        public void MoveToPreviousLevel()
        {
            if (currentLevelIndex > 0)
            {
                currentLevelIndex--;
                SaveProgress();
                LoadLevel(currentLevelIndex);
            }
        }
        
        public void ReloadCurrentLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt(id, currentLevelIndex);
            PlayerPrefs.Save();
        }

        private void AdjustCameraTargets(LevelController levelController)
        {
            if (upTarget == null || downTarget == null || leftTarget == null || rightTarget == null) return;
            
            var gridData = levelController.GetGridData();
            if (gridData == null) return;
            
            upTarget.position = gridData.GetUpTargetPosition();
            downTarget.position = gridData.GetDownTargetPosition();
            leftTarget.position = gridData.GetLeftTargetPosition();
            rightTarget.position = gridData.GetRightTargetPosition();
        }
    }
}