using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private Dialog<bool> dialogPrefab;

    [Scene]
    [SerializeField]
    private string gameScene;

    [Scene]
    [SerializeField]
    private string aboutScene;

    public void StartNewGame() {
        SceneManager.LoadScene(this.gameScene);
    }

    public void GoAbout() {
        SceneManager.LoadScene(this.aboutScene);
    }

    public async void Quit() {
        bool result = await Dialog.Show(
            this.dialogPrefab,
            "Quit",
            "Are you sure?",
            Dialog.YesNoOptions(),
            this.canvas.transform).Result;
        if (!result) {
            return;
        }
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
