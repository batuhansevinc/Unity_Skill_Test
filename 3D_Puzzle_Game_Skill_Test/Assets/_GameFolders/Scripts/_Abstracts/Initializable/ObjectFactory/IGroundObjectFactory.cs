using Assignment01.Enums;
using Assignment01.Controller;
using UnityEngine;

namespace Assignment01.Abstract.Initialize
{
    public interface IGroundObjectFactory
    {
        GameObject CreateObject(ObjectType type, PipeTypeSO selectedPipeType = null);
    }
}
