using DG.Tweening;
using UnityEngine;

namespace Assignment01.Abstract.Rotate
{
    public class Rotator : IRotatable
    {
        private float _rotationDuration = 0.1f;
        private Tween _currentTween;  
        private Vector3 _targetRotation;  // Hedef rotasyon değerini tutan değişken.

        public Rotator() {}

        public void Rotate(GameObject target, float _duration)
        {
            if (target)
            {
                // Eğer mevcut bir tween devam ediyorsa, onu hemen sonlandır.
                if (_currentTween != null && _currentTween.IsActive() && _currentTween.IsPlaying())
                {
                    _currentTween.Kill();
                    target.transform.localEulerAngles = _targetRotation; 
                }

                Vector3 currentRotation = target.transform.localEulerAngles;
                _targetRotation = currentRotation + new Vector3(0, 90, 0);

                // Yeni bir rotasyon tween'i başlat.
                _currentTween = target.transform.DORotate(_targetRotation, _duration, RotateMode.FastBeyond360);
            }
        }
    }
}