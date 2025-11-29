using UnityEngine;
using UnityEditor;
using BufoGames.Managers;

namespace BufoGames.Editor
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerValidator : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Setup Validation", EditorStyles.boldLabel);
            
            LevelManager levelManager = (LevelManager)target;
            
            SerializedProperty levelDatabaseProp = serializedObject.FindProperty("levelDatabase");
            SerializedProperty defaultThemeProp = serializedObject.FindProperty("defaultTheme");
            
            if (levelDatabaseProp != null && levelDatabaseProp.objectReferenceValue != null)
            {
                EditorGUILayout.HelpBox("✅ Level Database atanmış", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ Level Database eksik!", MessageType.Error);
            }
            
            if (defaultThemeProp != null && defaultThemeProp.objectReferenceValue != null)
            {
                EditorGUILayout.HelpBox("✅ Default Theme atanmış", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ Default Theme eksik!", MessageType.Error);
            }
            
            EditorGUILayout.Space(5);
            
            SerializedProperty upTargetProp = serializedObject.FindProperty("upTarget");
            SerializedProperty downTargetProp = serializedObject.FindProperty("downTarget");
            SerializedProperty leftTargetProp = serializedObject.FindProperty("leftTarget");
            SerializedProperty rightTargetProp = serializedObject.FindProperty("rightTarget");
            
            bool allTargetsAssigned = true;
            
            if (upTargetProp == null || upTargetProp.objectReferenceValue == null)
            {
                allTargetsAssigned = false;
            }
            if (downTargetProp == null || downTargetProp.objectReferenceValue == null)
            {
                allTargetsAssigned = false;
            }
            if (leftTargetProp == null || leftTargetProp.objectReferenceValue == null)
            {
                allTargetsAssigned = false;
            }
            if (rightTargetProp == null || rightTargetProp.objectReferenceValue == null)
            {
                allTargetsAssigned = false;
            }
            
            if (allTargetsAssigned)
            {
                EditorGUILayout.HelpBox("✅ Tüm Camera Target'lar atanmış", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Bazı Camera Target'lar eksik", MessageType.Warning);
            }
            
            EditorGUILayout.Space(5);
            
            SerializedProperty levelCompletedEventProp = serializedObject.FindProperty("levelCompletedEvent");
            SerializedProperty fireworksEventProp = serializedObject.FindProperty("fireworksEvent");
            SerializedProperty startEndGameAnimationsEventProp = serializedObject.FindProperty("startEndGameAnimationsEvent");
            
            bool allEventsAssigned = true;
            
            if (levelCompletedEventProp == null || levelCompletedEventProp.objectReferenceValue == null)
            {
                allEventsAssigned = false;
            }
            if (fireworksEventProp == null || fireworksEventProp.objectReferenceValue == null)
            {
                allEventsAssigned = false;
            }
            if (startEndGameAnimationsEventProp == null || startEndGameAnimationsEventProp.objectReferenceValue == null)
            {
                allEventsAssigned = false;
            }
            
            if (allEventsAssigned)
            {
                EditorGUILayout.HelpBox("✅ Tüm Game Event'ler atanmış", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Bazı Game Event'ler eksik", MessageType.Warning);
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Hızlı İşlemler", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Camera Target'ları Otomatik Oluştur"))
            {
                CreateCameraTargets();
            }
        }
        
        private void CreateCameraTargets()
        {
            GameObject targetsParent = GameObject.Find("CameraTargets");
            if (targetsParent == null)
            {
                targetsParent = new GameObject("CameraTargets");
            }
            
            SerializedProperty upTargetProp = serializedObject.FindProperty("upTarget");
            SerializedProperty downTargetProp = serializedObject.FindProperty("downTarget");
            SerializedProperty leftTargetProp = serializedObject.FindProperty("leftTarget");
            SerializedProperty rightTargetProp = serializedObject.FindProperty("rightTarget");
            
            if (upTargetProp.objectReferenceValue == null)
            {
                GameObject upTarget = new GameObject("UpTarget");
                upTarget.transform.parent = targetsParent.transform;
                upTargetProp.objectReferenceValue = upTarget.transform;
            }
            
            if (downTargetProp.objectReferenceValue == null)
            {
                GameObject downTarget = new GameObject("DownTarget");
                downTarget.transform.parent = targetsParent.transform;
                downTargetProp.objectReferenceValue = downTarget.transform;
            }
            
            if (leftTargetProp.objectReferenceValue == null)
            {
                GameObject leftTarget = new GameObject("LeftTarget");
                leftTarget.transform.parent = targetsParent.transform;
                leftTargetProp.objectReferenceValue = leftTarget.transform;
            }
            
            if (rightTargetProp.objectReferenceValue == null)
            {
                GameObject rightTarget = new GameObject("RightTarget");
                rightTarget.transform.parent = targetsParent.transform;
                rightTargetProp.objectReferenceValue = rightTarget.transform;
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}

