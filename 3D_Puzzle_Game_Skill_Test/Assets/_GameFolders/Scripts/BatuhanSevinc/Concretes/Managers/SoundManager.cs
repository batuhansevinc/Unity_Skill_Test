using BatuhanSevinc.Abstracts.Patterns;
using UnityEngine;

public class SoundManager : SingletonMonoAndDontDestroy<SoundManager>
{

    [SerializeField] AudioSource _hitSound;
    [SerializeField] AudioSource _moveSound;
    private void Awake()
    {
        SetSingleton(this);
    }
    
    public void PlayHitSound()
    {
        _hitSound.Play();  
    }

    public void PlayMoveSound()
    {
        _moveSound.Play();
    }
    
}
