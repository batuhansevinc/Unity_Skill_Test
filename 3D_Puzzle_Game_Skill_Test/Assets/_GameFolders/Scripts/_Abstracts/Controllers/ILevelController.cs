namespace BufoGames.Abstract.Controllers
{
    /// <summary>
    /// Interface for level controller implementations
    /// Supports asymmetric grids (e.g., 3x5, 4x6)
    /// </summary>
    public interface ILevelController
    {
        /// <summary>
        /// Get the grid width (X axis / columns)
        /// </summary>
        int GetGridWidth();
        
        /// <summary>
        /// Get the grid height (Z axis / rows)
        /// </summary>
        int GetGridHeight();
        
        /// <summary>
        /// Get X interval between grid cells
        /// </summary>
        float GetXInterval();
        
        /// <summary>
        /// Get Z interval between grid cells
        /// </summary>
        float GetZInterval();
        
        /// <summary>
        /// Initialize the level
        /// </summary>
        void InitializeLevel();
        
        /// <summary>
        /// Check if level is completed
        /// </summary>
        void CheckLevelCompletion();
    }
}

