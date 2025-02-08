using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI aircraftServedText;
    [SerializeField] private TextMeshProUGUI delayStrikesText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuHolder;

    [Header("Plane Info")]
    [SerializeField] private TextMeshProUGUI callsignText;
    [SerializeField] private TextMeshProUGUI etaText;

    [Header("GameOverScreen")]
    [SerializeField] private GameObject gameOverScreenHolder;
    [SerializeField] private TextMeshProUGUI messageText; 

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

        if(GameManager.selectedPlane) {
            UpdateAircraftInfoPanel();
        } else {
            callsignText.text = "Callsign: N/A";
            etaText.text = "Route ETA: N/A";
        }
        
    }

    private void UpdateAircraftInfoPanel() {
        PlaneData planeData = GameManager.selectedPlane.GetComponent<PlaneControl>().planeData;
        callsignText.text = "Callsign: " + planeData.callsign;
        etaText.text = "Route ETA: " + PlaneRouteAirtimeETA(planeData);
    }

    public void ShowGameOverScreen(GameManager.GameOverType gameOverType) {
        gameOverScreenHolder.SetActive(true);

        string message = null;
        switch(gameOverType) {
            case GameManager.GameOverType.Collision:
                message = "There was an aircraft collision!";
                break;
            case GameManager.GameOverType.Delays:
                message = "3 aircraft experienced significant delays.";
                break;
            case GameManager.GameOverType.Fuel:
                message = "An aircraft ran out of fuel mid-air!";
                break;
        }

        messageText.text = message;
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
        int minutes = Mathf.FloorToInt(GameManager.time / 60f);
        int seconds = Mathf.FloorToInt(GameManager.time - 60f * minutes);

        return "Time: " + minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }

    private string PlaneRouteAirtimeETA(PlaneData planeData) {
        float distance = 0f;

        List<Waypoint.Internal> waypoints = planeData.internalWaypoints;
        float speed = planeData.speed;

        if(waypoints.Count == 0) {
            return "N/A";
        }

        if(waypoints[0].type == Waypoint.Type.Terminus) {
            return "Landed";
        }

        for (int i = 0; i < waypoints.Count; i++) {
            Vector2 p1 = waypoints[i].position;
            Vector2 p2;

            if (i == 0) {
                p2 = GameManager.selectedPlane.transform.Find("Hitbox").position;
            } else {
                if(waypoints[i - 1].type == Waypoint.Type.Transition) {
                    break;
                }

                p2 = waypoints[i - 1].position;
            }

            distance += Vector2.Distance(p1, p2);
        }

        float seconds = distance / speed;

        int minutes = Mathf.FloorToInt(seconds / 60f);
        int correctedSeconds = Mathf.FloorToInt(seconds - minutes * 60f);

        return minutes.ToString("D2") + ":" + correctedSeconds.ToString("D2");
    }
}