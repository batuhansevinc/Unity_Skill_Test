using UnityEngine;
using BufoGames.Constants;
using BufoGames.Data;

namespace BufoGames.Generation
{
    /// <summary>
    /// Generates grid tiles from level data
    /// </summary>
    public class GridGenerator
    {
        public void GenerateGrid(GameObject parent, LevelDataSO levelData, ThemeDataSO theme)
        {
            if (theme == null || theme.tileAPrefab == null || theme.tileBPrefab == null)
            {
                Debug.LogError("GridGenerator: Theme or tile prefabs are missing!");
                return;
            }
            
            int gridSize = levelData.gridSize;
            
            // Generate checkerboard pattern
            for (int x = 0; x < gridSize; x++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    GameObject tilePrefab = ((x + z) % 2 == 0) ? theme.tileAPrefab : theme.tileBPrefab;
                    Vector3 position = GetGridPosition(x, z);
                    
                    GameObject tile = GameObject.Instantiate(tilePrefab, position, Quaternion.identity, parent.transform);
                    tile.name = $"Tile_{x}_{z}";
                }
            }
            
            Debug.Log($"GridGenerator: Generated {gridSize}x{gridSize} grid");
        }
        
        private Vector3 GetGridPosition(int x, int z)
        {
            return new Vector3(
                x * LevelConstants.X_INTERVAL,
                0,
                z * LevelConstants.Z_INTERVAL
            );
        }
    }
}

