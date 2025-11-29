using BufoGames.Data;
using BufoGames.Enums;
using UnityEngine;

namespace BufoGames.Abstract.Initialize
{
    public interface IGroundObjectFactory
    {
        GameObject CreateObject(ObjectType type, PieceType selectedPipeType = PieceType.None);
    }
}
