using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour {
    public void ReturnToTitleScreen() {
        SceneManager.LoadScene("TitleScreen");
    }

    private void EnterLevel(int levelNum) {
        SceneManager.LoadScene("Level" + levelNum);
    }
}