using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    public void EnterGame() {
        SceneManager.LoadScene(1);
    }
}
