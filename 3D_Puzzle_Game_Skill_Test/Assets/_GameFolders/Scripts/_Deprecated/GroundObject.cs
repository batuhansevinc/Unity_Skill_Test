using System.Collections;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using BufoGames.Abstract.Animate;
using BufoGames.Abstract.Initialize;
using BufoGames.Abstract.Rotate;
using BufoGames.Constants;
using BufoGames.Enums;

namespace BufoGames.Controller
{
    public class GroundObject : MonoBehaviour
    {
        public enum Direction
        {
            None,
            Up,
            Down,
            Left,
            Right
        }
        [SerializeField] GroundObjectSO _groundObjectData;
        [SerializeField] ObjectType _selectedObjectType = ObjectType.None;
        
        // Pipe type is now set at runtime, not from inspector
        private Data.PieceType _pipeType = Data.PieceType.None;
        
        // Theme for getting pipe prefabs (set at runtime by PieceSpawner)
        private Data.ThemeDataSO _theme;

        [Header("Duration")] 
        [SerializeField] float _rotationDuration = 0.5f;

        [Header("References")]
        public Transform instantiateTransform;

        private bool _canAnimate;
        private GameObject _currentObjectInstance;
        private IInitializable _initializer;
        private IRotatable _rotator;
        private IAnimatable _animator;
        private Tween _currentTween;
        private Vector3 _originalPosition;
        [SerializeField] float rotation;

        private List<GameObject> connectedObjects = new List<GameObject>();

        private LevelController _levelController;
        private GameObject _sourceGameObject;

        public ObjectType SelectedObjectType
        {
            get => _selectedObjectType;
            set => _selectedObjectType = value;
        }
        
        public Data.PieceType PipeType
        {
            get => _pipeType;
            set => _pipeType = value;
        }
        
        public void SetTheme(Data.ThemeDataSO theme)
        {
            _theme = theme;
            if (_initializer != null && theme != null)
            {
                (_initializer as GroundObjectFactory)?.SetTheme(theme);
            }
        }
        
