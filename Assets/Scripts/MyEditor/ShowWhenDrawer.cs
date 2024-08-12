using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MyEditor
{
    [CustomPropertyDrawer(typeof(ShowWhenAttribute), true)]
    public class ShowWhenDrawer : PropertyDrawer
    {
        
        private static Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowWhenAttribute showWhen = (ShowWhenAttribute)attribute;
            SerializedProperty targetProperty = property.serializedObject.FindProperty(showWhen.property);

            if (targetProperty != null && IsConditionMet(targetProperty, showWhen.equal))
            {
                    EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowWhenAttribute showWhen = (ShowWhenAttribute)attribute;
            SerializedProperty targetProperty = property.serializedObject.FindProperty(showWhen.property);

            if (targetProperty != null && IsConditionMet(targetProperty, showWhen.equal))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return 0;
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
    }

    public class ShowWhenAttribute : PropertyAttribute
    {
        public string property { get; }
        public object equal { get; }

        public ShowWhenAttribute(string property, object equal)
        {
            this.property = property;
            this.equal = equal;
        }
    }
}
