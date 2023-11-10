using System.Collections;
using System.Collections.Generic;
using BatuhanSevinc.ScriptableObjects;
using UnityEngine;

public class NewLevelControlle : MonoBehaviour
{
    SpawnedObjectController _sourceObject;
    GameObject _destinationObject;
    List<SpawnedObjectController> _allObjects = new List<SpawnedObjectController>();
    [SerializeField] GameEvent _levelCompletedEvent;
    [SerializeField] GameEvent _fireworksEvent;
    [SerializeField] GameEvent _startEndGameAnimationsEvent;

    private LayerMask layerMask;

    private void Start()
    {
        layerMask = ~LayerMask.GetMask("TILE");

        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.5f);
        _allObjects.AddRange(FindObjectsOfType<SpawnedObjectController>());
        _sourceObject = FindResourceObject();
        _destinationObject = GameObject.FindGameObjectWithTag("Destination");
        CheckConnectionsFromSource();
    }

    private SpawnedObjectController FindResourceObject()
    {
        foreach (var obj in _allObjects)
        {
            if (obj.IsResourceObject)
                return obj;
        }
        return null;
    }

    public void CheckConnectionsFromSource()
    {
        foreach (var obj in _allObjects)
        {
            obj.IsConnectedwithA = false;
        }

        Queue<SpawnedObjectController> queue = new Queue<SpawnedObjectController>();
        HashSet<SpawnedObjectController> visited = new HashSet<SpawnedObjectController>();
        
        foreach (var collider in _sourceObject.InteractingColliders)
        {
            var neighbour = collider.GetComponent<SpawnedObjectController>();
            if (neighbour && !visited.Contains(neighbour))
            {
                queue.Enqueue(neighbour);
                visited.Add(neighbour);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            current.IsConnectedwithA = true;

            foreach (var collider in current.InteractingColliders)
            {
                var neighbour = collider.GetComponent<SpawnedObjectController>();
                if (neighbour && !visited.Contains(neighbour) && neighbour != current)
                {
                    queue.Enqueue(neighbour);
                    visited.Add(neighbour);
                }
            }
        }
        
        bool allConnected = true;
        foreach (var obj in _allObjects)
        {
            if (!obj.IsConnectedwithA)
            {
                allConnected = false;
                break;
            }
        }

        if (allConnected)
        {
            Debug.Log("Level Completed");
            StartCoroutine(LevelCompleted());

        }

        IEnumerator LevelCompleted()
        {
            _startEndGameAnimationsEvent.InvokeEvents();
            yield return new WaitForSeconds(2f);
            _fireworksEvent.InvokeEvents();
            _levelCompletedEvent.InvokeEvents();
            Destroy(this.gameObject);
        }
    }
    
}
