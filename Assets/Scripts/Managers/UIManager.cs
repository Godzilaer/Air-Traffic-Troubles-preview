using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI aircraftServedText;
    [SerializeField] private TextMeshProUGUI delayStrikesText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuHolder;

    [Header("GameOverScreen")]
    [SerializeField] private GameObject gameOverScreenHolder;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    private void Update() {
        scoreText.text = GameManager.score.ToString();
        aircraftServedText.text = "Aircraft Served: " + GameManager.aircraftServed.ToString();
        delayStrikesText.text = "Delay Strikes: " + GameManager.delayStrikes.ToString() + "/3";
        timeText.text = GetReadableTime();
    }

    public void ShowGameOverScreen() {
        gameOverScreenHolder.SetActive(true);
    }

    public void PauseMenuButton() {
        Time.timeScale = 0f;

        pauseMenuHolder.SetActive(true);
    }

    public void ResumeButton() {
        Time.timeScale = 1f;

        pauseMenuHolder.SetActive(false);
    }

    public void RetryButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string GetReadableTime() {
        var minutes = Mathf.Floor(GameManager.time / 60f);
        string seconds = Mathf.Floor(GameManager.time - 60f * minutes).ToString();

        if (int.Parse(seconds) < 10) {
            seconds = "0" + seconds;
        }

        return "Time: " + minutes + ":" + seconds;
    }
}