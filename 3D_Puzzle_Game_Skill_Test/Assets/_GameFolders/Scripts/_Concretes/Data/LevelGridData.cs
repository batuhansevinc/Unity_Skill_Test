using UnityEngine;
using BufoGames.Constants;

namespace BufoGames.Data
{
    /// <summary>
    /// Encapsulates all grid-related data and calculations for a level
    /// Supports asymmetric grids (e.g., 3x5, 4x6)
    /// </summary>
    [System.Serializable]
    public class LevelGridData
    {
        [SerializeField, Range(1, 12)] 
        private int gridWidth = 4;   // X ekseni (sütun sayısı)
        
        [SerializeField, Range(1, 12)] 
        private int gridHeight = 4;  // Z ekseni (satır sayısı)
        
        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public int GridArea => gridWidth * gridHeight;
        public float XInterval => LevelConstants.X_INTERVAL;
        public float ZInterval => LevelConstants.Z_INTERVAL;
        
        public LevelGridData(int width, int height)
        {
            gridWidth = Mathf.Clamp(width, 1, 12);
            gridHeight = Mathf.Clamp(height, 1, 12);
        }
        
        /// <summary>
        /// Calculate position for a cell at given grid coordinates
        /// </summary>
        public Vector3 GetCellPosition(int x, int z)
        {
            return new Vector3(x * XInterval, 0, z * ZInterval);
        }
        
        /// <summary>
        /// Check if position is within grid bounds
        /// </summary>
        public bool IsValidPosition(int x, int z)
        {
            return x >= 0 && x < gridWidth && z >= 0 && z < gridHeight;
        }
        
        /// <summary>
        /// Calculate maximum X coordinate based on grid width
        /// </summary>
        public float GetMaxX()
        {
            return (gridWidth - 1) * XInterval;
        }
        
        /// <summary>
        /// Calculate maximum Z coordinate based on grid height
        /// </summary>
        public float GetMaxZ()
        {
            return (gridHeight - 1) * ZInterval;
        }
        
        /// <summary>
        /// Get center position of the grid
        /// </summary>
        public Vector3 GetCenterPosition()
        {
            return new Vector3(GetMaxX() / 2f, 0, GetMaxZ() / 2f);
        }
        
        /// <summary>
        /// Get camera target position for upward view
        /// </summary>
        public Vector3 GetUpTargetPosition()
        {
            return new Vector3(GetMaxX() / 2f, 0, GetMaxZ() + ZInterval);
        }
        
        /// <summary>
        /// Get camera target position for downward view
        /// </summary>
        public Vector3 GetDownTargetPosition()
        {
            return new Vector3(GetMaxX() / 2f, 0, -ZInterval);
        }
        
        /// <summary>
        /// Get camera target position for left view
        /// </summary>
        public Vector3 GetLeftTargetPosition()
        {
            return new Vector3(-XInterval, 0, GetMaxZ() / 2f);
        }
        
        /// <summary>
        /// Get camera target position for right view
        /// </summary>
        public Vector3 GetRightTargetPosition()
        {
            return new Vector3(GetMaxX() + XInterval, 0, GetMaxZ() / 2f);
        }
    }
}

