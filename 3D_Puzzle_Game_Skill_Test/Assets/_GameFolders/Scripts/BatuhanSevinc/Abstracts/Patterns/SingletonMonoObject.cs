using UnityEngine;

namespace BatuhanSevinc.Abstracts.Patterns
{
    /// <summary>
    /// Do not use this class because this call purpose is inheritance other singleton classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMonoObject<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; protected set; }

        protected abstract void SetSingleton(T instance);
    }
}