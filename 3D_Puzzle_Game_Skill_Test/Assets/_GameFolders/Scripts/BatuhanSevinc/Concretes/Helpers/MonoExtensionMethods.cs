using UnityEngine;

namespace BatuhanSevinc.Helpers
{
    public static class MonoExtensionMethods
    {
        public static void GetReference<T>(this MonoBehaviour mono, ref T value) where T : Object
        {
            if (value == null)
            {
                value = mono.GetComponentInChildren<T>();
            }
        }
    }
}