using UnityEngine;

namespace Assignment01.Uis
{
    public class ReTryButton : BaseButtonWithGameEvents
    {
        [SerializeField] AudioSource _buttonClickSound;
        protected override void HandleOnButtonClicked()
        {
            _buttonClickSound.Play();
            _buttonEvent.InvokeEvents();
        }
    }
}