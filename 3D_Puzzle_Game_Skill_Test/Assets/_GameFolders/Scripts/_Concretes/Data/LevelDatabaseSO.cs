using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace BufoGames.Data
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "Puzzle/Level Database")]
    public class LevelDatabaseSO : ScriptableObject
    {
        [Header("All Levels")]
        public List<LevelDataSO> levels = new List<LevelDataSO>();
        
        public LevelDataSO GetLevel(int index)
        {
            if (index >= 0 && index < levels.Count)
                return levels[index];
            
            Debug.LogError($"LevelDatabase: Level index {index} out of range! Total levels: {levels.Count}");
            return null;
        }
        
        public int GetTotalLevelCount()
        {
            return levels.Count;
        }
        
        public void AddLevel(LevelDataSO level)
        {
            if (level != null && !levels.Contains(level))
            {
                levels.Add(level);
                Debug.Log($"LevelDatabase: Added level {level.levelIndex}");
            }
        }
        
        public void RemoveLevel(LevelDataSO level)
        {
            if (levels.Contains(level))
            {
                levels.Remove(level);
                Debug.Log($"LevelDatabase: Removed level {level.levelIndex}");
            }
        }
        
        public void SortByLevelIndex()
        {
            levels = levels.OrderBy(l => l.levelIndex).ToList();
        }
    }
}

