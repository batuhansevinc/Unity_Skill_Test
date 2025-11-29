namespace BufoGames.Grid
{
    /// <summary>
    /// Cardinal directions for grid-based connections
    /// Order matters: Used for rotation calculation (clockwise)
    /// </summary>
    public enum Direction
    {
        Up = 0,     // +Z
        Right = 1,  // +X
        Down = 2,   // -Z
        Left = 3    // -X
    }
}
