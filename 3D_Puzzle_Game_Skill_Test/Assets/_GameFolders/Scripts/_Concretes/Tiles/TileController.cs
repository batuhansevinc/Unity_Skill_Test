using UnityEngine;
using DG.Tweening;

namespace BufoGames.Tiles
{
    public class TileController : MonoBehaviour
    {
        [SerializeField] private float bounceHeight = 0.20f;
        [SerializeField] private float bounceDuration = 0.25f;
        [SerializeField] private Ease downEase = Ease.OutSine;
        [SerializeField] private Ease upEase = Ease.OutSine;
        
        private Tween _bounceTween;
        private Vector3 _originalPosition;
        private bool _originalPositionSet;

        private void Start()
        {
            if (!_originalPositionSet)
                _originalPosition = transform.position;
        }

        public void SetOriginalPosition(Vector3 position)
        {
            _originalPosition = position;
            _originalPositionSet = true;
        }

        public void PlayBounce()
        {
            _bounceTween?.Kill();
            transform.position = _originalPosition;
            
            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMoveY(_originalPosition.y - bounceHeight, bounceDuration / 2).SetEase(downEase));
            sequence.Append(transform.DOMoveY(_originalPosition.y, bounceDuration / 2).SetEase(upEase));
            _bounceTween = sequence;
        }

        private void OnDestroy()
        {
            _bounceTween?.Kill();
        }
    }
}
