using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }

    [Header("Game Stats")]
    [SerializeField] private GameObject gameStatsHolder;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI scoreAddedText;
    [SerializeField] private TextMeshProUGUI scoreMultiplierText;
    [SerializeField] private TextMeshProUGUI aircraftServedText;
    [SerializeField] private TextMeshProUGUI delayStrikesText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuHolder;

    [Header("Plane Info")]
    [SerializeField] private GameObject planeInfoHolder;
    [SerializeField] private TextMeshProUGUI callsignText;
    [SerializeField] private TextMeshProUGUI aircraftText;
    [SerializeField] private TextMeshProUGUI aircraftSpeedText;
    [SerializeField] private TextMeshProUGUI routeETAText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("GameOverScreen")]
    [SerializeField] private GameObject gameOverScreenHolder;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverAircraftServedText;
    [SerializeField] private TextMeshProUGUI gameOverAvgScorePerAircraft;
    [SerializeField] private TextMeshProUGUI gameOverDelayStrikesText;
    [SerializeField] private TextMeshProUGUI gameOverTimeText;

    [Header("Controls")]
    [SerializeField] private GameObject controlsText;

    private class FormattedPlaneInfo {
        public string aircraft;
        public string speed;

        public FormattedPlaneInfo(string planeName) {
            int spaceIndex = planeName.IndexOf("(");

            if (spaceIndex >= 0) {
                planeName = planeName.Substring(0, spaceIndex);
            }

            planeName = planeName.Trim();

            switch (planeName) {
                case "GeneralAviation":
                    aircraft = "GA";
                    speed = "Slow";
                    break;

                case "RegionalJet":
                    aircraft =  "Reg. Jet";
                    speed = "Medium";
                    break;

                case "DualJet":
                    aircraft = "Dual Jet";
                    speed = "Fast";
                    break;

                case "QuadJet":
                    aircraft = "Quad Jet";
                    speed = "Very fast";
                    break;

                default:
                    aircraft = "N/A";
                    speed = "N/A";
                    Debug.LogError("Plane Name (" + planeName + ") is not accounted for in FormattedPlaneInfo (UIManager).");
                    break;
            }
        }
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    private void Start() {
        scoreMultiplierText.text = "x" + UserData.Instance.levelCompletion.scoreMultiplier.ToString("0.0");
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(pauseMenuHolder.activeSelf) {
                ResumeButton();
            } else {
                PauseMenuButton();
            }
            
        }

        scoreText.text = "Score: " + GameManager.score.ToString();
        aircraftServedText.text = "Aircraft Served: " + GameManager.aircraftServed.ToString();
        delayStrikesText.text = "Delay Strikes: " + GameManager.delayStrikes.ToString() + "/3";
        timeText.text = "Time: " + GetReadableTime();

        if(GameManager.selectedPlane) {
            planeInfoHolder.SetActive(true);
            UpdatePlaneInfoPanel();
        } else {
            planeInfoHolder.SetActive(false);
            callsignText.text = "No Plane Selected";
        }
    }

    private void UpdatePlaneInfoPanel() {
        GameObject plane = GameManager.selectedPlane;
        PlaneData planeData = plane.GetComponent<PlaneControl>().planeData;

        FormattedPlaneInfo info = new FormattedPlaneInfo(plane.name);

        callsignText.text = "Callsign: " + planeData.callsign;
        aircraftText.text = "Aircraft: " + info.aircraft;
        aircraftSpeedText.text = "Speed: " + info.speed;
        routeETAText.text = "Route ETA: " + PlaneRouteETA(planeData);
        statusText.text = "Status: " + (planeData.onGround ? "On Ground" : "Airborne");
    }

    public void ShowGameOverScreen(GameManager.GameOverType gameOverType) {
        gameOverScreenHolder.SetActive(true);

        gameOverTimeText.text = "Session lasted for " + GetReadableTime();
        gameOverScoreText.text = "Score: " + GameManager.score.ToString();
        gameOverAircraftServedText.text = "Aircraft Served: " + GameManager.aircraftServed.ToString();
        string avgScorePerAircraft = GameManager.aircraftServed > 0 ? (GameManager.score / GameManager.aircraftServed).ToString("F1") : "0";
        gameOverAvgScorePerAircraft.text = "Average Score per Aircraft: " + avgScorePerAircraft + "/15";
        gameOverDelayStrikesText.text = "Delay Strikes: " + GameManager.delayStrikes.ToString() + "/3";

        gameStatsHolder.SetActive(false);

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

    public void OnToggleControlsButtonPressed() {
        controlsText.SetActive(!controlsText.activeSelf);
    }

    public void ReturnToLevelSelection() {
        SceneManager.LoadScene("LevelSelection");
    }

    public void RetryLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string GetReadableTime() {
        int minutes = Mathf.FloorToInt(GameManager.time / 60f);
        int seconds = Mathf.FloorToInt(GameManager.time - 60f * minutes);

        return minutes.ToString("D2") + ":" + seconds.ToString("D2");
    }

    private string PlaneRouteETA(PlaneData planeData) {
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
        return Mathf.CeilToInt(seconds) + "s";
    }

    public IEnumerator ScoreAddedVisual(float scoreAdded) {
        float roundedScore = Mathf.RoundToInt(scoreAdded);
        scoreAddedText.text = "+" + roundedScore;

        scoreAddedText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        scoreAddedText.gameObject.SetActive(false);
    }

    
}