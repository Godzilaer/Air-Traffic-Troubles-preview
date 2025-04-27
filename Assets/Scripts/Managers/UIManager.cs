using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    public static UIManager Instance { get; private set; }
    public static bool isPauseMenuActive;

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
    [SerializeField] private TextMeshProUGUI gameOverTimeText;
    [SerializeField] private TextMeshProUGUI gameOverDifficultyText;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverAircraftServedText;
    [SerializeField] private TextMeshProUGUI gameOverAvgScorePerAircraft;
    [SerializeField] private TextMeshProUGUI gameOverDelayStrikesText;

    [Header("Controls")]
    [SerializeField] private GameObject controlsText;

    private bool isTimeAlreadyFrozenByTutorial;

    private class FormattedPlaneInfo {
        public string aircraft;
        public string speed;

        public FormattedPlaneInfo(PlaneData.AircraftType aircraftType) {
            switch (aircraftType) {
                case PlaneData.AircraftType.Helicopter:
                    aircraft = "Helicopter";
                    speed = "Slow";
                    break;

                case PlaneData.AircraftType.GeneralAviation:
                    aircraft = "GA";
                    speed = "Slow";
                    break;

                case PlaneData.AircraftType.RegionalJet:
                    aircraft = "Reg. Jet";
                    speed = "Medium";
                    break;

                case PlaneData.AircraftType.DualJet:
                    aircraft = "Dual Jet";
                    speed = "Fast";
                    break;

                case PlaneData.AircraftType.QuadJet:
                    aircraft = "Quad Jet";
                    speed = "Very fast";
                    break;

                default:
                    aircraft = "N/A";
                    speed = "N/A";
                    Debug.LogError($"Aircraft type {aircraftType} is not accounted for in FormattedPlaneInfo (UIManager).");
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

        isPauseMenuActive = false;
    }

    private void Start() {
        scoreMultiplierText.text = $"x{UserData.Instance.levelCompletion.scoreMultiplier:#0.0}";
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (pauseMenuHolder.activeSelf) {
                ResumeButton();
            } else {
                PauseMenuButton();
            }

        }

        scoreText.text = $"Score: {GameManager.score}";
        aircraftServedText.text = $"Aircraft Served: {GameManager.aircraftServed}";
        delayStrikesText.text = $"Delay Strikes: {GameManager.delayStrikes}/3";
        timeText.text = $"Time: {GetReadableTime()}";

        if (GameManager.selectedPlane) {
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

        FormattedPlaneInfo info = new FormattedPlaneInfo(planeData.aircraftType);

        callsignText.text = $"Callsign: {planeData.callsign}";
        aircraftText.text = $"Aircraft: {info.aircraft}";
        aircraftSpeedText.text = $"Speed: {info.speed}";
        routeETAText.text = $"Route ETA: {PlaneRouteETA(planeData)}";
        statusText.text = $"Status: {(planeData.onGround ? "On Ground" : "Airborne")}";
    }

    public void ShowGameOverScreen(GameManager.GameOverType gameOverType) {
        gameOverScreenHolder.SetActive(true);

        gameOverTimeText.text = $"Session lasted for {GetReadableTime()}";
        //Casting to enum gives the string from the int key (ex. 0 to Helicopter)
        gameOverDifficultyText.text = $"Difficulty: {(LevelDifficulty)UserData.Instance.levelCompletion.selectedDifficulty}";
        gameOverScoreText.text = $"Score: {GameManager.score}";
        gameOverAircraftServedText.text = $"Aircraft Served: {GameManager.aircraftServed}";
        float avgScorePerAircraft = GameManager.aircraftServed > 0 ? (float)GameManager.score / GameManager.aircraftServed : 0f;
        gameOverAvgScorePerAircraft.text = $"Average Score per Aircraft: {avgScorePerAircraft: #0.00}/{10f * UserData.Instance.levelCompletion.scoreMultiplier}";
        gameOverDelayStrikesText.text = $"Delay Strikes: {GameManager.delayStrikes}/3";

        gameStatsHolder.SetActive(false);

        string message = null;
        switch (gameOverType) {
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
        isPauseMenuActive = true;

        if(Time.timeScale == 0f) {
            isTimeAlreadyFrozenByTutorial = true;
        } else {
            Time.timeScale = 0f;
            isTimeAlreadyFrozenByTutorial = false;
        }
        

        pauseMenuHolder.SetActive(true);
    }

    public void ResumeButton() {
        isPauseMenuActive = false;

        if (!isTimeAlreadyFrozenByTutorial) {
            Time.timeScale = 1f;
        }

        pauseMenuHolder.SetActive(false);
    }

    public void RetryButton() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnToggleControlsButtonPressed() {
        controlsText.SetActive(!controlsText.activeSelf);
    }

    public void ReturnToLevelSelection(bool saveData) {
        if(GameManager.Instance.isTutorial) {
            SceneManager.LoadScene("TitleScreen");
            return;
        }

        //Pause menu returning to main menu will save data
        //Game over returing to main menu will not because GameManager already did that
        if (saveData) {
            UserData.LevelCompletion.CompleteLevel(UserData.Instance.levelCompletion.selectedLevelId, GameManager.score, UserData.Instance.levelCompletion.selectedDifficulty);
        }

        SceneManager.LoadScene("LevelSelection");
    }

    public void RetryLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private string GetReadableTime() {
        int minutes = Mathf.FloorToInt(Time.time / 60f);
        int seconds = Mathf.FloorToInt(Time.time - 60f * minutes);

        return $"{minutes:#00}:{seconds:#00}";
    }

    private string PlaneRouteETA(PlaneData planeData) {
        float distance = 0f;

        List<Waypoint.Internal> waypoints = planeData.internalWaypoints;
        float speed = planeData.speed;

        if (waypoints.Count == 0) {
            return "N/A";
        }

        if (waypoints[0].type == Waypoint.Type.Terminus && planeData.aircraftType != PlaneData.AircraftType.Helicopter) {
            return "Landed";
        }

        for (int i = 0; i < waypoints.Count; i++) {
            Vector2 p1 = waypoints[i].position;
            Vector2 p2;

            if (i == 0) {
                p2 = GameManager.selectedPlane.transform.Find("Hitbox").position;
            } else {
                if (waypoints[i - 1].type == Waypoint.Type.Transition) {
                    break;
                }

                p2 = waypoints[i - 1].position;
            }

            distance += Vector2.Distance(p1, p2);
        }

        float seconds = distance / speed;
        return $"{Mathf.CeilToInt(seconds)}s";
    }

    public IEnumerator ScoreAddedVisual(float scoreAdded) {
        float roundedScore = Mathf.RoundToInt(scoreAdded);
        scoreAddedText.text = $"+{roundedScore}";

        scoreAddedText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        scoreAddedText.gameObject.SetActive(false);
    }
}