using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
#endif

namespace BubbleShooter.Core {
    public class AddressablesLabelAttribute : SelectionPropertyAttribute {
#if UNITY_EDITOR
        public override (string[], object[]) GetOptions(SerializedProperty property) {
            string[] labels = AddressableAssetSettingsDefaultObject.Settings.GetLabels().ToArray();
            return (labels, labels);
        }
#endif
    }
}
