namespace BufoGames.Grid
{
    /// <summary>
    /// Helper methods for Direction calculations
    /// Pure static methods - no allocations
    /// </summary>
    public static class DirectionHelper
    {
        /// <summary>
        /// Get grid offset for a direction
        /// </summary>
        public static void GetOffset(Direction dir, out int dx, out int dz)
        {
            switch (dir)
            {
                case Direction.Up:
                    dx = 0; dz = 1;
                    break;
                case Direction.Down:
                    dx = 0; dz = -1;
                    break;
                case Direction.Left:
                    dx = -1; dz = 0;
                    break;
                case Direction.Right:
                    dx = 1; dz = 0;
                    break;
                default:
                    dx = 0; dz = 0;
                    break;
            }
        }
        
        /// <summary>
        /// Get the opposite direction
        /// </summary>
        public static Direction GetOpposite(Direction dir)
        {
            return dir switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => dir
            };
        }
        
        /// <summary>
        /// Rotate direction clockwise by steps (each step = 90°)
        /// </summary>
        public static Direction Rotate(Direction dir, int steps)
        {
            int index = (int)dir;
            int rotated = (index + steps) % 4;
            if (rotated < 0) rotated += 4;
            return (Direction)rotated;
        }
        
        /// <summary>
        /// Check if two positions are neighbors in given direction
        /// </summary>
        public static bool AreNeighbors(int x1, int z1, int x2, int z2, out Direction directionFromFirst)
        {
            int dx = x2 - x1;
            int dz = z2 - z1;
            
            if (dx == 0 && dz == 1) { directionFromFirst = Direction.Up; return true; }
            if (dx == 0 && dz == -1) { directionFromFirst = Direction.Down; return true; }
            if (dx == 1 && dz == 0) { directionFromFirst = Direction.Right; return true; }
            if (dx == -1 && dz == 0) { directionFromFirst = Direction.Left; return true; }
            
            directionFromFirst = Direction.Up;
            return false;
        }
    }
}
