using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BufoGames.Data
{
    [CreateAssetMenu(fileName = "Level_", menuName = "Puzzle/Level Data")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Identification")]
        public int levelIndex = 1;
        
        [Header("Grid Configuration")]
        [Range(2, 12)]
        public int gridWidth = 4;   // X ekseni (sütun sayısı)
        [Range(2, 12)]
        public int gridHeight = 4;  // Z ekseni (satır sayısı)
        
        [Header("Puzzle Pieces")]
        public List<PieceData> pieces = new List<PieceData>();
        
        [Header("Validation")]
        [HideInInspector] public bool isValidated = false;
        [HideInInspector] public string validationMessage = "";
        [HideInInspector] public int estimatedDifficulty = 1;
        [HideInInspector] public int minimumMoves = -1;
        
        // Helper properties
        public int GridArea => gridWidth * gridHeight;
        
        // Helper methods
        public PieceData GetPieceAt(int x, int z)
        {
            return pieces.FirstOrDefault(p => p.x == x && p.z == z);
        }
        
        public void SetPieceAt(int x, int z, PieceType type, int rotation = 0)
        {
            RemovePieceAt(x, z);
            pieces.Add(new PieceData(x, z, type, rotation));
        }
        
        public void RemovePieceAt(int x, int z)
        {
            pieces.RemoveAll(p => p.x == x && p.z == z);
        }
        
        public bool HasPieceAt(int x, int z)
        {
            return GetPieceAt(x, z) != null;
        }
        
        public bool IsValidPosition(int x, int z)
        {
            return x >= 0 && x < gridWidth && z >= 0 && z < gridHeight;
        }
        
        public PieceData GetSource()
        {
            return pieces.FirstOrDefault(p => p.pieceType == PieceType.Source);
        }
        
        public List<PieceData> GetDestinations()
        {
            return pieces.Where(p => p.pieceType == PieceType.Destination).ToList();
        }
        
        public int GetDestinationCount()
        {
            return pieces.Count(p => p.pieceType == PieceType.Destination);
        }
        
        public void RotatePieceAt(int x, int z)
        {
            var piece = GetPieceAt(x, z);
            if (piece != null)
            {
                piece.rotation = (piece.rotation + 90) % 360;
            }
        }
        
        public int GetTotalPieceCount()
        {
            return pieces.Count;
        }
        
        public void ClearAllPieces()
        {
            pieces.Clear();
            isValidated = false;
            validationMessage = "";
        }
        
        /// <summary>
        /// Remove pieces that are outside the current grid bounds
        /// </summary>
        public void CleanupOutOfBoundsPieces()
        {
            pieces.RemoveAll(p => p.x >= gridWidth || p.z >= gridHeight);
        }
    }
    
    [System.Serializable]
    public class PieceData
    {
        public int x;
        public int z;
        public PieceType pieceType;
        public int rotation; // 0, 90, 180, 270
        public bool isDecoy; // True if this piece is a decoy (not part of the solution path)
        
        public PieceData(int x, int z, PieceType type, int rot = 0, bool isDecoy = false)
        {
            this.x = x;
            this.z = z;
            this.pieceType = type;
            this.rotation = rot;
            this.isDecoy = isDecoy;
        }
    }
}

