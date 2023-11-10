using BatuhanSevinc.Abstracts.DataAccessLayers;
using BatuhanSevinc.Enums;
using BatuhanSevinc.Helpers;
using UnityEngine;

namespace BatuhanSevinc.Managers
{
    public class SaveLoadManager
    {
        static readonly object _lock = new object();
        static SaveLoadManager _instance;

        IDataSaveLoadDal _dataSaveLoadDal;

        public static SaveLoadManager CreateInstance(SaveLoadType saveLoadType)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new SaveLoadManager();
                }

                _instance.SetDalOperation(saveLoadType);

                return _instance;
            }
        }

        private SaveLoadManager()
        {
        }

        private void SetDalOperation(SaveLoadType saveLoadType)
        {
            IDataSaveLoadDal dataSaveLoadDal = SaveLoadDalFactory.CreateInstance(saveLoadType);
            if (!dataSaveLoadDal.Equals(_dataSaveLoadDal))
            {
                _dataSaveLoadDal = dataSaveLoadDal;
            }
        }

        public void SaveDataProcess(string key, object value)
        {
            Debug.Log($"<color=red>{key}</color> <color=red>{value}</color> data saved");
            _dataSaveLoadDal.SaveData(key, value);
        }

        public T LoadDataProcess<T>(string key)
        {
            Debug.Log($"<color=red>{key}</color> data loaded");
            var value = _dataSaveLoadDal.LoadData<T>(key);
            return value;
        }

        public void SaveUnityObjectProcess(string key, UnityEngine.Object value)
        {
            Debug.Log($"<color=red>{key}</color> <color=red>{value.name}</color> unity object saved");
            _dataSaveLoadDal.SaveUnityObject(key, value);
        }

        public T LoadUnityObjectProcess<T>(string key) where T : UnityEngine.Object
        {
            Debug.Log($"<color=red>{key}</color> data loaded");
            var value = _dataSaveLoadDal.LoadUnityObject<T>(key);
            return value;
        }

        public bool HasKeyAvailable(string key)
        {
            return _dataSaveLoadDal.HasKey(key);
        }

        public void DeleteData(string name)
        {
            _dataSaveLoadDal.DeleteData(name);
        }
    }
}