        public Direction GetFacingDirection()
        {
            float yRotation = instantiateTransform.eulerAngles.y;
            
            // Normalize rotation to 0-360 range
            yRotation = yRotation % 360f;
            if (yRotation < 0) yRotation += 360f;
            
            rotation = yRotation;

            float tolerance = 5f; // Increased tolerance for better detection

            if (yRotation <= tolerance || yRotation >= (360f - tolerance)) 
            {
                Debug.Log(gameObject.name + " is facing Up (rotation: " + yRotation + ")");
                return Direction.Up;
            }
            else if (Mathf.Abs(yRotation - 90f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Right (rotation: " + yRotation + ")");
                return Direction.Right;
            }
            else if (Mathf.Abs(yRotation - 180f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Down (rotation: " + yRotation + ")");
                return Direction.Down;
            }
            else if (Mathf.Abs(yRotation - 270f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Left (rotation: " + yRotation + ")");
                return Direction.Left;
            }
            else
            {
                Debug.Log(gameObject.name + " has an undefined direction (rotation: " + yRotation + ")");
                return Direction.None;
            }
        }

        void Awake()
        {
            _initializer = new GroundObjectFactory(_groundObjectData, instantiateTransform);
            _rotator = new Rotator();
            _animator = new AnimationSetter(0.15f, 0.15f);
            _originalPosition = transform.localPosition;
            _canAnimate = false;
        }

        void Start()
        {
            _levelController = FindObjectOfType<LevelController>();
            _sourceGameObject = GameObject.FindWithTag("Source");
            
            // Set theme to factory if available
            if (_theme != null && _initializer is GroundObjectFactory factory)
            {
                factory.SetTheme(_theme);
            }
            
            if (_selectedObjectType != ObjectType.None)
            {
                _initializer.SetObjectType(_selectedObjectType);
                
                // If it's a pipe, set pipe type
                if (_selectedObjectType == ObjectType.Pipe && _pipeType != Data.PieceType.None)
                {
                    (_initializer as GroundObjectFactory)?.SetPipeType(_pipeType);
                }
                
                _initializer.Initialize();
                _canAnimate = true;
            }

            _currentObjectInstance = _initializer.GetCurrentObjectInstance();
            // Rotation is now set by LevelGenerator based on LevelDataSO, not random
        }

        void Animate()
        {
            transform.localPosition = _originalPosition;
            _animator.Animate(gameObject);
        }

        public void RotateObject()
        {
            _rotator.Rotate(instantiateTransform.gameObject, _rotationDuration);
            if (_canAnimate)
            {
                Animate();
            }

            StartCoroutine(CheckRotation());
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(LevelConstants.PIPE_TAG) || 
                other.CompareTag(LevelConstants.SOURCE_TAG) || 
                other.CompareTag(LevelConstants.DESTINATION_TAG))
            {
                connectedObjects.Add(other.gameObject);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (connectedObjects.Contains(other.gameObject))
            {
                connectedObjects.Remove(other.gameObject);
            }
        }

        public List<GameObject> GetConnectedObjects()
        {
            return connectedObjects;
        }
        
        /// <summary>
        /// Get actually connected objects based on current rotation direction
        /// Only returns objects that are in the direction the pipe is facing
        /// </summary>
        public List<GameObject> GetValidConnectedObjects()
        {
            List<GameObject> validConnections = new List<GameObject>();
            Direction currentDirection = GetFacingDirection();
            
            // If no specific direction, return all (for cross pipes)
            if (currentDirection == Direction.None)
            {
                return connectedObjects;
            }
            
            foreach (var obj in connectedObjects)
            {
                if (obj == null) continue;
                
                // Calculate relative position
                Vector3 relativePos = obj.transform.position - transform.position;
                Direction objDirection = GetDirectionFromVector(relativePos);
                
                // Check if object is in the direction we're facing
                if (IsValidConnection(currentDirection, objDirection))
                {
                    validConnections.Add(obj);
                }
            }
            
            return validConnections;
        }
        
        /// <summary>
        /// Determine direction based on relative position vector
        /// </summary>
        private Direction GetDirectionFromVector(Vector3 relativePos)
        {
            float absX = Mathf.Abs(relativePos.x);
            float absZ = Mathf.Abs(relativePos.z);
            
            if (absX > absZ)
            {
                return relativePos.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return relativePos.z > 0 ? Direction.Up : Direction.Down;
            }
        }
        
        /// <summary>
        /// Check if the connection is valid based on pipe rotation
        /// </summary>
        private bool IsValidConnection(Direction pipeDirection, Direction objectDirection)
        {
            // For straight pipes, check both ends
            if (_pipeType == Data.PieceType.StraightPipe)
            {
                return (pipeDirection == Direction.Up && objectDirection == Direction.Up) ||
                       (pipeDirection == Direction.Up && objectDirection == Direction.Down) ||
                       (pipeDirection == Direction.Down && objectDirection == Direction.Down) ||
                       (pipeDirection == Direction.Down && objectDirection == Direction.Up) ||
                       (pipeDirection == Direction.Left && objectDirection == Direction.Left) ||
                       (pipeDirection == Direction.Left && objectDirection == Direction.Right) ||
                       (pipeDirection == Direction.Right && objectDirection == Direction.Right) ||
                       (pipeDirection == Direction.Right && objectDirection == Direction.Left);
            }
            
            // For corner pipes, check adjacent directions
            if (_pipeType == Data.PieceType.CornerPipe)
            {
                // This needs more sophisticated logic based on actual pipe shape
                return true; // Simplified for now
            }
            
            // For T-junction and Cross, all directions valid
            return true;
        }

        IEnumerator CheckRotation()
        {
            yield return new WaitForSeconds(0.6f);
            GetFacingDirection();
            _levelController.CheckConnectionsFromSource();
        }
    }
}
