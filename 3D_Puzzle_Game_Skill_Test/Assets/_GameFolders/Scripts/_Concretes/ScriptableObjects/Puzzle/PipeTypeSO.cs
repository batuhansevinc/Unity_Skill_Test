using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle/PipeType")]
public class PipeTypeSO : ScriptableObject
{
    public string pipeName; // Example: T, L, U
    public GameObject pipePrefab;
    // You can add more properties if needed.
}