using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsManager : MonoBehaviour {
    private void Awake() {
        #if !UNITY_WEBGL
        Application.targetFrameRate = 60;
        #endif
    }

    public void OnReturnToTitleScreenPressed() {
        SceneManager.LoadScene("TitleScreen");
    }
}
