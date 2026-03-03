using UnityEngine;
using System.Collections.Generic;

namespace BufoGames.Data
{
    /// <summary>
    /// Central helper for pipe connections - used by Generator, Validator, and Runtime
    /// Port mask bits: Up=1, Right=2, Down=4, Left=8
    /// Direction indices: Up=0, Right=1, Down=2, Left=3
    /// </summary>
    public static class PipeConnectionHelper
    {
        public const int PORT_UP = 1;
        public const int PORT_RIGHT = 2;
        public const int PORT_DOWN = 4;
        public const int PORT_LEFT = 8;
        
        public const int DIR_UP = 0;
        public const int DIR_RIGHT = 1;
        public const int DIR_DOWN = 2;
        public const int DIR_LEFT = 3;
        
        public static readonly int[] DirToPortMask = { PORT_UP, PORT_RIGHT, PORT_DOWN, PORT_LEFT };
        public static readonly int[] OppositeDir = { DIR_DOWN, DIR_LEFT, DIR_UP, DIR_RIGHT };
        public static readonly Vector2Int[] DirOffset = {
            new Vector2Int(0, 1),   // Up: z+1
            new Vector2Int(1, 0),   // Right: x+1
            new Vector2Int(0, -1),  // Down: z-1
            new Vector2Int(-1, 0)   // Left: x-1
        };
        
        /// <summary>
        /// Rotation=0: Up+Down (vertical)
        /// Rotation=90: Left+Right (horizontal)
        /// </summary>
        private static readonly int[] StraightMasks = {
            PORT_UP | PORT_DOWN,    // 0° = vertical
            PORT_LEFT | PORT_RIGHT, // 90° = horizontal
            PORT_UP | PORT_DOWN,    // 180° = vertical
            PORT_LEFT | PORT_RIGHT  // 270° = horizontal
        };
        
        /// <summary>
        /// Rotation=0: Up+Right
        /// Rotation=90: Right+Down
        /// Rotation=180: Down+Left
        /// Rotation=270: Left+Up
        /// </summary>
        private static readonly int[] CornerMasks = {
            PORT_UP | PORT_RIGHT,   // 0°
            PORT_RIGHT | PORT_DOWN, // 90°
            PORT_DOWN | PORT_LEFT,  // 180°
            PORT_LEFT | PORT_UP     // 270°
        };
        
        /// <summary>
        /// T-Junction has 3 ports (missing one)
        /// Rotation=0: Up+Right+Down (missing Left)
        /// Rotation=90: Right+Down+Left (missing Up)
        /// Rotation=180: Down+Left+Up (missing Right)
        /// Rotation=270: Left+Up+Right (missing Down)
        /// </summary>
        private static readonly int[] TJunctionMasks = {
            PORT_UP | PORT_RIGHT | PORT_DOWN,   // 0° missing Left
            PORT_RIGHT | PORT_DOWN | PORT_LEFT, // 90° missing Up
            PORT_DOWN | PORT_LEFT | PORT_UP,    // 180° missing Right
            PORT_LEFT | PORT_UP | PORT_RIGHT    // 270° missing Down
        };
        
        /// <summary>
        /// Cross has all 4 ports - rotation doesn't matter
        /// </summary>
        private const int CrossMask = PORT_UP | PORT_RIGHT | PORT_DOWN | PORT_LEFT;
        
        /// <summary>
        /// Source/Destination has single port
        /// Rotation=0: Up
        /// Rotation=90: Right
        /// Rotation=180: Down
        /// Rotation=270: Left
        /// </summary>
        private static readonly int[] SinglePortMasks = {
            PORT_UP,    // 0°
            PORT_RIGHT, // 90°
            PORT_DOWN,  // 180°
            PORT_LEFT   // 270°
        };
        
        public static int GetPortMask(PieceType type, int rotationDegrees)
        {
            int rotStep = ((rotationDegrees % 360) + 360) % 360 / 90;
            
            return type switch
            {
                PieceType.StraightPipe or PieceType.StaticStraightPipe => StraightMasks[rotStep % 2],
                PieceType.CornerPipe or PieceType.StaticCornerPipe => CornerMasks[rotStep],
                PieceType.TJunctionPipe or PieceType.StaticTJunctionPipe => TJunctionMasks[rotStep],
                PieceType.CrossPipe or PieceType.StaticCrossPipe => CrossMask,
                PieceType.Source or PieceType.Destination => SinglePortMasks[rotStep],
                _ => 0
            };
        }
        
        public static bool HasPort(PieceType type, int rotationDegrees, int direction)
        {
            return (GetPortMask(type, rotationDegrees) & DirToPortMask[direction]) != 0;
        }
        
        public static List<int> GetOpenPorts(PieceType type, int rotationDegrees)
        {
            var ports = new List<int>();
            int mask = GetPortMask(type, rotationDegrees);
            for (int dir = 0; dir < 4; dir++)
            {
                if ((mask & DirToPortMask[dir]) != 0)
                    ports.Add(dir);
            }
            return ports;
        }
        
        public static int GetPortCount(PieceType type)
        {
            return type switch
            {
                PieceType.Source or PieceType.Destination => 1,
                PieceType.StraightPipe or PieceType.StaticStraightPipe => 2,
                PieceType.CornerPipe or PieceType.StaticCornerPipe => 2,
                PieceType.TJunctionPipe or PieceType.StaticTJunctionPipe => 3,
                PieceType.CrossPipe or PieceType.StaticCrossPipe => 4,
                _ => 0
            };
        }
        
