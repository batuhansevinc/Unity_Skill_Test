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
            EditorGUILayout.LabelField("Event Validation", EditorStyles.boldLabel);
            
            SerializedProperty levelCompletedEventProp = serializedObject.FindProperty("levelCompletedEvent");
            SerializedProperty fireworksEventProp = serializedObject.FindProperty("fireworksEvent");
            SerializedProperty startEndGameAnimationsEventProp = serializedObject.FindProperty("startEndGameAnimationsEvent");
            
            bool allEventsAssigned = true;
            
            if (levelCompletedEventProp == null || levelCompletedEventProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("⚠️ Level Completed Event eksik (Runtime'da LevelManager tarafından inject edilecek)", MessageType.Warning);
                allEventsAssigned = false;
            }
            else
            {
                EditorGUILayout.HelpBox("✅ Level Completed Event atanmış", MessageType.Info);
            }
            
            if (fireworksEventProp == null || fireworksEventProp.objectReferenceValue == null)
            {
                allEventsAssigned = false;
            }
            
            if (startEndGameAnimationsEventProp == null || startEndGameAnimationsEventProp.objectReferenceValue == null)
            {
                allEventsAssigned = false;
            }
            
            EditorGUILayout.Space(5);
            
            if (!allEventsAssigned)
            {
                EditorGUILayout.HelpBox("ℹ️ Event'ler runtime'da LevelManager tarafından inject edilir. Prefab'da boş olması normaldir.", MessageType.Info);
            }
        }
    }
}

