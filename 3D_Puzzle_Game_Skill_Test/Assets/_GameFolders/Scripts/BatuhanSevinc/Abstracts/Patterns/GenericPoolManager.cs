using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BatuhanSevinc.Abstracts.Patterns
{
    public abstract class GenericPoolManager<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] int _firstTimeSpawnCount = 10;
        [SerializeField] int _spawnCount = 2;
        [SerializeField] T _prefab;

        readonly Queue<T> _poolObjects = new Queue<T>();

        IEnumerator Start()
        {
            yield return InitializePoolAsync();
        }

        IEnumerator InitializePoolAsync()
        {
            Queue<T> enemyControllers = new Queue<T>();

            for (int j = 0; j < _firstTimeSpawnCount; j++)
            {
                var newObject = Instantiate(_prefab);
                newObject.gameObject.SetActive(false);
                newObject.transform.SetParent(this.transform);
                enemyControllers.Enqueue(newObject);
                yield return null;
            }
        }
        
        void GrowPool()
        {
            Queue<T> enemyControllers = new Queue<T>();

            for (int j = 0; j < _spawnCount; j++)
            {
                var newObject = Instantiate(_prefab);
                newObject.gameObject.SetActive(false);
                newObject.transform.SetParent(this.transform);
                enemyControllers.Enqueue(newObject);
            }
        }

        public void SetPool(T poolObject)
        {
            poolObject.gameObject.SetActive(false);
            poolObject.transform.parent = this.transform;
            _poolObjects.Enqueue(poolObject);
        }

        public T GetPool()
        {
            if (_poolObjects.Count == 0)
            {
                GrowPool();
            }
            
            var poolObject = _poolObjects.Dequeue();
            return poolObject;
        }
    }
}

