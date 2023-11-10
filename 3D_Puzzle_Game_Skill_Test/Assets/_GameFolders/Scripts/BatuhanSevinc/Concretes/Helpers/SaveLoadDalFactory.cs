using BatuhanSevinc.Abstracts.DataAccessLayers;
using BatuhanSevinc.DataAccessLayers;
using BatuhanSevinc.Enums;

namespace BatuhanSevinc.Helpers
{
    public static class SaveLoadDalFactory
    {
        readonly static IDataSaveLoadDal _localSaveLoadInstance;

        static SaveLoadDalFactory()
        {
            _localSaveLoadInstance = new PlayerPrefsDataSaveLoadDal();
        }
        
        public static IDataSaveLoadDal CreateInstance(SaveLoadType saveLoadType)
        {
            return _localSaveLoadInstance;
        }
    }
}