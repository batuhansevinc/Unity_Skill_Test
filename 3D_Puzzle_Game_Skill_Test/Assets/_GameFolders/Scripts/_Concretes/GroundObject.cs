using System.Collections;
using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using Assignment01.Abstract.Animate;
using Assignment01.Abstract.Initialize;
using Assignment01.Abstract.Rotate;
using Assignment01.Enums;

namespace Assignment01.Controller
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
        [SerializeField] PipeTypeSO _selectedPipeType;

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

        private NewLevelControlle _levelController;
        private GameObject _sourceGameObject;

        public ObjectType SelectedObjectType
        {
            get => _selectedObjectType;
            set => _selectedObjectType = value;
        }
        public Direction GetFacingDirection()
        {
            float yRotation = instantiateTransform.eulerAngles.y;
            rotation = yRotation;

            float tolerance = 1f; // Tolerans değerini 1 derece olarak belirledik

            if (Mathf.Abs(yRotation - 0f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Up");
                return Direction.Up;
            }
            else if (Mathf.Abs(yRotation - 90f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Right");
                return Direction.Right;
            }
            else if (Mathf.Abs(yRotation - 270f) <= tolerance || Mathf.Abs(yRotation + 90f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Left");
                return Direction.Left;
            }
            else if (Mathf.Abs(yRotation - 180f) <= tolerance) 
            {
                Debug.Log(gameObject.name + " is facing Down");
                return Direction.Down;
            }
            else
            {
                Debug.Log(gameObject.name + " has an undefined direction");
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
            _levelController = FindObjectOfType<NewLevelControlle>();
            _sourceGameObject = GameObject.FindWithTag("Source");
            if (_selectedObjectType != ObjectType.None)
            {
                _initializer.SetObjectType(_selectedObjectType);
                _initializer.Initialize();
                _canAnimate = true;
            }

            if (_selectedObjectType == ObjectType.Pipe && _selectedPipeType)
            {
                _initializer.SetPipeType(_selectedPipeType);
                _initializer.Initialize();
                _canAnimate = true;
            }

            _currentObjectInstance = _initializer.GetCurrentObjectInstance();
            SetRandomRotationToInstantiateTransform();
        }
        
        void SetRandomRotationToInstantiateTransform()
        {
            // Rotasyon değerlerini bir dizi içerisinde saklayın
            float[] rotationValues = { 0, 90, -180, -90 };

            // Rastgele bir indeks seçin
            int randomIndex = Random.Range(0, rotationValues.Length);

            // Rastgele seçilen indekse karşılık gelen rotasyon değerini instantiateTransform'a atayın
            instantiateTransform.eulerAngles = new Vector3(0, rotationValues[randomIndex], 0);
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
            if (other.CompareTag("Pipe") || other.CompareTag("Source") || other.CompareTag("Destination"))
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

        IEnumerator CheckRotation()
        {
            yield return new WaitForSeconds(0.6f);
            GetFacingDirection();
            _levelController.CheckConnectionsFromSource();
        }
    }
}