        public static int GetUniqueRotationCount(PieceType type)
        {
            return type switch
            {
                PieceType.StraightPipe or PieceType.StaticStraightPipe => 2,
                PieceType.CrossPipe or PieceType.StaticCrossPipe => 1,
                _ => 4
            };
        }
        
        /// <summary>
        /// Check if two adjacent pieces can connect
        /// </summary>
        public static bool CanConnect(PieceType typeA, int rotA, int dirFromA, 
                                       PieceType typeB, int rotB)
        {
            bool aHasPort = HasPort(typeA, rotA, dirFromA);
            bool bHasPort = HasPort(typeB, rotB, OppositeDir[dirFromA]);
            return aHasPort && bHasPort;
        }
        
        /// <summary>
        /// Get required rotation for a pipe to have specific ports open
        /// Returns -1 if impossible
        /// </summary>
        public static int GetRotationForPorts(PieceType type, HashSet<int> requiredDirs)
        {
            int uniqueRots = GetUniqueRotationCount(type);
            for (int r = 0; r < 4; r++)
            {
                int mask = GetPortMask(type, r * 90);
                bool allMatch = true;
                foreach (int dir in requiredDirs)
                {
                    if ((mask & DirToPortMask[dir]) == 0)
                    {
                        allMatch = false;
                        break;
                    }
                }
                if (allMatch)
                    return r * 90;
            }
            return -1;
        }
        
        /// <summary>
        /// Get best pipe type and rotation for given connection directions
        /// </summary>
        public static (PieceType type, int rotation) GetPipeForConnections(
            HashSet<int> dirs, List<PieceType> allowedTypes)
        {
            int count = dirs.Count;
            
            // 4 connections -> Cross
            if (count == 4 && allowedTypes.Contains(PieceType.CrossPipe))
                return (PieceType.CrossPipe, 0);
            
            // 3 connections -> T-Junction
            if (count >= 3 && allowedTypes.Contains(PieceType.TJunctionPipe))
            {
                int missing = -1;
                for (int i = 0; i < 4; i++)
                {
                    if (!dirs.Contains(i))
                    {
                        missing = i;
                        break;
                    }
                }
                // T-Junction rotation based on missing port
                int rot = missing switch
                {
                    DIR_LEFT => 0,    // Missing Left → 0°
                    DIR_UP => 90,     // Missing Up → 90°
                    DIR_RIGHT => 180, // Missing Right → 180°
                    DIR_DOWN => 270,  // Missing Down → 270°
                    _ => 0
                };
                return (PieceType.TJunctionPipe, rot);
            }
            
            // 2 connections
            if (count == 2)
            {
                var dirList = new List<int>(dirs);
                int d1 = dirList[0], d2 = dirList[1];
                
                // Opposite directions -> Straight
                if (Mathf.Abs(d1 - d2) == 2 && allowedTypes.Contains(PieceType.StraightPipe))
                {
                    // Vertical (Up+Down) = 0°, Horizontal (Left+Right) = 90°
                    int rot = (dirs.Contains(DIR_UP) && dirs.Contains(DIR_DOWN)) ? 0 : 90;
                    return (PieceType.StraightPipe, rot);
                }
                
                // Adjacent directions -> Corner
                if (allowedTypes.Contains(PieceType.CornerPipe))
                {
                    if (dirs.Contains(DIR_UP) && dirs.Contains(DIR_RIGHT)) 
                        return (PieceType.CornerPipe, 0);
                    if (dirs.Contains(DIR_RIGHT) && dirs.Contains(DIR_DOWN)) 
                        return (PieceType.CornerPipe, 90);
                    if (dirs.Contains(DIR_DOWN) && dirs.Contains(DIR_LEFT)) 
                        return (PieceType.CornerPipe, 180);
                    if (dirs.Contains(DIR_LEFT) && dirs.Contains(DIR_UP)) 
                        return (PieceType.CornerPipe, 270);
                }
            }
            
            // 1 connection - use straight pointing in that direction
            if (count == 1 && allowedTypes.Contains(PieceType.StraightPipe))
            {
                int dir = new List<int>(dirs)[0];
                int rot = (dir == DIR_UP || dir == DIR_DOWN) ? 0 : 90;
                return (PieceType.StraightPipe, rot);
            }
            
            // Fallback
            return (PieceType.StraightPipe, 0);
        }
        
        /// <summary>
        /// Convert direction index to rotation degrees for single-port pieces
        /// </summary>
        public static int DirToRotation(int dir)
        {
            return dir * 90;
        }
        
        /// <summary>
        /// Check if a piece at position would have any port pointing outside grid
        /// </summary>
        public static bool HasPortOutsideGrid(PieceType type, int rotation, 
            int x, int z, int gridWidth, int gridHeight)
        {
            var ports = GetOpenPorts(type, rotation);
            foreach (int dir in ports)
            {
                int nx = x + DirOffset[dir].x;
                int nz = z + DirOffset[dir].y;
                if (nx < 0 || nx >= gridWidth || nz < 0 || nz >= gridHeight)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get valid rotations for a piece at boundary position
        /// </summary>
        public static List<int> GetValidRotationsAtPosition(PieceType type, 
            int x, int z, int gridWidth, int gridHeight)
        {
            var validRots = new List<int>();
            int uniqueRots = GetUniqueRotationCount(type);
            int step = 360 / uniqueRots;
            
            for (int r = 0; r < uniqueRots; r++)
            {
                int rot = r * step;
                if (!HasPortOutsideGrid(type, rot, x, z, gridWidth, gridHeight))
                    validRots.Add(rot);
            }
            return validRots;
        }
    }
}
