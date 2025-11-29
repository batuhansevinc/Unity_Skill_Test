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
        
        [Header("Pipe Prefabs - Each Type")]
        [Tooltip("Straight pipe (I shape) - ┃")]
        public GameObject straightPipePrefab;
        
        [Tooltip("Corner pipe (L shape) - └")]
        public GameObject cornerPipePrefab;
        
        [Tooltip("T-Junction pipe (T shape) - ├")]
        public GameObject tJunctionPipePrefab;
        
        [Tooltip("Cross pipe (+ shape) - ╬")]
        public GameObject crossPipePrefab;
        
        [Header("Materials (Optional)")]
        public Material pipeMaterial;
        public Material activePipeMaterial;
        
        /// <summary>
        /// Get pipe prefab based on piece type
        /// </summary>
        public GameObject GetPipePrefab(PieceType type)
        {
            switch (type)
            {
                case PieceType.StraightPipe:
                    return straightPipePrefab;
                case PieceType.CornerPipe:
                    return cornerPipePrefab;
                case PieceType.TJunctionPipe:
                    return tJunctionPipePrefab;
                case PieceType.CrossPipe:
                    return crossPipePrefab;
                case PieceType.Source:
                    return sourcePrefab;
                case PieceType.Destination:
                    return destinationPrefab;
                default:
                    return null;
            }
        }
    }
}

