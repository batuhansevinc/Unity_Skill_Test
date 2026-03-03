using System.Collections.Generic;
using BufoGames.Grid;
using BufoGames.Pieces;
using UnityEngine;

namespace BufoGames.Validators
{
    public class ConnectionValidator
    {
        private readonly GridManager _gridManager;
        
        private readonly HashSet<PieceBase> _visited;
        private readonly Queue<PieceBase> _queue;
        private readonly Direction[] _portBuffer = new Direction[4];
        
        private int _lastConnectedCount;
        private int _lastTotalCount;
        private int _lastConnectedDestinations;
        private int _lastTotalDestinations;
        
        public int LastConnectedCount => _lastConnectedCount;
        public int LastTotalCount => _lastTotalCount;
        public int LastConnectedDestinations => _lastConnectedDestinations;
        public int LastTotalDestinations => _lastTotalDestinations;
        
        public ConnectionValidator(GridManager gridManager, int expectedPieceCount = 16)
        {
            _gridManager = gridManager;
            _visited = new HashSet<PieceBase>(expectedPieceCount);
            _queue = new Queue<PieceBase>(expectedPieceCount);
        }
        
        public bool ValidateAllConnections(SourceController source, List<PieceBase> allPieces)
        {
            if (source == null || allPieces == null || allPieces.Count == 0)
                return false;
            
            ResetAllConnections(allPieces);
            _visited.Clear();
            _queue.Clear();
            
            source.IsConnected = true;
            _visited.Add(source);
            _queue.Enqueue(source);
            
            while (_queue.Count > 0)
            {
                PieceBase current = _queue.Dequeue();
                ProcessNeighbors(current);
            }
            
            return CountAndVerify(allPieces);
        }
        
        private void ProcessNeighbors(PieceBase current)
        {
            int portCount = current.GetOpenPorts(_portBuffer);
            
            for (int i = 0; i < portCount; i++)
            {
                Direction port = _portBuffer[i];
                PieceBase neighbor = _gridManager.GetNeighbor(current, port);
                
                if (neighbor == null || _visited.Contains(neighbor))
                    continue;
                
                Direction oppositePort = DirectionHelper.GetOpposite(port);
                
                if (neighbor.HasPort(oppositePort))
                {
                    neighbor.IsConnected = true;
                    _visited.Add(neighbor);
                    _queue.Enqueue(neighbor);
                }
            }
        }
        
        private void ResetAllConnections(List<PieceBase> pieces)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != null)
                    pieces[i].IsConnected = false;
            }
        }
        
        private bool CountAndVerify(List<PieceBase> pieces)
        {
            _lastConnectedCount = 0;
            _lastTotalCount = 0;
            _lastConnectedDestinations = 0;
            _lastTotalDestinations = 0;
            
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null) continue;
                
                _lastTotalCount++;
                
                if (pieces[i].IsConnected)
                    _lastConnectedCount++;
                
                if (pieces[i] is DestinationController)
                {
                    _lastTotalDestinations++;
                    if (pieces[i].IsConnected)
                        _lastConnectedDestinations++;
                }
            }
            
            bool allDestinationsConnected = _lastConnectedDestinations == _lastTotalDestinations 
                                            && _lastTotalDestinations > 0;
            
            return allDestinationsConnected;
        }
        
        public bool AreAllDestinationsConnected()
        {
            return _lastConnectedDestinations == _lastTotalDestinations && _lastTotalDestinations > 0;
        }
        
        public bool IsDestinationConnected(DestinationController destination)
        {
            return destination != null && destination.IsConnected;
        }
        
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
