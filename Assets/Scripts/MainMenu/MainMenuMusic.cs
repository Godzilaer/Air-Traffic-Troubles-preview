using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuMusic : MonoBehaviour {
    private AudioSource audioS;
    private bool hasStartedPlaying;

    private void Start() {
        DontDestroyOnLoad(gameObject);

        audioS = GetComponent<AudioSource>();
        audioS.Play();
        //StartCoroutine(FadeIn());

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene) {
        if (newScene.buildIndex > 2) {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            Destroy(gameObject);
            //StartCoroutine(FadeOut());
            
            return;
        }

        hasStartedPlaying = true;

        foreach (MainMenuMusic instance in FindObjectsOfType<MainMenuMusic>()) {
            if (!instance.hasStartedPlaying) {
                Destroy(instance);
                break;
            }
        }
    }

    /*
    private IEnumerator FadeIn() {
        while(audioS.volume < 1f) {
            audioS.volume += 0.01f;

            yield return null;
        }
    }

    private IEnumerator FadeOut() {
        while (audioS.volume > 0f) {
            audioS.volume -= 0.01f;

            //yield return new WaitForSeconds(0.03f);
            yield return null;
        }
        print("after");
        Destroy(gameObject);
    }
    */
}
