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
        
        // Spawn Animation Constants
        public const float SPAWN_DROP_HEIGHT = 3f;
        public const float TILE_DROP_DURATION = 0.55f;
        public const float PIECE_DROP_DURATION = 0.35f;
        public const float SPAWN_STAGGER_INTERVAL = 0.02f;
        public const float SPAWN_TOTAL_MAX_DURATION = 1f;
        public const float PHASE_GAP = 0.1f;
        
        // Tag Constants
        public const string SOURCE_TAG = "Source";
        public const string DESTINATION_TAG = "Destination";
        public const string PIPE_TAG = "Pipe";
        public const string GROUND_OBJECT_TAG = "GroundObject";
        
        // Layer Constants
        public const string TILE_LAYER = "TILE";
    }
}

