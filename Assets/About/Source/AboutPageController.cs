using UnityEngine;
using UnityEngine.SceneManagement;

public class AboutPageController : MonoBehaviour {
    [SerializeField]
    private string socialUrl;

    [Scene]
    [SerializeField]
    private string menuScene;

    public void GoSocial() {
        Application.OpenURL(this.socialUrl);
    }

    public void GoMenu() {
        SceneManager.LoadScene(this.menuScene);
    }
}
