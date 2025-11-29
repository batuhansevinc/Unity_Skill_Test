using BufoGames.Data;
using BufoGames.Enums;
using UnityEngine;

namespace BufoGames.Abstract.Initialize
{
    public interface IInitializable
    {
        void Initialize();
        void SetObjectType(ObjectType type);
        void SetPipeType(PieceType pipeType);
        GameObject GetCurrentObjectInstance();
    }
}

