using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SceneAttribute : SelectionPropertyAttribute {
#if UNITY_EDITOR
    public override (string[], object[]) GetOptions(SerializedProperty property) {
        string[] scenes = Enumerable.Range(0, SceneManager.sceneCountInBuildSettings)
            .Select(i => SceneUtility.GetScenePathByBuildIndex(i))
            .ToArray();
        return (scenes, scenes);
    }
#endif
}
