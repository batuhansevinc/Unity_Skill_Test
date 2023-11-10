using UnityEngine;
using System.Collections.Generic;

public class SpawnedObjectController : MonoBehaviour
{
    public bool IsResourceObject = false;
    private bool _isConnectedwithA = false;
    private List<Collider> interactingColliders = new List<Collider>();

    public bool IsConnectedwithA
    {
        get => _isConnectedwithA;
        set => _isConnectedwithA = value;
    }

    public List<Collider> InteractingColliders => interactingColliders;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TILE"))
            return;

        if (!interactingColliders.Contains(other))
        {
            interactingColliders.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TILE"))
            return;

        interactingColliders.Remove(other);
    }
}