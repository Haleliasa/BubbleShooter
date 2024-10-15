#nullable enable

using System.Threading.Tasks;
using UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    [SerializeField]
    private Canvas canvas = null!;

    [SerializeField]
    private Dialog<bool>? dialogPrefab;

    [Scene]
    [SerializeField]
    private string gameScene = null!;

    [Scene]
    [SerializeField]
    private string aboutScene = null!;

    private IObjectPool<Dialog<bool>>? dialogPool;
    private IObjectPool<DialogButton<bool>>? dialogButtonPool;

    public void Init(
        IObjectPool<Dialog<bool>>? dialogPool = null,
        IObjectPool<DialogButton<bool>>? dialogButtonPool = null) {
        this.dialogPool = dialogPool;
        this.dialogButtonPool = dialogButtonPool;
    }

    public void StartNewGame() {
        SceneManager.LoadScene(this.gameScene);
    }

    public void GoAbout() {
        SceneManager.LoadScene(this.aboutScene);
    }

    public void Quit() {
        QuitInternal().FireAndForget();
    }

    private async Task QuitInternal() {
        bool result = await Dialog.Show(
            this.dialogPool,
            this.dialogPrefab,
            "Quit",
            "Are you sure?",
            Dialog.YesNoOptions(),
            this.canvas.transform,
            buttonPool: this.dialogButtonPool).Result;
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
