using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    private void Awake() {
        Application.targetFrameRate = 60;
    }

    public void EnterGame() {
        SceneManager.LoadScene(1);
    }
}
