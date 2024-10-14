using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SelectionPropertyAttribute), useForChildren: true)]
public class SelectionPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        SelectionPropertyAttribute attr = (SelectionPropertyAttribute)attribute;
        (string[] titles, object[] values) = attr.GetOptions(property);
        int selectedIndex = Array.IndexOf(values, property.boxedValue);
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, titles);
        if (selectedIndex >= 0) {
            try {
                property.boxedValue = values[selectedIndex];
            } catch {
                property.boxedValue = null;
            }
        } else {
            property.boxedValue = null;
        }
        property.serializedObject.ApplyModifiedProperties();
    }
}
