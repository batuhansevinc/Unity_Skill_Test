namespace BufoGames.Abstract.Controllers
{
    /// <summary>
    /// Interface for level controller implementations
    /// </summary>
    public interface ILevelController
    {
        /// <summary>
        /// Get the current grid size
        /// </summary>
        int GetGridSize();
        
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

