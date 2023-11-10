using BatuhanSevinc.Abstracts.DataAccessLayers;
using BatuhanSevinc.Helpers;
using UnityEngine;
using Newtonsoft.Json;

namespace BatuhanSevinc.DataAccessLayers
{
    public class PlayerPrefsDataSaveLoadDal : IDataSaveLoadDal
    {
        public void SaveData(string key, object value)
        {
            string jsonObject = JsonConvert.SerializeObject(value); 
            string encryptDataValue = EncryptHelper.SetEncrypt(jsonObject);
            PlayerPrefs.SetString(key, encryptDataValue);
        }

        public T LoadData<T>(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return default;

            string encryptDataValue = PlayerPrefs.GetString(key);
            string serializeValue = EncryptHelper.GetDecrypt(encryptDataValue);

            if (serializeValue == null) return default;

            T value = JsonConvert.DeserializeObject<T>(serializeValue);
            return value;
        }

        public void SaveUnityObject(string key, UnityEngine.Object value)
        {
            string jsonSerializeValue = JsonUtility.ToJson(value);
            string encryptDataValue = EncryptHelper.SetEncrypt(jsonSerializeValue);

            PlayerPrefs.SetString(key, encryptDataValue);
        }

        public void DeleteData(string key)
        {
            if (!PlayerPrefs.HasKey(key)) return;

            PlayerPrefs.DeleteKey(key);
        }

        public T LoadUnityObject<T>(string key) where T : UnityEngine.Object
        {
            if (!PlayerPrefs.HasKey(key)) return null;

            string encryptDataValue = PlayerPrefs.GetString(key);
            string serializeValue = EncryptHelper.GetDecrypt(encryptDataValue);

            if (string.IsNullOrEmpty(serializeValue)) return null;

            T value = JsonUtility.FromJson<T>(serializeValue);
            return value;
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
    }
}