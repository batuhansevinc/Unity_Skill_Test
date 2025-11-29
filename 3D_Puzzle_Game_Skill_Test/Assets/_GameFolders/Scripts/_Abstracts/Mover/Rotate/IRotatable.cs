using UnityEngine;

namespace BufoGames.Abstract.Rotate
{
    public interface IRotatable
    {
        void Rotate(GameObject target, float _duration);
    }
}