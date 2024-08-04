using Evironment.MapGenerator;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGenerationSettingsEditor : UnityEditor.Editor
    {
        private bool showMapGenerationSettings = true;
        private bool showFenceSettings = true;
        private bool showPoolingObjectSettings = true;

        public override void OnInspectorGUI()
        {
            // Get a reference to the target script
            MapGenerator mapSettings = (MapGenerator)target;

            // Begin checking for changes
            serializedObject.Update();

            // Draw the "Map Generation Settings" fields
            showMapGenerationSettings = EditorGUILayout.Foldout(showMapGenerationSettings, "Map Generation Settings", true, EditorStyles.foldout);
            if (showMapGenerationSettings)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("player"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("viewDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("unloadDistance"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tileSpacing"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleSpawnRatio"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectPool"));
            }

            // Draw the fields for fencePrefab and fenceParent if the limited map is true
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isLimitedMap"));
            if (mapSettings.isLimitedMap)
            {
                showFenceSettings = EditorGUILayout.Foldout(showFenceSettings, "Fence Settings", true, EditorStyles.foldout);
                if (showFenceSettings)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fencePrefab"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fenceParent"));
                }
            }

            // Draw the "Pooling Object Settings" fields
            showPoolingObjectSettings = EditorGUILayout.Foldout(showPoolingObjectSettings, "Pooling Object Settings", true, EditorStyles.foldout);
            if (showPoolingObjectSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tileTypes"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("tileParent"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleTypes"), true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("obstacleParent"));
                EditorGUI.indentLevel--;
            }
            

            // Apply changes to the serialized object
            serializedObject.ApplyModifiedProperties();
        }
    }
}
