using Assignment01.Helpers;
using BatuhanSevinc.Enums;
using BatuhanSevinc.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Assignment01.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Rodd Games/Container/Area Data Container", fileName = "Area Image Container")]
    public class LevelDataContainerSO : ScriptableObject
    {
        [SerializeField,BoxGroup("Basic Info")] string _id;
        [SerializeField, ReadOnly,BoxGroup("Basic Info")] string _levelName;
        [SerializeField,BoxGroup("Area Info")] GameObject _areaPrefab;
        [SerializeField,BoxGroup("Area Info")] protected TextAsset _textAsset;
        [SerializeField,BoxGroup("UI Info")] GameObject _uiPrefab;
        [SerializeField,BoxGroup("UI Info")] Sprite _mapIcon;
        [SerializeField,BoxGroup("UI Info")] string _mapNameText;// added this line for the UI prefab
        [SerializeField,BoxGroup("Level Info")] int _levelProcessValue = 0;
        [SerializeField,ReadOnly,BoxGroup("Level Info")] int _maxLevelProcess = 0;
        [SerializeField,BoxGroup("Level Info")] bool _isLevelCompleted = false;
        [SerializeField,BoxGroup("Level Info")] bool _isLevelBought = false;
        

        public GameObject AreaPrefab => _areaPrefab;

        public GameObject UIPrefab => _uiPrefab; // added this line for the UI prefab getter

        public int LevelProcessValue => _levelProcessValue;

        public int MaxLevelProcess => _maxLevelProcess;

        public bool IsLevelCompleted => _isLevelCompleted;

        public Sprite MapIcon => _mapIcon;

        public string MapNameText => _mapNameText;

        public bool IsLevelBought => _isLevelBought;

        public event System.Action<bool> OnLevelCompleted;
        public event System.Action OnLevelProcessValueIncreased;

        void Awake()
        {
            if(string.IsNullOrEmpty(_id)) CreateId();
        }

        void OnValidate()
        {
            if (!_levelName.Equals(this.name))
            {
                _levelName = this.name;
            }
        }

        void OnEnable()
        {
            _isLevelCompleted = false;
            _levelProcessValue = 0;
            LoadData();
        }

        [Button(ButtonSizes.Gigantic),BoxGroup("Buttons")]
        private void CreateId()
        {
            _id = IdGeneratorHelper.GenerateID();
        }
        
        [Button(ButtonSizes.Gigantic),BoxGroup("Buttons")]
        private void SetLevelName()
        {
            _levelName = this.name;
        }

        [Button(ButtonSizes.Gigantic),BoxGroup("Buttons")]
        private void SetLevelsZero()
        {
            _levelProcessValue = 0;
            _maxLevelProcess = 0;
        }

        public void IncreaseLevelProcessValue(int increaseValue)
        {
            _levelProcessValue = LevelProcessValue + increaseValue;

            if (LevelProcessValue >= _maxLevelProcess)
            {
                _isLevelCompleted = true;
                SaveData();
                OnLevelCompleted?.Invoke(IsLevelCompleted);
            }

            OnLevelProcessValueIncreased?.Invoke();
        }

        public void IncreaseMaxLevelProcessWhenLevelStarted(int increaseValue)
        {
            _maxLevelProcess += increaseValue;
        }

        public void SetMaxLevelZero()
        {
            _maxLevelProcess = 0;
        }

        public void SetLevelBought()
        {
            _isLevelBought = true;
            SaveData();
        }
        
        void SaveData()
        {
            var saveLoadManager = SaveLoadManager.CreateInstance(SaveLoadType.PlayerPrefs);
            saveLoadManager.SaveDataProcess(_id, new AreaDataEntity()
            {
                IsLevelCompleted = IsLevelCompleted,
                IsLevelBought = IsLevelBought
            });
        }

        void LoadData()
        {
            if (IsLevelCompleted) return;
            
            var saveLoadManager = SaveLoadManager.CreateInstance(SaveLoadType.PlayerPrefs);
            if (saveLoadManager.HasKeyAvailable(_id))
            {
                var entityData = saveLoadManager.LoadDataProcess<AreaDataEntity>(_id);
                _isLevelCompleted = entityData.IsLevelCompleted;
                _isLevelBought = entityData.IsLevelBought;
            }
            else
            {
                _isLevelCompleted = false;
                _isLevelBought = false;
            }
        }
        
        
    }

    public struct AreaDataEntity
    {
        public bool IsLevelCompleted { get; set; }
        public bool IsLevelBought { get; set; }
    }
}