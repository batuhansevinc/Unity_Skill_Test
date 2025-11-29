using BufoGames.Data;
using BufoGames.Enums;
using UnityEngine;

namespace BufoGames.Abstract.Initialize
{
    public class GroundObjectFactory : IInitializable, IGroundObjectFactory
    {
        GroundObjectSO _groundObjectData; 
        ObjectType _selectedObjectType; 
        PieceType _selectedPipeType; // Changed from PipeTypeSO to PieceType enum
        Transform _instantiateTransform;
        GameObject _currentObjectInstance;
        ThemeDataSO _theme; // Theme for getting pipe prefabs

        public GroundObjectFactory(GroundObjectSO data, Transform instantiateTrans)
        {
            _groundObjectData = data;
            _instantiateTransform = instantiateTrans;
            _selectedPipeType = PieceType.None;
        }

        public void SetObjectType(ObjectType type)
        {
            _selectedObjectType = type;
        }

        public void SetPipeType(PieceType pipeType)
        {
            _selectedPipeType = pipeType;
        }
        
        public void SetTheme(ThemeDataSO theme)
        {
            _theme = theme;
        }

        public void Initialize()
        {
            _currentObjectInstance = CreateObject(_selectedObjectType, _selectedPipeType);
        }
    
        public GameObject GetCurrentObjectInstance()
        {
            return _currentObjectInstance;
        }

        /// <summary>
        /// Create object based on type and pipe type
        /// </summary>
        public GameObject CreateObject(ObjectType type, PieceType selectedPipeType = PieceType.None)
        {
            if (_currentObjectInstance)
            {
                GameObject.Destroy(_currentObjectInstance);
            }

            GameObject newInstance = null;
            switch (type)
            {
                case ObjectType.B:
                    if (_groundObjectData.GasStationPrefab)
                    {
                        newInstance = GameObject.Instantiate(_groundObjectData.GasStationPrefab, _instantiateTransform.position, _instantiateTransform.rotation, _instantiateTransform);
                    }
                    break;

                case ObjectType.A:
                    if (_groundObjectData.OilPumpPrefab)
                    {
                        newInstance = GameObject.Instantiate(_groundObjectData.OilPumpPrefab, _instantiateTransform.position, _instantiateTransform.rotation, _instantiateTransform);
                    }
                    break;

                case ObjectType.Pipe:
                    newInstance = CreatePipeFromTheme(selectedPipeType);
                    break;
            }
            _currentObjectInstance = newInstance;
            return newInstance;
        }
        
        /// <summary>
        /// Create pipe prefab from theme based on pipe type enum
        /// </summary>
        private GameObject CreatePipeFromTheme(PieceType pipeType)
        {
            if (_theme == null)
            {
                Debug.LogError("GroundObjectFactory: Theme is null! Cannot create pipe.");
                return null;
            }
            
            GameObject pipePrefab = _theme.GetPipePrefab(pipeType);
            
            if (pipePrefab == null)
            {
                Debug.LogError($"GroundObjectFactory: No prefab found for pipe type {pipeType} in theme!");
                return null;
            }
            
            return GameObject.Instantiate(pipePrefab, _instantiateTransform.position, _instantiateTransform.rotation, _instantiateTransform);
        }
    }
}
