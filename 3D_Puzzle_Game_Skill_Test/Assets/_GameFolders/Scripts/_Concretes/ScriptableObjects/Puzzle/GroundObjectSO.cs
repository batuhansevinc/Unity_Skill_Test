using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Puzzle/GroundObject")]
public class GroundObjectSO : ScriptableObject
{
    public GameObject GasStationPrefab;
    public GameObject OilPumpPrefab;
    public List<PipeCategorySO> pipeCategories;
    // You can add more properties if needed.
}