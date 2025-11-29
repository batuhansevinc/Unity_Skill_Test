using UnityEngine;
using BufoGames.Data;
using BufoGames.Grid;
using BufoGames.Tiles;

namespace BufoGames.Pieces
{
    public abstract class PieceBase : MonoBehaviour
    {
        [SerializeField] protected int gridX;
        [SerializeField] protected int gridZ;
        [SerializeField] protected bool isConnected;
        
        protected TileController _tileController;
        
        public int GridX => gridX;
        public int GridZ => gridZ;
        public bool IsConnected 
        { 
            get => isConnected; 
            set => isConnected = value; 
        }
        
        public abstract PieceType PieceType { get; }
        public abstract int CurrentRotation { get; }
        
        public void SetGridPosition(int x, int z)
        {
            gridX = x;
            gridZ = z;
        }
        
        public void SetTileController(TileController tile)
        {
            _tileController = tile;
        }
        
        protected void TriggerTileBounce()
        {
            _tileController?.PlayBounce();
        }
        
        public bool HasPort(Direction direction)
        {
            return PipePortData.HasPort(PieceType, CurrentRotation, direction);
        }
        
        public int GetOpenPorts(Direction[] result)
        {
            return PipePortData.GetRotatedPorts(PieceType, CurrentRotation, result);
        }
        
        public int PortCount => PipePortData.GetPortCount(PieceType);
        
        public bool CanConnectTo(PieceBase other, Direction directionToOther)
        {
            if (other == null) return false;
            if (!HasPort(directionToOther)) return false;
            Direction oppositeDir = DirectionHelper.GetOpposite(directionToOther);
            if (!other.HasPort(oppositeDir)) return false;
            return true;
        }
        
        public virtual void OnConnectionStateChanged(bool connected)
        {
            isConnected = connected;
        }
    }
}
