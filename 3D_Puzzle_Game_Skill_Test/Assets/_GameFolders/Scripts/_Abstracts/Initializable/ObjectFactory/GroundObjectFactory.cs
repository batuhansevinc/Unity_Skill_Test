using Assignment01.Enums;
using UnityEngine;

namespace Assignment01.Abstract.Initialize
{
    public class GroundObjectFactory : IInitializable, IGroundObjectFactory
    {
        GroundObjectSO _groundObjectData; 
        ObjectType _selectedObjectType; 
        PipeTypeSO _selectedPipeType;
        Transform _instantiateTransform;
        GameObject _currentObjectInstance;

        public GroundObjectFactory(GroundObjectSO data, Transform instantiateTrans)
        {
            _groundObjectData = data;
            _instantiateTransform = instantiateTrans;
        }

        public void SetObjectType(ObjectType type)
        {
            _selectedObjectType = type;
        }

        public void SetPipeType(PipeTypeSO pipeType)
        {
            _selectedPipeType = pipeType;
        }

        public void Initialize()
        {
            _currentObjectInstance = CreateObject(_selectedObjectType, _selectedPipeType);
        }
    
        public GameObject GetCurrentObjectInstance()
        {
            return _currentObjectInstance;
        }

        // CreateObject metodunu interface'in şartlarına uygun olarak güncelledik
        public GameObject CreateObject(ObjectType type, PipeTypeSO selectedPipeType = null)
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
                    if (selectedPipeType && selectedPipeType.pipePrefab)
                    {
                        newInstance = GameObject.Instantiate(selectedPipeType.pipePrefab, _instantiateTransform.position, _instantiateTransform.rotation, _instantiateTransform);
                    }
                    break;
            }
            _currentObjectInstance = newInstance;
            return newInstance;
        }

    }
}
