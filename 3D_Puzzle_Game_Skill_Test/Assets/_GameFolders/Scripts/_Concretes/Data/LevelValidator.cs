using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BufoGames.Data
{
    public class LevelValidator
    {
        private LevelDataSO levelData;
        private string validationMessage;
        private int estimatedDifficulty;
        
        public LevelValidator(LevelDataSO data)
        {
            levelData = data;
        }
        
        public bool Validate()
        {
            // 1. Check basic requirements
            if (!ValidateBasicRequirements())
                return false;
            
            // 2. Check if solvable
            if (!ValidateSolvability())
                return false;
            
            // 3. Calculate difficulty
            CalculateDifficulty();
            
            validationMessage = "✅ Level is solvable!";
            return true;
        }
        
        private bool ValidateBasicRequirements()
        {
            // Check source
            var source = levelData.GetSource();
            if (source == null)
            {
                validationMessage = "❌ No source found! Add a source piece.";
                return false;
            }
            
            // Check destination
            var destination = levelData.GetDestination();
            if (destination == null)
            {
                validationMessage = "❌ No destination found! Add a destination piece.";
                return false;
            }
            
            // Check minimum pipes
            int pipeCount = levelData.pieces.Count(p => 
                p.pieceType != PieceType.Source && 
                p.pieceType != PieceType.Destination);
            
            if (pipeCount < 1)
            {
                validationMessage = "❌ No pipes found! Add at least one pipe.";
                return false;
            }
            
            return true;
        }
        
        private bool ValidateSolvability()
        {
            // Pathfinding algorithm to check if source can reach destination
            var source = levelData.GetSource();
            var destination = levelData.GetDestination();
            
            // BFS to find path
            Queue<PieceData> queue = new Queue<PieceData>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
            
            queue.Enqueue(source);
            visited.Add(new Vector2Int(source.x, source.z));
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                
                // Check if reached destination
                if (current.x == destination.x && current.z == destination.z)
                {
                    return true;
                }
                
                // Get connections from current piece
                var connections = GetConnections(current);
                
                foreach (var nextPos in connections)
                {
                    if (visited.Contains(nextPos))
                        continue;
                    
                    var nextPiece = levelData.GetPieceAt(nextPos.x, nextPos.y);
                    if (nextPiece != null)
                    {
                        queue.Enqueue(nextPiece);
                        visited.Add(nextPos);
                    }
                }
            }
            
            validationMessage = "❌ Level not solvable! Source cannot reach destination with current pipe configuration.";
            return false;
        }
        
        private List<Vector2Int> GetConnections(PieceData piece)
        {
            List<Vector2Int> connections = new List<Vector2Int>();
            
            // Determine which directions this piece connects based on type and rotation
            bool[] directions = GetDirections(piece); // [Up, Right, Down, Left]
            
            // Up
            if (directions[0] && piece.z < levelData.gridSize - 1)
                connections.Add(new Vector2Int(piece.x, piece.z + 1));
            
            // Right
            if (directions[1] && piece.x < levelData.gridSize - 1)
                connections.Add(new Vector2Int(piece.x + 1, piece.z));
            
            // Down
            if (directions[2] && piece.z > 0)
                connections.Add(new Vector2Int(piece.x, piece.z - 1));
            
            // Left
            if (directions[3] && piece.x > 0)
                connections.Add(new Vector2Int(piece.x - 1, piece.z));
            
            return connections;
        }
        
        private bool[] GetDirections(PieceData piece)
        {
            // Returns [Up, Right, Down, Left]
            bool[] dirs = new bool[4];
            
            switch (piece.pieceType)
            {
                case PieceType.Source:
                case PieceType.Destination:
                case PieceType.CrossPipe:
                    dirs = new bool[] { true, true, true, true };
                    break;
                    
                case PieceType.StraightPipe:
                    if (piece.rotation == 0 || piece.rotation == 180)
                        dirs = new bool[] { true, false, true, false }; // Vertical
                    else
                        dirs = new bool[] { false, true, false, true }; // Horizontal
                    break;
                    
                case PieceType.CornerPipe:
                    switch (piece.rotation)
                    {
                        case 0: dirs = new bool[] { true, true, false, false }; break;   // Up-Right
                        case 90: dirs = new bool[] { false, true, true, false }; break;  // Right-Down
                        case 180: dirs = new bool[] { false, false, true, true }; break; // Down-Left
                        case 270: dirs = new bool[] { true, false, false, true }; break; // Left-Up
                    }
                    break;
                    
                case PieceType.TJunctionPipe:
                    switch (piece.rotation)
                    {
                        case 0: dirs = new bool[] { true, true, true, false }; break;  // Up-Right-Down
                        case 90: dirs = new bool[] { true, true, false, true }; break;  // Up-Right-Left
                        case 180: dirs = new bool[] { true, false, true, true }; break; // Up-Down-Left
                        case 270: dirs = new bool[] { false, true, true, true }; break; // Right-Down-Left
                    }
                    break;
            }
            
            return dirs;
        }
        
        private void CalculateDifficulty()
        {
            int pipeCount = levelData.GetTotalPieceCount() - 2; // Exclude source and destination
            int gridArea = levelData.gridSize * levelData.gridSize;
            float density = (float)pipeCount / gridArea;
            
            // Simple difficulty estimation
            if (pipeCount < 5) estimatedDifficulty = 1;
            else if (pipeCount < 10) estimatedDifficulty = 2;
            else if (pipeCount < 15) estimatedDifficulty = 3;
            else if (pipeCount < 20) estimatedDifficulty = 4;
            else estimatedDifficulty = 5;
            
            // Adjust for grid size
            if (levelData.gridSize >= 8) estimatedDifficulty++;
            
            estimatedDifficulty = Mathf.Clamp(estimatedDifficulty, 1, 5);
        }
        
        public string GetValidationMessage() => validationMessage;
        public int GetEstimatedDifficulty() => estimatedDifficulty;
    }
}

