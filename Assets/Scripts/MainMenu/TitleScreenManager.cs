using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour {
    private void Awake() {
        Application.targetFrameRate = 60;
    }

    public void EnterNewScene(string name) {
        SceneManager.LoadScene(name);
    }
}