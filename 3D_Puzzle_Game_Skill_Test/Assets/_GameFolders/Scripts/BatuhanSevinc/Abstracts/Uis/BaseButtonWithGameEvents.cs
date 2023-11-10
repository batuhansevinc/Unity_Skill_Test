using BatuhanSevinc.ScriptableObjects;
using BatuhanSevinc.Uis;
using UnityEngine;

namespace Assignment01.Uis
{
    public class BaseButtonWithGameEvents : BaseButton
    {
        [SerializeField] protected GameEvent _buttonEvent;

        protected override void HandleOnButtonClicked()
        {
            _buttonEvent.InvokeEvents();
        }
    }
}