using UnityEditor;
using UnityEngine;

public abstract class SelectionPropertyAttribute : PropertyAttribute {
#if UNITY_EDITOR
    public abstract (string[], object[]) GetOptions(SerializedProperty property);
#endif
}
