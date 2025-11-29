namespace BufoGames.Constants
{
    /// <summary>
    /// Central location for all level-related constants
    /// </summary>
    public static class LevelConstants
    {
        // Timing Constants
        public const float INITIALIZATION_DELAY = 0.5f;
        public const float ROTATION_CHECK_DELAY = 0.6f;
        public const float COMPLETION_ANIMATION_DURATION = 2f;
        
        // Grid Constants
        public const float X_INTERVAL = 0.71f;
        public const float Z_INTERVAL = 0.71f;
        
        // Animation Constants
        public const float DEFAULT_ROTATION_DURATION = 0.5f;
        public const float DEFAULT_SCALE_DURATION = 0.15f;
        
        // Tag Constants
        public const string SOURCE_TAG = "Source";
        public const string DESTINATION_TAG = "Destination";
        public const string PIPE_TAG = "Pipe";
        public const string GROUND_OBJECT_TAG = "GroundObject";
        
        // Layer Constants
        public const string TILE_LAYER = "TILE";
    }
}

