using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class MonoBehaviourInspector : UnityEditor.Editor
    {
        
        private List<KeyValuePair<string, List<SerializedProperty>>> groupProperties;
        private Dictionary<string, bool> foldoutStates;
        private Dictionary<string, KeyValuePair<bool, bool>> showWhenFoldoutStates;
        private HashSet<string> drawnNestedPaths;

        private void OnEnable()
        {
            foldoutStates = new Dictionary<string, bool>();
            groupProperties = new List<KeyValuePair<string, List<SerializedProperty>>>();
            showWhenFoldoutStates = new Dictionary<string, KeyValuePair<bool, bool>>();
            drawnNestedPaths = new HashSet<string>();
            

            var lastGroup = new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>());

            var obj = serializedObject;
            var iterator = obj.GetIterator();
            string prevGroupName = "";


            var temporaryGroupProperties = new List<KeyValuePair<string, List<SerializedProperty>>>();
            temporaryGroupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>()));

            while (iterator.NextVisible(true))
            {
                if (iterator.name == "m_Script") continue;

                InspectorGroupAttribute groupAttribute = null;
                FirstGroupAttribute firstGroupAttribute = null;
                LastGroupAttribute lastGroupAttribute = null;
                NonGroupAttribute nonGroupAttribute = null;

                try
                {
                    var field = iterator.GetUnderlyingField();
                    if (field != null)
                    {
                        groupAttribute = field.GetCustomAttribute<InspectorGroupAttribute>();
                        firstGroupAttribute = field.GetCustomAttribute<FirstGroupAttribute>();
                        lastGroupAttribute = field.GetCustomAttribute<LastGroupAttribute>();
                        nonGroupAttribute = field.GetCustomAttribute<NonGroupAttribute>();
                    }
                }
                catch (Exception)
                {
                    // ignored
                }

                if (firstGroupAttribute != null)
                {
                    temporaryGroupProperties[0].Value.Add(iterator.Copy());
                    prevGroupName = "first";
                }
                else if (lastGroupAttribute != null)
                {
                    lastGroup.Value.Add(iterator.Copy());
                    prevGroupName = "last";
                }
                else if (nonGroupAttribute != null)
                {
                    if (temporaryGroupProperties[temporaryGroupProperties.Count - 1].Key != "")
                    {
                        temporaryGroupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>()));
                    }
                    temporaryGroupProperties[temporaryGroupProperties.Count - 1].Value.Add(iterator.Copy());
                    prevGroupName = "";
                }
                else if (groupAttribute != null)
                {
                    var groupIndex = GetGroupIndex(groupAttribute.groupName, temporaryGroupProperties);
                    if (groupIndex == -1)
                    {
                        temporaryGroupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>(groupAttribute.groupName, new List<SerializedProperty>()));
                        groupIndex = temporaryGroupProperties.Count - 1;
                        foldoutStates[groupAttribute.groupName] = false;
                    }
                    temporaryGroupProperties[groupIndex].Value.Add(iterator.Copy());
                    prevGroupName = groupAttribute.groupName;
                }
                else
                {
                    AddToLastGroup(iterator.Copy(), prevGroupName, ref temporaryGroupProperties, ref lastGroup);
                }
            }
            temporaryGroupProperties.Add(lastGroup);
            groupProperties = temporaryGroupProperties;
        }
        
        private void AddToLastGroup(SerializedProperty property,
            string groupName,
            ref List<KeyValuePair<string,
                List<SerializedProperty>>> groupProperties,
            ref KeyValuePair<string, List<SerializedProperty>> lastGroup)
        {
            if (groupName == "first")
            {
                groupProperties[0].Value.Add(property);
                return;
            }
            if (groupName == "last")
            {
                lastGroup.Value.Add(property);
                return;
            }
            for (int i = groupProperties.Count - 1; i >= 0; i--)
            {
                if (groupProperties[i].Key == groupName)
                {
                    groupProperties[i].Value.Add(property);
                    return;
                }
            }
            groupProperties.Add(new KeyValuePair<string, List<SerializedProperty>>("", new List<SerializedProperty>()));
            groupProperties[groupProperties.Count - 1].Value.Add(property);
        }


        public int GetGroupIndex(string groupName, List<KeyValuePair<string, List<SerializedProperty>>> groupProperties)
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

            try
            {
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
                        for (int i = 0; i < group.Value.Count; i++)
                        {
                            var property = group.Value[i];

                            // Skip if the property is no longer valid
                            if (property == null)
                                continue;

                            if (property.propertyType == SerializedPropertyType.ArraySize)
                                continue;

                            if (IsPartOfNested(property.propertyPath))
                            {
                                if (!drawnNestedPaths.Contains(property.propertyPath))
                                {
                                    drawnNestedPaths.Add(property.propertyPath);
                                    EditorGUILayout.PropertyField(property, true);
                                }
                            }
                            else
                            {
                                var showWhenAttribute = property.GetAttribute<ShowWhenAttribute>();
                                if (showWhenAttribute != null)
                                {
                                    var targetProperty = serializedObject.FindProperty(showWhenAttribute.property);
                                    if (targetProperty != null && IsConditionMet(targetProperty, showWhenAttribute.equal))
                                    {
                                        if (showWhenAttribute.groupName != string.Empty)
                                        {
                                            string groupName = showWhenAttribute.groupName;
                                            if (!showWhenFoldoutStates.ContainsKey(groupName))
                                            {
                                                showWhenFoldoutStates[groupName] = new KeyValuePair<bool, bool>(true, EditorGUILayout.Foldout(false, groupName));
                                            }
                                            var isDrawn = showWhenFoldoutStates[groupName].Key;
                                            var isFolded = showWhenFoldoutStates[groupName].Value;

                                            if (!isDrawn)
                                            {
                                                showWhenFoldoutStates[groupName] = new KeyValuePair<bool, bool>(true, EditorGUILayout.Foldout(isFolded, groupName));
                                            }

                                            if (isFolded)
                                            {
                                                EditorGUI.indentLevel++;
                                                EditorGUILayout.PropertyField(property, true);
                                                EditorGUI.indentLevel--;
                                            }
                                        }
                                        else
                                        {
                                            EditorGUILayout.PropertyField(property, true);
                                        }
                                    }
                                }
                                else
                                {
                                    EditorGUILayout.PropertyField(property, true);
                                }
                            }
                        }
                    }

                    ResetShowWhenFoldoutStates();
                    if (!string.IsNullOrEmpty(group.Key))
                        EditorGUI.indentLevel--;
                }
            }
            catch (Exception _)
            {
                // ignored
            }

            serializedObject.ApplyModifiedProperties();
        }



        private void ResetShowWhenFoldoutStates()
        {
            var needResets = new List<string>();
            foreach (var group in showWhenFoldoutStates)
            {
                if (group.Value.Key)
                {
                    needResets.Add(group.Key);
                }
            }

            foreach (var group in needResets)
            {
                showWhenFoldoutStates[group] = new KeyValuePair<bool, bool>(false, showWhenFoldoutStates[group].Value);
            }
        }
        
        private bool IsConditionMet(SerializedProperty targetProperty, object equal)
        {
            switch (targetProperty.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    return targetProperty.boolValue.Equals(equal);
                case SerializedPropertyType.Enum:
                    return targetProperty.enumValueIndex.Equals((int)equal);
                case SerializedPropertyType.Integer:
                    return targetProperty.intValue.Equals((int)equal);
                case SerializedPropertyType.Float:
                    return targetProperty.floatValue.Equals((float)equal);
                case SerializedPropertyType.String:
                    return targetProperty.stringValue.Equals(equal);
                default:
                    Debug.LogWarning("Unsupported property type in ShowWhen attribute.");
                    return false;
            }
        }
        
        private bool IsPartOfNested(string propertyPath)
        {
            return propertyPath.Contains(".");
        }
    }

    public static class SerializedPropertyExtensions
    {
        public static T GetAttribute<T>(this SerializedProperty property) where T : PropertyAttribute
        {
            var targetType = property.serializedObject.targetObject.GetType();
            var field = GetFieldInfoFromPath(targetType, property.propertyPath);

            if (field != null)
            {
                var attribute = field.GetCustomAttributes(typeof(T), false).FirstOrDefault();
                return (T)attribute;
            }
            return null;
        }

        private static FieldInfo GetFieldInfoFromPath(Type type, string path)
        {
            var parts = path.Split('.');
            FieldInfo field = null;

            foreach (var part in parts)
            {
                field = type.GetField(part, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field == null) return null;

                type = field.FieldType;
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    type = type.GetGenericArguments()[0];
                }
            }
            return field;
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

    public class ShowWhenAttribute : PropertyAttribute
    {
        public string property { get; }
        public object equal { get; }
        
        public string groupName { get; }

        public ShowWhenAttribute(string property, object equal, string groupName = "")
        {
            this.property = property;
            this.equal = equal;
            this.groupName = groupName;
        }
    }
}