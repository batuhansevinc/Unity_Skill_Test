using BufoGames.Controller;
using BufoGames.Data;
using BufoGames.Generation;
using UnityEngine;

namespace BufoGames.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private bool isTesting = true;
        [SerializeField] private string id = "CurrentLevel";
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private LevelDatabaseSO levelDatabase;
        [SerializeField] private ThemeDataSO defaultTheme;
        
        public Transform upTarget;
        public Transform downTarget;
        public Transform leftTarget;
        public Transform rightTarget;

        private InputManager inputManager;
        private LevelGenerator levelGenerator;
        private GameObject currentLevelInstance;
        private LevelController currentLevelController;
        private int currentLevelIndex;
        private bool isInitialized;
        
        public int CurrentLevel => currentLevelIndex + 1;
        public int TotalLevels => levelDatabase != null ? levelDatabase.GetTotalLevelCount() : 0;
        public LevelController CurrentLevelController => currentLevelController;
        
        public event System.Action<LevelController, int> LevelLoaded;

        public void Initialize(InputManager sceneInputManager)
        {
            if (isInitialized)
            {
                Deinitialize();
            }

            inputManager = sceneInputManager;
            InitializeGenerator();
            InitializeCurrentLevelIndex();
            isInitialized = true;
        }

        public void Deinitialize()
        {
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
                currentLevelInstance = null;
                currentLevelController = null;
            }

            if (levelGenerator != null)
            {
                Destroy(levelGenerator.gameObject);
                levelGenerator = null;
            }

            inputManager = null;
            isInitialized = false;
        }

        private void InitializeGenerator()
        {
            if (levelGenerator != null)
            {
                Destroy(levelGenerator.gameObject);
            }

            GameObject generatorObj = new GameObject("LevelGenerator");
            generatorObj.transform.SetParent(transform);
            levelGenerator = generatorObj.AddComponent<LevelGenerator>();
            
            if (defaultTheme != null)
            {
                levelGenerator.SetDefaultTheme(defaultTheme);
            }
        }
        
        private void InitializeCurrentLevelIndex()
        {
            if (levelDatabase == null || levelDatabase.GetTotalLevelCount() == 0)
            {
                currentLevelIndex = 0;
                return;
            }
            
            if (isTesting)
            {
                currentLevelIndex = currentLevel - 1;
            }
            else
            {
                currentLevelIndex = PlayerPrefs.GetInt(id, 0);
            }

            if (currentLevelIndex < 0 || currentLevelIndex >= TotalLevels)
            {
                currentLevelIndex = 0;
            }
        }

        public void LoadLevel(int levelIndex)
        {
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance);
                currentLevelInstance = null;
                currentLevelController = null;
            }

            if (levelGenerator == null || levelDatabase == null)
            {
                return;
            }
            
            if (levelIndex < 0 || levelIndex >= TotalLevels) return;
            
            LevelDataSO levelData = levelDatabase.GetLevel(levelIndex);
            if (levelData == null) return;
            
            LevelGenerationResult levelResult = levelGenerator.GenerateLevel(levelData);
            currentLevelInstance = levelResult.LevelRoot;
            currentLevelController = levelResult.LevelController;
            
            if (currentLevelController != null)
            {
                AdjustCameraTargets(currentLevelController);
            }
            
            currentLevelIndex = levelIndex;
            LevelLoaded?.Invoke(currentLevelController, CurrentLevel);
            
            // Disable input during spawn animation
            if (inputManager != null)
                inputManager.SetInputEnabled(false);
            
            // Play rain drop spawn animation
            levelGenerator.PlaySpawnAnimation(() =>
            {
                if (inputManager != null)
                    inputManager.SetInputEnabled(true);
                
                if (currentLevelController != null)
                    currentLevelController.OnSpawnAnimationComplete();
            });
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
