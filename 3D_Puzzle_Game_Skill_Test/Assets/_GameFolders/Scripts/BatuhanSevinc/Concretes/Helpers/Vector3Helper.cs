using UnityEngine;

namespace BatuhanSevinc.Helpers
{
    public static class Vector3Helper
    {
        public static Vector3 Zero { get; }
        public static Vector3 Forward { get; }
        public static Vector3 Back { get; }
        public static Vector3 Left { get; }
        public static Vector3 Right { get; }
        public static Vector3 Up { get; }
        public static Vector3 Down { get; }
        
        static Vector3Helper()
        {
            Zero = Vector3.zero;
            Forward = Vector3.forward;
            Back = Vector3.back;
            Left = Vector3.left;
            Right = Vector3.right;
            Up = Vector3.up;
            Down = Vector3.down;
        }
    }
}