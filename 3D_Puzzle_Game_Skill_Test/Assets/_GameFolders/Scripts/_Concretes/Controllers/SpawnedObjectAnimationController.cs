using UnityEngine;

public class SpawnedObjectAnimationController : MonoBehaviour
{
    Animator _animator;

    private void Awake()
    {
        if (_animator == null)
        {
            GetReference();
        }
    }

    void Start()
    {
        _animator.ResetTrigger("isWorking");
    }

    public void StartAnimation()
    {
        _animator.SetTrigger("isWorking");
    }

    void GetReference()
    {
        _animator = GetComponentInChildren<Animator>();
    }
}
