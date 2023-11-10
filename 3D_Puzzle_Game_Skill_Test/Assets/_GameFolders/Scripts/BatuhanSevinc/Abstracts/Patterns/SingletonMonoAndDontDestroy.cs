using UnityEngine;

namespace BatuhanSevinc.Abstracts.Patterns
{
    /// <summary>
    /// Use this abstract class only for set singleton mono object and can't destroy when scene close. When this class inherit another mono classes must be trigger 'SetSingleton' method inside Awake method. 
    /// </summary>
    /// <typeparam name="T">Generic type must be inherit from MonoBehaviour.</typeparam>
    public abstract class SingletonMonoAndDontDestroy<T> : SingletonMonoObject<T> where T : MonoBehaviour
    {
        protected override void SetSingleton(T instance)
        {
            if (Instance == null)
            {
                Instance = instance;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}