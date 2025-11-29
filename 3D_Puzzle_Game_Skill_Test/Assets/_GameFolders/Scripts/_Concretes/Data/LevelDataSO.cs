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
        [Range(2, 10)]
        public int gridSize = 4;
        
        [Header("Puzzle Pieces")]
        public List<PieceData> pieces = new List<PieceData>();
        
        [Header("Validation")]
        [HideInInspector] public bool isValidated = false;
        [HideInInspector] public string validationMessage = "";
        [HideInInspector] public int estimatedDifficulty = 1;
        
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
        
        public PieceData GetSource()
        {
            return pieces.FirstOrDefault(p => p.pieceType == PieceType.Source);
        }
        
        public PieceData GetDestination()
        {
            return pieces.FirstOrDefault(p => p.pieceType == PieceType.Destination);
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
    }
    
    [System.Serializable]
    public class PieceData
    {
        public int x;
        public int z;
        public PieceType pieceType;
        public int rotation; // 0, 90, 180, 270
        
        public PieceData(int x, int z, PieceType type, int rot = 0)
        {
            this.x = x;
            this.z = z;
            this.pieceType = type;
            this.rotation = rot;
        }
    }
    
}

