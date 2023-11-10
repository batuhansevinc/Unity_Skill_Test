using UnityEngine;

[CreateAssetMenu(fileName = "LevelReferences", menuName = "Custom/LevelReferences")]
public class LevelReferencesSO : ScriptableObject
{
    [SerializeField]
    private Transform _upTarget;

    [SerializeField]
    private Transform _downTarget;

    [SerializeField]
    private Transform _leftTarget;

    [SerializeField]
    private Transform _rightTarget;

    public Transform UpTarget => _upTarget;
    public Transform DownTarget => _downTarget;
    public Transform LeftTarget => _leftTarget;
    public Transform RightTarget => _rightTarget;
}