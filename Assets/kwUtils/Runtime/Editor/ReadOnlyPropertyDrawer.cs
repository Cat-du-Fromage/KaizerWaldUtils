using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KWUtils
{
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    public class ReadOnlyFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
#endif
}
