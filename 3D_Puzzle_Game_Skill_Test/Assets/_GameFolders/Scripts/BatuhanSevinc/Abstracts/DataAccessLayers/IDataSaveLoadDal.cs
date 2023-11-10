namespace BatuhanSevinc.Abstracts.DataAccessLayers
{
    public interface IDataSaveLoadDal
    {
        void SaveData(string key, object value);
        T LoadData<T>(string key);
        bool HasKey(string key);
        T LoadUnityObject<T>(string key) where T : UnityEngine.Object;
        void SaveUnityObject(string key, UnityEngine.Object value);
        void DeleteData(string key);
    }
}