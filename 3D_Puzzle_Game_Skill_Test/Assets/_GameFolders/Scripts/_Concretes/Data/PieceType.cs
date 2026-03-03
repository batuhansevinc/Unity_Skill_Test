namespace BufoGames.Data
{
    /// <summary>
    /// Types of pieces that can be placed on the grid
    /// </summary>
    public enum PieceType
    {
        None,
        
        // Special pieces
        Source,
        Destination,
        
        // Rotatable pipes
        StraightPipe,
        CornerPipe,
        TJunctionPipe,
        CrossPipe,
        
        // Static (non-rotatable) pipes - same port logic, different visuals
        StaticStraightPipe,
        StaticCornerPipe,
        StaticTJunctionPipe,
        StaticCrossPipe
    }
    
    /// <summary>
    /// Helper extension methods for PieceType
    /// </summary>
    public static class PieceTypeExtensions
    {
        /// <summary>
        /// Check if piece type is static (non-rotatable)
        /// </summary>
        public static bool IsStatic(this PieceType type)
        {
            return type == PieceType.StaticStraightPipe ||
                   type == PieceType.StaticCornerPipe ||
                   type == PieceType.StaticTJunctionPipe ||
                   type == PieceType.StaticCrossPipe;
        }
        
        /// <summary>
        /// Check if piece type is rotatable
        /// </summary>
        public static bool IsRotatable(this PieceType type)
        {
            return type == PieceType.StraightPipe ||
                   type == PieceType.CornerPipe ||
                   type == PieceType.TJunctionPipe ||
                   type == PieceType.CrossPipe ||
                   type == PieceType.Source ||
                   type == PieceType.Destination;
        }
        
        /// <summary>
        /// Get the base pipe type (static → rotatable equivalent)
        /// </summary>
        public static PieceType GetBasePipeType(this PieceType type)
        {
            return type switch
            {
                PieceType.StaticStraightPipe => PieceType.StraightPipe,
                PieceType.StaticCornerPipe => PieceType.CornerPipe,
                PieceType.StaticTJunctionPipe => PieceType.TJunctionPipe,
                PieceType.StaticCrossPipe => PieceType.CrossPipe,
                _ => type
            };
        }
    }
}

