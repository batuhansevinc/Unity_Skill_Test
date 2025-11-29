using System.Collections.Generic;
using BufoGames.Grid;
using BufoGames.Pieces;
using UnityEngine;

namespace BufoGames.Validators
{
    /// <summary>
    /// Grid-based connection validator using BFS algorithm
    /// No physics, no triggers - pure mathematical position-based validation
    /// Time Complexity: O(n) where n = number of pieces
    /// Space Complexity: O(n) for visited set and queue
    /// </summary>
    public class ConnectionValidator
    {
        private readonly GridManager _gridManager;
        
        // Pre-allocated collections to avoid GC
        private readonly HashSet<PieceBase> _visited;
        private readonly Queue<PieceBase> _queue;
        private readonly Direction[] _portBuffer = new Direction[4];
        
        // Statistics
        private int _lastConnectedCount;
        private int _lastTotalCount;
        
        public int LastConnectedCount => _lastConnectedCount;
        public int LastTotalCount => _lastTotalCount;
        
        public ConnectionValidator(GridManager gridManager, int expectedPieceCount = 16)
        {
            _gridManager = gridManager;
            _visited = new HashSet<PieceBase>(expectedPieceCount);
            _queue = new Queue<PieceBase>(expectedPieceCount);
        }
        
        /// <summary>
        /// Validate all connections starting from source using BFS
        /// Returns true if all pieces are connected to source
        /// </summary>
        public bool ValidateAllConnections(SourceController source, List<PieceBase> allPieces)
        {
            if (source == null || allPieces == null || allPieces.Count == 0)
            {
                Debug.LogWarning("ConnectionValidator: Invalid source or pieces list");
                return false;
            }
            
            // Reset state
            ResetAllConnections(allPieces);
            _visited.Clear();
            _queue.Clear();
            
            // Start BFS from source
            source.IsConnected = true;
            _visited.Add(source);
            _queue.Enqueue(source);
            
            // BFS traversal
            while (_queue.Count > 0)
            {
                PieceBase current = _queue.Dequeue();
                ProcessNeighbors(current);
            }
            
            // Count results
            return CountAndVerify(allPieces);
        }
        
        /// <summary>
        /// Process all neighbors of a piece
        /// </summary>
        private void ProcessNeighbors(PieceBase current)
        {
            // Get current piece's open ports
            int portCount = current.GetOpenPorts(_portBuffer);
            
            for (int i = 0; i < portCount; i++)
            {
                Direction port = _portBuffer[i];
                
                // Get neighbor in that direction
                PieceBase neighbor = _gridManager.GetNeighbor(current, port);
                
                if (neighbor == null || _visited.Contains(neighbor))
                    continue;
                
                // Check if neighbor has a port facing back to current
                Direction oppositePort = DirectionHelper.GetOpposite(port);
                
                if (neighbor.HasPort(oppositePort))
                {
                    // Connection valid! Mark and enqueue
                    neighbor.IsConnected = true;
                    _visited.Add(neighbor);
                    _queue.Enqueue(neighbor);
                }
            }
        }
        
        /// <summary>
        /// Reset all pieces to disconnected state
        /// </summary>
        private void ResetAllConnections(List<PieceBase> pieces)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != null)
                {
                    pieces[i].IsConnected = false;
                }
            }
        }
        
        /// <summary>
        /// Count connected pieces and verify all are connected
        /// </summary>
        private bool CountAndVerify(List<PieceBase> pieces)
        {
            _lastConnectedCount = 0;
            _lastTotalCount = 0;
            
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != null)
                {
                    _lastTotalCount++;
                    if (pieces[i].IsConnected)
                    {
                        _lastConnectedCount++;
                    }
                }
            }
            
            bool allConnected = _lastConnectedCount == _lastTotalCount && _lastTotalCount > 0;
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"ConnectionValidator: {_lastConnectedCount}/{_lastTotalCount} pieces connected. Complete: {allConnected}");
            #endif
            
            return allConnected;
        }
        
        /// <summary>
        /// Quick check without full BFS - just checks if destination is connected
        /// Useful for optimizing repeated checks
        /// </summary>
        public bool IsDestinationConnected(DestinationController destination)
        {
            return destination != null && destination.IsConnected;
        }
        
        /// <summary>
        /// Debug method to get connection info for a specific piece
        /// </summary>
        public string GetPieceConnectionInfo(PieceBase piece)
        {
            if (piece == null) return "null";
            
            int portCount = piece.GetOpenPorts(_portBuffer);
            string ports = "";
            for (int i = 0; i < portCount; i++)
            {
                ports += _portBuffer[i].ToString() + " ";
            }
            
            return $"Piece({piece.GridX},{piece.GridZ}) Type:{piece.PieceType} Rot:{piece.CurrentRotation} Ports:[{ports}] Connected:{piece.IsConnected}";
        }
    }
}
