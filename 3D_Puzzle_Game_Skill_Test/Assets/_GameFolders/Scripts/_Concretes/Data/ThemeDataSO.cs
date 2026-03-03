using UnityEngine;

namespace BufoGames.Data
{
    [CreateAssetMenu(fileName = "Theme_Default", menuName = "Puzzle/Theme")]
    public class ThemeDataSO : ScriptableObject
    {
        [Header("Tile Prefabs")]
        public GameObject tileAPrefab;
        public GameObject tileBPrefab;
        
        [Header("Special Pieces")]
        public GameObject sourcePrefab;
        public GameObject destinationPrefab;
        
        [Header("Rotatable Pipe Prefabs")]
        [Tooltip("Straight pipe (I shape) - ┃")]
        public GameObject straightPipePrefab;
        
        [Tooltip("Corner pipe (L shape) - └")]
        public GameObject cornerPipePrefab;
        
        [Tooltip("T-Junction pipe (T shape) - ├")]
        public GameObject tJunctionPipePrefab;
        
        [Tooltip("Cross pipe (+ shape) - ╬")]
        public GameObject crossPipePrefab;
        
        [Header("Static Pipe Prefabs (Non-Rotatable)")]
        [Tooltip("Static straight pipe - locked in place")]
        public GameObject staticStraightPipePrefab;
        
        [Tooltip("Static corner pipe - locked in place")]
        public GameObject staticCornerPipePrefab;
        
        [Tooltip("Static T-Junction pipe - locked in place")]
        public GameObject staticTJunctionPipePrefab;
        
        [Tooltip("Static cross pipe - locked in place")]
        public GameObject staticCrossPipePrefab;
        
        [Header("Materials (Optional)")]
        public Material pipeMaterial;
        public Material activePipeMaterial;
        public Material staticPipeMaterial;
        
        /// <summary>
        /// Get pipe prefab based on piece type
        /// </summary>
        public GameObject GetPipePrefab(PieceType type)
        {
            return type switch
            {
                // Rotatable pipes
                PieceType.StraightPipe => straightPipePrefab,
                PieceType.CornerPipe => cornerPipePrefab,
                PieceType.TJunctionPipe => tJunctionPipePrefab,
                PieceType.CrossPipe => crossPipePrefab,
                
                // Static pipes (fallback to rotatable if static not assigned)
                PieceType.StaticStraightPipe => staticStraightPipePrefab != null ? staticStraightPipePrefab : straightPipePrefab,
                PieceType.StaticCornerPipe => staticCornerPipePrefab != null ? staticCornerPipePrefab : cornerPipePrefab,
                PieceType.StaticTJunctionPipe => staticTJunctionPipePrefab != null ? staticTJunctionPipePrefab : tJunctionPipePrefab,
                PieceType.StaticCrossPipe => staticCrossPipePrefab != null ? staticCrossPipePrefab : crossPipePrefab,
                
                // Special pieces
                PieceType.Source => sourcePrefab,
                PieceType.Destination => destinationPrefab,
                
                _ => null
            };
        }
        
        /// <summary>
        /// Check if static prefabs are assigned
        /// </summary>
        public bool HasStaticPrefabs()
        {
            return staticStraightPipePrefab != null ||
                   staticCornerPipePrefab != null ||
                   staticTJunctionPipePrefab != null ||
                   staticCrossPipePrefab != null;
        }
    }
}

