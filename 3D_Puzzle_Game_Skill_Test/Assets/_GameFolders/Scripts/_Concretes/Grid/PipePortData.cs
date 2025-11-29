using BufoGames.Data;

namespace BufoGames.Grid
{
    /// <summary>
    /// Static data for pipe connection ports based on type and rotation
    /// Zero allocation - uses pre-allocated arrays
    /// </summary>
    public static class PipePortData
    {
        // Pre-allocated port arrays for each pipe type (base rotation = 0)
        // StraightPipe: | shape - connects Up and Down
        private static readonly Direction[] StraightPorts = { Direction.Up, Direction.Down };
        
        // CornerPipe: └ shape - connects Up and Right
        private static readonly Direction[] CornerPorts = { Direction.Up, Direction.Right };
        
        // TJunctionPipe: ├ shape - connects Up, Down, Right
        private static readonly Direction[] TJunctionPorts = { Direction.Up, Direction.Down, Direction.Right };
        
        // CrossPipe: + shape - connects all directions
        private static readonly Direction[] CrossPorts = { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        // Source: single direction (Up at base rotation) - like a pipe outlet
        private static readonly Direction[] SourcePorts = { Direction.Up };
        
        // Destination: single direction (Up at base rotation) - like a pipe inlet
        private static readonly Direction[] DestinationPorts = { Direction.Up };
        
        // Empty array for unknown types
        private static readonly Direction[] EmptyPorts = { };
        
        /// <summary>
        /// Get base ports for a piece type (rotation = 0)
        /// </summary>
        public static Direction[] GetBasePorts(PieceType type)
        {
            return type switch
            {
                PieceType.StraightPipe => StraightPorts,
                PieceType.CornerPipe => CornerPorts,
                PieceType.TJunctionPipe => TJunctionPorts,
                PieceType.CrossPipe => CrossPorts,
                PieceType.Source => SourcePorts,
                PieceType.Destination => DestinationPorts,
                _ => EmptyPorts
            };
        }
        
        /// <summary>
        /// Get port count for a piece type
        /// </summary>
        public static int GetPortCount(PieceType type)
        {
            return type switch
            {
                PieceType.StraightPipe => 2,
                PieceType.CornerPipe => 2,
                PieceType.TJunctionPipe => 3,
                PieceType.CrossPipe => 4,
                PieceType.Source => 1,
                PieceType.Destination => 1,
                _ => 0
            };
        }
        
        /// <summary>
        /// Check if a piece type has a port in given direction at given rotation
        /// Most performant method - no allocations
        /// </summary>
        public static bool HasPort(PieceType type, int rotation, Direction direction)
        {
            int steps = (rotation / 90) % 4;
            Direction[] basePorts = GetBasePorts(type);
            
            foreach (var basePort in basePorts)
            {
                Direction rotatedPort = DirectionHelper.Rotate(basePort, steps);
                if (rotatedPort == direction)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Fill a pre-allocated array with rotated ports
        /// Use this to avoid allocations in hot paths
        /// </summary>
        /// <param name="type">Piece type</param>
        /// <param name="rotation">Rotation in degrees (0, 90, 180, 270)</param>
        /// <param name="result">Pre-allocated array to fill</param>
        /// <returns>Number of ports filled</returns>
        public static int GetRotatedPorts(PieceType type, int rotation, Direction[] result)
        {
            Direction[] basePorts = GetBasePorts(type);
            int steps = (rotation / 90) % 4;
            int count = basePorts.Length;
            
            for (int i = 0; i < count && i < result.Length; i++)
            {
                result[i] = DirectionHelper.Rotate(basePorts[i], steps);
            }
            
            return count;
        }
    }
}
