using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using BufoGames.Data;

namespace BufoGames.Pieces
{
    public class DestinationController : PieceBase
    {
        [SerializeField] private Transform visualTransform;
        [SerializeField] private int currentRotation = 0;
        [SerializeField] private float rotationDuration = 0.25f;
        [SerializeField] private Ease rotationEase = Ease.OutBack;
        
        public UnityEvent OnRotationCompleted;
        public UnityEvent OnConnected;
        public UnityEvent OnDisconnected;
        
        private Tween _rotationTween;
        private bool _isRotating;
        private bool _wasConnected;
        
        public override PieceType PieceType => PieceType.Destination;
        public override int CurrentRotation => currentRotation;
        public bool IsRotating => _isRotating;
        
        public override void OnConnectionStateChanged(bool connected)
        {
            base.OnConnectionStateChanged(connected);
            
            if (connected && !_wasConnected)
            {
                OnConnected?.Invoke();
            }
            else if (!connected && _wasConnected)
            {
                OnDisconnected?.Invoke();
            }
            
            _wasConnected = connected;
        }
        
        public void SetInitialRotation(int rotation)
        {
            currentRotation = NormalizeRotation(rotation);
            ApplyRotationImmediate();
        }
        
        public void Rotate()
        {
            if (_isRotating && _rotationTween != null && _rotationTween.IsActive())
            {
                _rotationTween.Complete();
            }
            
            _isRotating = true;
            TriggerTileBounce();
            currentRotation = NormalizeRotation(currentRotation + 90);
            
            Transform target = visualTransform != null ? visualTransform : transform;
            Vector3 targetEuler = new Vector3(0, currentRotation, 0);
            
            _rotationTween = target
                .DOLocalRotate(targetEuler, rotationDuration, RotateMode.Fast)
                .SetEase(rotationEase)
                .OnComplete(OnRotationAnimationComplete);
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
        
        public void SetVisualTransform(Transform visual)
        {
            visualTransform = visual;
        }
        
        private void OnDestroy()
        {
            _rotationTween?.Kill();
        }
    }
}
