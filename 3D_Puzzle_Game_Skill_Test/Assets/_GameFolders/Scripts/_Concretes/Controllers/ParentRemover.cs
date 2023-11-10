using UnityEngine;

public class ParentRemover : MonoBehaviour
{
    [SerializeField] GameObject _parentObject;

    void Start()
    {
        _parentObject = GetComponentInParent<ClickableObject>().transform.gameObject;
        transform.SetParent(_parentObject.transform);
        transform.rotation = new Quaternion(0f, 0f, 0f,0f);
    }
}
