using UnityEngine;

public class ParentRemover : MonoBehaviour
{
    [SerializeField] GameObject _parentObject;

    void Start()
    {
        var clickableObject = GetComponentInParent<ClickableObject>();
        if (clickableObject != null)
        {
            _parentObject = clickableObject.transform.gameObject;
            transform.SetParent(_parentObject.transform);
            transform.rotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning($"ParentRemover: No ClickableObject found in parent hierarchy for {gameObject.name}");
        }
    }
}
