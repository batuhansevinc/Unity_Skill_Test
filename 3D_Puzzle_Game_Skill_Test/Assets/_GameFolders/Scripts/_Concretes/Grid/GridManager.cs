using System.Collections.Generic;
using BufoGames.Pieces;

namespace BufoGames.Grid
{
    /// <summary>
    /// Manages grid-based piece lookup
    /// O(1) position-to-piece lookup using dictionary
    /// </summary>
    public class GridManager
    {
        private Dictionary<(int x, int z), PieceBase> _pieceMap;
        private int _gridSize;
        
        public int GridSize => _gridSize;
        
        public GridManager(int gridSize)
        {
            _gridSize = gridSize;
            _pieceMap = new Dictionary<(int, int), PieceBase>(gridSize * gridSize);
        }
        
        /// <summary>
        /// Build the piece map from a list of pieces
        /// Call this once after spawning all pieces
        /// </summary>
        public void BuildMap(List<PieceBase> pieces)
        {
            _pieceMap.Clear();
            foreach (var piece in pieces)
            {
                if (piece != null)
                {
                    _pieceMap[(piece.GridX, piece.GridZ)] = piece;
                }
            }
        }
        
        /// <summary>
        /// Register a single piece (for dynamic spawning)
        /// </summary>
        public void RegisterPiece(PieceBase piece)
        {
            if (piece != null)
            {
                _pieceMap[(piece.GridX, piece.GridZ)] = piece;
            }
        }
        
        /// <summary>
        /// Unregister a piece (for removal)
        /// </summary>
        public void UnregisterPiece(PieceBase piece)
        {
            if (piece != null)
            {
                _pieceMap.Remove((piece.GridX, piece.GridZ));
            }
        }
        
        /// <summary>
        /// Get piece at specific grid position - O(1)
        /// </summary>
        public PieceBase GetPieceAt(int x, int z)
        {
            _pieceMap.TryGetValue((x, z), out var piece);
            return piece;
        }
        
        /// <summary>
        /// Get neighbor piece in specified direction - O(1)
        /// </summary>
        public PieceBase GetNeighbor(PieceBase from, Direction direction)
        {
            DirectionHelper.GetOffset(direction, out int dx, out int dz);
            return GetPieceAt(from.GridX + dx, from.GridZ + dz);
        }
        
        /// <summary>
        /// Get neighbor piece by coordinates and direction - O(1)
        /// </summary>
        public PieceBase GetNeighbor(int x, int z, Direction direction)
        {
            DirectionHelper.GetOffset(direction, out int dx, out int dz);
            return GetPieceAt(x + dx, z + dz);
        }
        
        /// <summary>
        /// Check if position is within grid bounds
        /// </summary>
        public bool IsValidPosition(int x, int z)
        {
            return x >= 0 && x < _gridSize && z >= 0 && z < _gridSize;
        }
        
        /// <summary>
        /// Check if there's a piece at position
        /// </summary>
        public bool HasPieceAt(int x, int z)
        {
            return _pieceMap.ContainsKey((x, z));
        }
        
        /// <summary>
        /// Get all registered pieces
        /// </summary>
        public IEnumerable<PieceBase> GetAllPieces()
        {
            return _pieceMap.Values;
        }
        
        /// <summary>
        /// Get total piece count
        /// </summary>
        public int PieceCount => _pieceMap.Count;
        
        /// <summary>
        /// Clear all pieces
        /// </summary>
        public void Clear()
        {
            _pieceMap.Clear();
        }
    }
}
