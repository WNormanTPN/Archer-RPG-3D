using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class InspectorGroupDrawer : UnityEditor.Editor
    {
        private List<KeyValuePair<string, List<SerializedProperty>>> groupProperties;
        private Dictionary<string, bool> foldoutStates;

        private void OnEnable()
        {
            foldoutStates = new Dictionary<string, bool>();
            groupProperties = new List<KeyValuePair<string, List<SerializedProperty>>>();
            groupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>()));

            var lastGroup = new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>());
            
            var obj = serializedObject;
            var iterator = obj.GetIterator();

            while (iterator.NextVisible(true))
            {
                if (iterator.name == "m_Script") continue;

                var groupAttribute = (InspectorGroupAttribute)iterator.GetAttribute<InspectorGroupAttribute>();
                var firstGroupAttribute = iterator.GetAttribute<FirstGroupAttribute>();
                var lastGroupAttribute = iterator.GetAttribute<LastGroupAttribute>();
                var nonGroupAttribute = iterator.GetAttribute<NonGroupAttribute>();

                if (firstGroupAttribute != null)
                {
                    groupProperties[0].Value.Add(iterator.Copy());
                }
                else if (lastGroupAttribute != null)
                {
                    lastGroup.Value.Add(iterator.Copy());
                }
                else if (nonGroupAttribute != null)
                {
                    if(groupProperties[groupProperties.Count - 1].Key != "")
                        groupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>()));
                    groupProperties[groupProperties.Count - 1].Value.Add(iterator.Copy());
                }
                else if (groupAttribute != null)
                {
                    var groupIndex = GetGroupIndex(groupAttribute.groupName);
                    if (groupIndex == -1)
                    {
                        groupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>(groupAttribute.groupName, new List<SerializedProperty>()));
                        groupIndex = groupProperties.Count - 1;
                        foldoutStates[groupAttribute.groupName] = false;
                    }
                    groupProperties[groupIndex].Value.Add(iterator.Copy());
                }
                else
                {
                    groupProperties[groupProperties.Count - 1].Value.Add(iterator.Copy());
                }
            }
            groupProperties.Add(lastGroup);
        }
        
        public int GetGroupIndex(string groupName)
        {
            for (int i = 0; i < groupProperties.Count; i++)
            {
                if (groupProperties[i].Key == groupName)
                {
                    return i;
                }
            }
            return -1;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var group in groupProperties)
            {
                bool isFoldedOut = !string.IsNullOrEmpty(group.Key) && foldoutStates.ContainsKey(group.Key) ? foldoutStates[group.Key] : true;
            
                if (!string.IsNullOrEmpty(group.Key))
                {
                    isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, group.Key);
                    foldoutStates[group.Key] = isFoldedOut;
                    EditorGUI.indentLevel++;
                }
            
                if (isFoldedOut)
                {
                    foreach (var property in group.Value)
                    {
                        if(property.propertyType == SerializedPropertyType.ArraySize)
                            continue;
                        EditorGUILayout.PropertyField(property, true);
                    }
                }
                if (!string.IsNullOrEmpty(group.Key))
                    EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    public static class SerializedPropertyExtensions
    {
        public static T GetAttribute<T>(this SerializedProperty property) where T : PropertyAttribute
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var field = targetType.GetField(property.propertyPath, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                var attribute = field.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                return (T)attribute;
            }
            return null;
        }
    }



    

    public class InspectorGroupAttribute : PropertyAttribute
    {
        public string groupName;

        public InspectorGroupAttribute(string groupName)
        {
            this.groupName = groupName;
        }
    }


    public class NonGroupAttribute : PropertyAttribute { }

    public class FirstGroupAttribute : PropertyAttribute { }

    public class LastGroupAttribute : PropertyAttribute { }

}
