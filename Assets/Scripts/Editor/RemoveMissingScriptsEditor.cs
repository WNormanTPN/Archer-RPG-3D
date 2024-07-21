using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class RemoveMissingScriptsEditor
    {
        [MenuItem("GameObject/Editor Extensions/Remove Missing Scripts")]
        private static void FindAndRemoveMissingInSelected()
        {
            GameObject[] allObjects = GetAllChildren(Selection.gameObjects);
            int count = RemoveMissingScriptsFrom(allObjects);
            if (count == 0) return;
            EditorUtility.DisplayDialog("Remove Missing Scripts", $"Removed {count} missing scripts.\n\nCheck console for details", "ok");
        }

        [MenuItem("Assets/Editor Extensions/Remove Missing Scripts")]
        private static void FindAndRemoveMissingInSelectedAssets()
        {
            FindAndRemoveMissingInSelected();
        }

        [MenuItem("Assets/Editor Extensions/Remove Missing Scripts", true)]
        private static bool FindAndRemoveMissingInSelectedAssetsValidate()
        {
            return Selection.objects.OfType<GameObject>().Any();
        }

        [MenuItem("Tools/Editor Extensions/Remove Missing Scripts From Prefabs")]
        private static void RemoveFromPrefabs()
        {
            string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
            IEnumerable<string> allPrefabsPath = allPrefabGuids.Select(AssetDatabase.GUIDToAssetPath);
            IEnumerable<GameObject> allPrefabsObjects = allPrefabsPath.Select(AssetDatabase.LoadAssetAtPath<GameObject>);
        
            List<GameObject> prefabsToSave = new List<GameObject>();
        
            foreach (var prefab in allPrefabsObjects)
            {
                if (prefab != null)
                {
                    ProcessPrefab(prefab);
                    prefabsToSave.Add(prefab);
                }
            }
        
            // Save prefabs after processing
            foreach (var prefab in prefabsToSave)
            {
                if (prefab != null)
                {
                    PrefabUtility.SavePrefabAsset(prefab);
                }
            }
        
            Debug.Log($"Removed All Missing Scripts from Prefabs");
        }

        private static void ProcessPrefab(GameObject prefab)
        {
            if (prefab == null) return;

            // Process children first
            RemoveMissingScriptsFrom(prefab.transform.GetComponentsInChildren<Transform>(true)
                .Select(t => t.gameObject)
                .ToArray());

            // Save prefab after processing children
            PrefabUtility.SavePrefabAsset(prefab);

            // Process parent
            RemoveMissingScriptsFrom(prefab);
        }

        private static int RemoveMissingScriptsFrom(params GameObject[] objects)
        {
            List<GameObject> forceSave = new List<GameObject>();
            int removedCounter = 0;
            foreach (GameObject current in objects)
            {
                if (current == null) continue;

                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(current);
                if (missingCount == 0) continue;

                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(current);
                EditorUtility.SetDirty(current);

                if (EditorUtility.IsPersistent(current) && PrefabUtility.IsAnyPrefabInstanceRoot(current))
                    forceSave.Add(current);

                Debug.Log($"Removed {missingCount} Missing Scripts from {current.name}", current);
                removedCounter += missingCount;
            }

            // Return the count of removed missing scripts
            return removedCounter;
        }

        private static GameObject[] GetAllChildren(GameObject[] selection)
        {
            List<Transform> transforms = new List<Transform>();

            foreach (GameObject o in selection)
            {
                transforms.AddRange(o.GetComponentsInChildren<Transform>(true));
            }

            return transforms.Distinct().Select(t => t.gameObject).ToArray();
        }
    }
}
