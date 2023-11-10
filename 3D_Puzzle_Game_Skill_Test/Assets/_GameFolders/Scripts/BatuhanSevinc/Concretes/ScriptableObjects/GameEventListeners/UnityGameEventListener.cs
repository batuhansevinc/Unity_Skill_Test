using BatuhanSevinc.Abstracts.GameEventListeners;
using UnityEngine;
using UnityEngine.Events;

namespace BatuhanSevinc.ScriptableObjects.GameEventListeners
{
    public class UnityGameEventListener : BaseGameEventListener
    {
        [SerializeField] UnityEvent _untiyEvent;
        [SerializeField] UnityEvent<object> _untiyEventObject;

       public override void Notify()
        {
            _untiyEvent?.Invoke();
        }

       public override void Notify(object value)
       {
           _untiyEventObject?.Invoke(value);
       }
    }
}