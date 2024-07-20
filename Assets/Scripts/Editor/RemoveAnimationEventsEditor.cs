using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class RemoveAnimationEventsEditor
{
    [MenuItem("Tools/Editor Extensions/Remove All Animation Events from Prefabs")]
    private static void RemoveAnimationEventsFromPrefabs()
    {
        // Find all prefab assets in the project
        string[] allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");
        IEnumerable<string> allPrefabsPath = allPrefabGuids.Select(AssetDatabase.GUIDToAssetPath);

        // List to keep track of prefabs that need saving
        List<GameObject> prefabsToSave = new List<GameObject>();

        foreach (var prefabPath in allPrefabsPath)
        {
            // Load the prefab
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;

            // Process the prefab to remove animation events
            ProcessPrefab(prefab);
            prefabsToSave.Add(prefab);
        }

        // Save all modified prefabs
        foreach (var prefab in prefabsToSave)
        {
            if (prefab != null)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        Debug.Log("Removed all animation events from prefabs.");
    }

    private static void ProcessPrefab(GameObject prefab)
    {
        // Process all animation clips in the prefab
        AnimationClip[] clips = GetAnimationClipsFrom(prefab);
        foreach (var clip in clips)
        {
            if (clip != null)
            {
                RemoveAnimationEvents(clip);
            }
        }
    }

    private static AnimationClip[] GetAnimationClipsFrom(GameObject prefab)
    {
        List<AnimationClip> clips = new List<AnimationClip>();

        // Get all Animation components in the prefab
        Animation[] animations = prefab.GetComponentsInChildren<Animation>(true);
        foreach (var animation in animations)
        {
            foreach (AnimationState state in animation)
            {
                if (state.clip != null)
                {
                    clips.Add(state.clip);
                }
            }
        }

        // Get all Animator components in the prefab
        Animator[] animators = prefab.GetComponentsInChildren<Animator>(true);
        foreach (var animator in animators)
        {
            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                AnimationClip[] controllerClips = controller.animationClips;
                clips.AddRange(controllerClips);
            }
        }

        return clips.ToArray();
    }

    private static void RemoveAnimationEvents(AnimationClip clip)
    {
        if (clip == null) return;

        // Get current animation events
        var events = AnimationUtility.GetAnimationEvents(clip);

        // Remove all animation events
        if (events.Length > 0)
        {
            AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[0]);
            EditorUtility.SetDirty(clip);
        }
    }
}
