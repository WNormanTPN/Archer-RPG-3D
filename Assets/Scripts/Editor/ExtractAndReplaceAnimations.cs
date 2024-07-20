using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public class ExtractAndReplaceAnimations : EditorWindow
{
    private string extractionPath = "Assets/ExtractedAnimations/";

    [MenuItem("Tools/Editor Extensions/Extract and Replace Animations from FBX")]
    public static void ShowWindow()
    {
        GetWindow<ExtractAndReplaceAnimations>("Extract and Replace Animations from FBX");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Extraction Path", extractionPath);
        extractionPath = EditorGUILayout.TextField(extractionPath);

        if (GUILayout.Button("Extract and Replace Animations"))
        {
            ExtractAndReplace();
        }
    }

    private void ExtractAndReplace()
    {
        Dictionary<string, AnimationClip> extractedClips = ExtractAnimations();
        ReplaceAnimationReferences(extractedClips);
    }

    private Dictionary<string, AnimationClip> ExtractAnimations()
    {
        Dictionary<string, AnimationClip> extractedClips = new Dictionary<string, AnimationClip>();

        if (!Directory.Exists(extractionPath))
        {
            Directory.CreateDirectory(extractionPath);
        }

        string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });

        foreach (string guid in fbxGuids)
        {
            string fbxPath = AssetDatabase.GUIDToAssetPath(guid);
            ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;

            if (modelImporter == null)
                continue;

            AnimationClip[] animationClips = AssetDatabase.LoadAllAssetRepresentationsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .ToArray();

            if (animationClips.Length == 0)
                continue;

            foreach (AnimationClip clip in animationClips)
            {
                string newClipPath = Path.Combine(extractionPath, $"{Path.GetFileNameWithoutExtension(fbxPath)}_{clip.name}.anim");
                AnimationClip newClip = new AnimationClip();
                EditorUtility.CopySerialized(clip, newClip);
                AssetDatabase.CreateAsset(newClip, newClipPath);
                extractedClips[clip.name] = newClip;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Extracted all animation clips from FBX files.");
        return extractedClips;
    }

    private void ReplaceAnimationReferences(Dictionary<string, AnimationClip> extractedClips)
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            // Remove missing scripts from prefab before processing
            RemoveMissingScripts(prefab);

            ReplaceClipsInGameObject(prefab, extractedClips);
            PrefabUtility.SavePrefabAsset(prefab);
        }

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EditorSceneManager.OpenScene(path);

            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                RemoveMissingScripts(root);
                ReplaceClipsInGameObject(root, extractedClips);
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Replaced all animation clip references.");
    }

    private void ReplaceClipsInGameObject(GameObject obj, Dictionary<string, AnimationClip> extractedClips)
    {
        Animator animator = obj.GetComponent<Animator>();
        if (animator != null)
        {
            AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
            if (controller != null)
            {
                foreach (var layer in controller.layers)
                {
                    ReplaceClipsInStateMachine(layer.stateMachine, extractedClips);
                }
            }
        }

        Animation animation = obj.GetComponent<Animation>();
        if (animation != null)
        {
            ReplaceClipsInAnimationComponent(animation, extractedClips);
        }

        foreach (Transform child in obj.transform)
        {
            ReplaceClipsInGameObject(child.gameObject, extractedClips);
        }
    }

    private void ReplaceClipsInStateMachine(AnimatorStateMachine stateMachine, Dictionary<string, AnimationClip> extractedClips)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.motion is AnimationClip clip && extractedClips.ContainsKey(clip.name))
            {
                state.state.motion = extractedClips[clip.name];
            }
        }

        foreach (var subStateMachine in stateMachine.stateMachines)
        {
            ReplaceClipsInStateMachine(subStateMachine.stateMachine, extractedClips);
        }
    }

    private void ReplaceClipsInAnimationComponent(Animation animation, Dictionary<string, AnimationClip> extractedClips)
    {
        foreach (AnimationState state in animation)
        {
            if (extractedClips.ContainsKey(state.clip.name))
            {
                AnimationClip newClip = extractedClips[state.clip.name];
                animation.RemoveClip(state.clip);
                animation.AddClip(newClip, newClip.name);
            }
        }
    }

    private void RemoveMissingScripts(GameObject obj)
    {
        Component[] components = obj.GetComponentsInChildren<Component>(true);
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty prop = serializedObject.FindProperty("m_Component");

        int r = 0;
        for (int j = 0; j < components.Length; j++)
        {
            if (components[j] == null)
            {
                prop.DeleteArrayElementAtIndex(j - r);
                r++;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
