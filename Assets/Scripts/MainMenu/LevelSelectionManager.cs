using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour {
    public static LevelSelectionManager Instance { get; private set; }

    //Level 1 at position 0
    [SerializeField] private Level.LevelSettings[] levelSettings;
    [SerializeField] private float[] difficultyScoreMultipliers;
    [SerializeField] private float[] difficultySpawnRateMultipliers;

    [Header("Objects")]
    [SerializeField] private Transform levelHolder;
    [SerializeField] private GameObject levelObject;

    [Header("Level Info")]
    [SerializeField] private TextMeshProUGUI levelTitle;
    [SerializeField] private GameObject infoHolder;

    [Header("High Score")]
    [SerializeField] private TextMeshProUGUI levelIncompleteText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI achievedOnDifficultyText;

    [Header("Planes Used")]
    [SerializeField] private Transform planesUsedHolder;

    [Header("Difficulty Selection")]
    [SerializeField] private TMP_Dropdown difficultySelectionDropdown;
    [SerializeField] private TextMeshProUGUI difficultyScoreMultiplierText;

    private int selectedLevelId;
    private int selectedDifficulty;

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
        } else {
            Instance = this;
        }

        UserData.Initialize();
    }

    private void Start() {
        int maxY = Mathf.CeilToInt(levelSettings.Length / 3f);

        for (int y = 0; y < maxY; y++) {
            //Max of 3 horizontal
            for (int x = 0; x < Mathf.Min(levelSettings.Length - y * 3, 3); x++) {
                int id = y * 3 + x;

                Transform newLevel = Instantiate(levelObject, levelHolder).transform;
                newLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-380f + x * 380f, 300f + y * -350f);

                newLevel.GetComponent<Level>().id = id;
            }
        }
    }

    public void OnLevelClicked(int id) {
        if (!UserData.LevelCompletion.CanUserAccessLevel(id)) { return; }

        ResetLevelInfo();

        selectedLevelId = id;
        levelTitle.text = "Level " + (id + 1);

        if (UserData.Instance.levelCompletion.completedLevelInfo.ContainsKey(id)) {
            UserData.LevelCompletion.LevelInfo levelInfo = UserData.Instance.levelCompletion.completedLevelInfo[id];
            highScoreText.text = "High Score: " + levelInfo.highScore.ToString();
            achievedOnDifficultyText.text = "Achieved on: " + ((LevelDifficulty)levelInfo.highScoreAchievedOnDifficulty).ToString();


        } else {
            levelIncompleteText.gameObject.SetActive(true);

            highScoreText.gameObject.SetActive(false);
            achievedOnDifficultyText.gameObject.SetActive(false);
        }

        //For ever plane used in this level get the gameobject in LevelInfo and setactive
        foreach (Level.PlaneType planeType in levelSettings[id].usedPlanes) {
            planesUsedHolder.Find(planeType.ToString()).gameObject.SetActive(true);
        }

        print("clicked " + id);
    }

    private void ResetLevelInfo() {
        infoHolder.SetActive(true);

        highScoreText.gameObject.SetActive(true);
        achievedOnDifficultyText.gameObject.SetActive(true);
        levelIncompleteText.gameObject.SetActive(false);

        foreach (Transform plane in planesUsedHolder) {
            plane.gameObject.SetActive(false);
        }
    }

    public void OnDifficultyChanged() {
        int difficulty = difficultySelectionDropdown.value;

        selectedDifficulty = difficulty;
        difficultyScoreMultiplierText.text = "Score x" + difficultyScoreMultipliers[difficulty].ToString("0.0");
    }

    public void OnReturnToTitleScreenPressed() {
        SceneManager.LoadScene("TitleScreen");
    }

    public void OnPlayButtonPressed() {
        UserData.Instance.levelCompletion.selectedLevelId = selectedLevelId;
        UserData.Instance.levelCompletion.selectedDifficulty = selectedDifficulty;
        UserData.Instance.levelCompletion.scoreMultiplier = difficultyScoreMultipliers[selectedDifficulty];
        UserData.Instance.levelCompletion.spawnDelayMultiplier = difficultySpawnRateMultipliers[selectedDifficulty];
        UserData.Save();

        SceneManager.LoadScene("Level" + (selectedLevelId + 1));
    }
}