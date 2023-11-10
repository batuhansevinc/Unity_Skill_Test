using UnityEngine;

namespace Assignment01.Abstract.Rotate
{
    public interface IRotatable
    {
        void Rotate(GameObject target, float _duration);
    }
}