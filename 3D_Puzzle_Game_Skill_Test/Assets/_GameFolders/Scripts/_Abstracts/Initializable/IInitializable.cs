using Assignment01.Enums;
using Assignment01.Controller;
using UnityEngine;

namespace Assignment01.Abstract.Initialize
{
    public interface IInitializable
    {
        void Initialize();
        void SetObjectType(ObjectType type);
        void SetPipeType(PipeTypeSO pipeType);
        GameObject GetCurrentObjectInstance();

    }
}