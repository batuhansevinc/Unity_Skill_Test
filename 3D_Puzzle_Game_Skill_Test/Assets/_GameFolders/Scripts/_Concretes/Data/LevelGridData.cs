using UnityEngine;
using BufoGames.Constants;

namespace BufoGames.Data
{
    /// <summary>
    /// Encapsulates all grid-related data and calculations for a level
    /// </summary>
    [System.Serializable]
    public class LevelGridData
    {
        [SerializeField, Range(1, 10)] 
        private int gridSize = 4;
        
        public int GridSize => gridSize;
        public float XInterval => LevelConstants.X_INTERVAL;
        public float ZInterval => LevelConstants.Z_INTERVAL;
        
        public LevelGridData(int size)
        {
            gridSize = Mathf.Clamp(size, 1, 10);
        }
        
        /// <summary>
        /// Calculate position for a cell at given grid coordinates
        /// </summary>
        public Vector3 GetCellPosition(int x, int z)
        {
            return new Vector3(x * XInterval, 0, z * ZInterval);
        }
        
        /// <summary>
        /// Calculate maximum X coordinate based on grid size
        /// </summary>
        public float GetMaxX()
        {
            return (gridSize - 1) * XInterval;
        }
        
        /// <summary>
        /// Calculate maximum Z coordinate based on grid size
        /// </summary>
        public float GetMaxZ()
        {
            return (gridSize - 1) * ZInterval;
        }
        
        /// <summary>
        /// Get camera target position for upward view
        /// </summary>
        public Vector3 GetUpTargetPosition()
        {
            float maxX = GetMaxX();
            float maxZ = GetMaxZ();
            return new Vector3(maxX / 2f, 0, maxZ + ZInterval);
        }
        
        /// <summary>
        /// Get camera target position for downward view
        /// </summary>
        public Vector3 GetDownTargetPosition()
        {
            float maxX = GetMaxX();
            return new Vector3(maxX / 2f, 0, -ZInterval);
        }
        
        /// <summary>
        /// Get camera target position for left view
        /// </summary>
        public Vector3 GetLeftTargetPosition()
        {
            float maxZ = GetMaxZ();
            return new Vector3(-XInterval, 0, maxZ / 2f);
        }
        
        /// <summary>
        /// Get camera target position for right view
        /// </summary>
        public Vector3 GetRightTargetPosition()
        {
            float maxX = GetMaxX();
            float maxZ = GetMaxZ();
            return new Vector3(maxX + XInterval, 0, maxZ / 2f);
        }
    }
}

