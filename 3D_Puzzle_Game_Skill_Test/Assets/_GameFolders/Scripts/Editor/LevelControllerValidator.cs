using UnityEngine;
using UnityEditor;
using BufoGames.Controller;

namespace BufoGames.Editor
{
    [CustomEditor(typeof(LevelController))]
    public class LevelControllerValidator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Completion Flow", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Level completion is emitted via typed C# events on LevelController and orchestrated by GameSceneManager/UIManager.",
                MessageType.Info);

            if (Application.isPlaying)
            {
                LevelController levelController = (LevelController)target;
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Is Level Complete", levelController.IsLevelComplete ? "Yes" : "No");
                EditorGUILayout.LabelField("Connection Stats", levelController.GetConnectionStats());
            }
        }
    }
}

