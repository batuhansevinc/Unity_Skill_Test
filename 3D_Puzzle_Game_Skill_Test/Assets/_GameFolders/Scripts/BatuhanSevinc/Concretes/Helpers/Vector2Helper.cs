using UnityEngine;

namespace BatuhanSevinc.Helpers
{
    public static class Vector2Helper
    {
        public static Vector2 Zero { get; }
        public static Vector2 Left { get; }
        public static Vector2 Right { get; }
        public static Vector2 Up { get; }
        public static Vector2 Down { get; }
        
        static Vector2Helper()
        {
            Zero = Vector2.zero;
            Left = Vector2.left;
            Right = Vector2.right;
            Up = Vector2.up;
            Down = Vector2.down;
        }
    }
}