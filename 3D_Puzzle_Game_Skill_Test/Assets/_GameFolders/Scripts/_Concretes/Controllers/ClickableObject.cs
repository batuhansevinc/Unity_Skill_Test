using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    public UnityEvent OnClick;

    public void OnObjectClicked()
    {
        OnClick.Invoke();
    }
}