using UnityEditor;
using UnityEngine;

namespace BubbleShooter.Core {
    public abstract class SelectionPropertyAttribute : PropertyAttribute {
#if UNITY_EDITOR
        public abstract (string[], object[]) GetOptions(SerializedProperty property);
#endif
    }
}
