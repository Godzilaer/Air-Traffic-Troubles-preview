using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour {
    private void Awake() {
        #if !UNITY_WEBGL
        Application.targetFrameRate = 60;
        #endif
    }

    public void EnterNewScene(string name) {
        SceneManager.LoadScene(name);
    }
}