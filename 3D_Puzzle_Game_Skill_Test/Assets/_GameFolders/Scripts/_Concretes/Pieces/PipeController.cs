using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using BufoGames.Data;

namespace BufoGames.Pieces
{
    public class PipeController : PieceBase
    {
        [SerializeField] private PieceType pieceType = PieceType.StraightPipe;
        [SerializeField] private int currentRotation = 0;
        [SerializeField] private Transform visualTransform;
        [SerializeField] private float rotationDuration = 0.25f;
        [SerializeField] private Ease rotationEase = Ease.OutBack;
        [SerializeField] private bool isStatic = false;
        
        public UnityEvent OnRotationStarted;
        public UnityEvent OnRotationCompleted;
        
        private Tween _rotationTween;
        private bool _isRotating;
        
        public override PieceType PieceType => pieceType;
        public override int CurrentRotation => currentRotation;
        public bool IsRotating => _isRotating;
        public bool IsStatic => isStatic;
        
        public void SetPieceType(PieceType type)
        {
            pieceType = type;
            // Auto-set static flag based on type
            isStatic = type.IsStatic();
        }
        
        public void SetStatic(bool value)
        {
            isStatic = value;
        }
        
        public void SetInitialRotation(int rotation)
        {
            currentRotation = NormalizeRotation(rotation);
            ApplyRotationImmediate();
        }
        
        public void Rotate()
        {
            // Static pipes cannot rotate
            if (isStatic) return;
            
            if (_isRotating && _rotationTween != null && _rotationTween.IsActive())
            {
                _rotationTween.Complete();
            }
            
            _isRotating = true;
            OnRotationStarted?.Invoke();
            TriggerTileBounce();
            
            currentRotation = NormalizeRotation(currentRotation + 90);
            
            Transform target = visualTransform != null ? visualTransform : transform;
            Vector3 targetEuler = new Vector3(0, currentRotation, 0);
            
            _rotationTween = target
                .DOLocalRotate(targetEuler, rotationDuration, RotateMode.Fast)
                .SetEase(rotationEase)
                .OnComplete(OnRotationAnimationComplete);
        }
        
        /// <summary>
        /// Check if this pipe can be rotated by the player
        /// </summary>
        public bool CanRotate()
        {
            return !isStatic && !_isRotating;
        }
        
        private void OnRotationAnimationComplete()
        {
            _isRotating = false;
            _rotationTween = null;
            OnRotationCompleted?.Invoke();
        }
        
        private void ApplyRotationImmediate()
        {
            Transform target = visualTransform != null ? visualTransform : transform;
            target.localEulerAngles = new Vector3(0, currentRotation, 0);
        }
        
        private int NormalizeRotation(int rotation)
        {
            rotation = rotation % 360;
            if (rotation < 0) rotation += 360;
            return rotation;
        }
        
        private void OnDestroy()
        {
            _rotationTween?.Kill();
        }
    }
}
