using UnityEngine;
using UnityEditor;

namespace BufoGames.Editor
{
    public class BatchLevelPrefabFixer : EditorWindow
    {
        [MenuItem("Tools/Batch Level Prefab Fixer (Deprecated)")]
        public static void ShowWindow()
        {
            GetWindow<BatchLevelPrefabFixer>("Batch Level Fixer");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Bu araç artık kullanılmıyor.\n\nLevel'lar artık runtime'da LevelDataSO'dan generate ediliyor ve event'ler LevelManager tarafından inject ediliyor.", MessageType.Info);
        }
    }
}

