using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Puzzle/PipeCategory")]
public class PipeCategorySO : ScriptableObject
{
    public List<PipeTypeSO> pipeTypes;
    // You can add more properties if needed.
}