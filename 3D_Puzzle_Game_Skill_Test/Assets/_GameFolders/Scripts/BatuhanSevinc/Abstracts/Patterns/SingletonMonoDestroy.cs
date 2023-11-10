using UnityEngine;

namespace BatuhanSevinc.Abstracts.Patterns
{
    /// <summary>
    /// Use this abstract class only for set singleton mono object and can destroy when scene close. When this class inherit another mono classes must be trigger 'SetSingleton' method inside Awake method. 
    /// </summary>
    /// <typeparam name="T">Generic type must be inherit from MonoBehaviour.</typeparam>
    public abstract class SingletonMonoDestroy<T> : SingletonMonoObject<T> where T : MonoBehaviour
    {
        protected override void SetSingleton(T instance)
        {
            Instance = instance;
        }
    }
}