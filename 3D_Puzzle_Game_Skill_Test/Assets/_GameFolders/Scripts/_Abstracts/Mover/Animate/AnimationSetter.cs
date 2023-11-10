using DG.Tweening;
using UnityEngine;

namespace Assignment01.Abstract.Animate
{
    public class AnimationSetter : IAnimatable
    {
        Tween _currentTween;
        float _bounceDistance;
        float _duration;

        public AnimationSetter(float bounceDistance, float duration)
        {
            _bounceDistance = bounceDistance;
            _duration = duration;
        }

        public void Animate(GameObject target)
        {
            _currentTween.Kill();
            _currentTween = target.transform.DOLocalMoveY(-_bounceDistance, _duration).SetLoops(2, LoopType.Yoyo);
        }
    }
}