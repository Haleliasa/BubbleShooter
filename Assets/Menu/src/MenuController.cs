#nullable enable

using System.Threading.Tasks;
using UI.Dialog;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {
    [Scene]
    [SerializeField]
    private string gameScene = null!;

    [Scene]
    [SerializeField]
    private string aboutScene = null!;

    private IDialogService dialogService = null!;

    public void Init(IDialogService dialogService) {
        this.dialogService = dialogService;
    }

    public void StartNewGame() {
        SceneManager.LoadScene(this.gameScene);
    }

    public void GoAbout() {
        SceneManager.LoadScene(this.aboutScene);
    }

    public void Quit() {
        this.QuitInternal().FireAndForget();
    }

    private async Task QuitInternal() {
        bool dialogRes = await this.dialogService.OpenAsync(
            "Quit",
            "Are you sure?",
            DialogOptions.YesNo()
        ).result;

        if (!dialogRes) {
            return;
        }

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
