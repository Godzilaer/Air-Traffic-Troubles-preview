using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class LevelSelectionManager : MonoBehaviour {
    public static LevelSelectionManager Instance { get; private set; }

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

    [Header("Aircraft Used")]
    [SerializeField] private Transform aircraftUsedHolder;

    [Header("Difficulty Selection")]
    [SerializeField] private TMP_Dropdown difficultySelectionDropdown;
    [SerializeField] private TextMeshProUGUI difficultyScoreMultiplierText;

    //Level 1 at position 0
    private List<LevelConfig> levelConfigs;

    private int selectedLevelId;
    private int selectedDifficulty = 1;

    private void Awake() {
        if (Instance != null) {
            Destroy(this);
        } else {
            Instance = this;
        }

        #if !UNITY_WEBGL
        Application.targetFrameRate = 60;
        #endif
        
        levelConfigs = Resources.LoadAll<LevelConfig>("LevelConfigs").ToList();

        //Remove Tutorial from list because it's not a level
        foreach(LevelConfig lc in levelConfigs) {
            if(lc.name == "Tutorial") {
                levelConfigs.Remove(lc);
                break;
            }
        }

        //Make sure the levelConfigs are in chronological order
        levelConfigs = levelConfigs.OrderBy(asset => int.Parse(asset.name)).ToList();

        UserData.Initialize();
    }

    private void Start() {
        int maxY = Mathf.CeilToInt(levelConfigs.Count / 3f);

        for (int y = 0; y < maxY; y++) {
            //Max of 3 horizontal
            for (int x = 0; x < Mathf.Min(levelConfigs.Count - y * 3, 3); x++) {
                int id = y * 3 + x;

                Transform newLevel = Instantiate(levelObject, levelHolder).transform;
                newLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-380f + x * 380f, 700f + y * -350f);

                newLevel.GetComponent<LevelToSelect>().id = id;
            }
        }
    }

    public void OnLevelClicked(int id) {
        if (!UserData.LevelCompletion.CanUserAccessLevel(id)) { return; }

        ResetLevelInfo();

        selectedLevelId = id;
        levelTitle.text = "Level " + (id + 1);

        if (UserData.Instance.levelCompletion.completedLevelInfo.Count > id) {
            UserData.LevelCompletion.LevelInfo levelInfo = UserData.Instance.levelCompletion.completedLevelInfo[id];
            highScoreText.text = "High Score: " + levelInfo.highScore.ToString();
            achievedOnDifficultyText.text = "Achieved on: " + ((LevelDifficulty)levelInfo.highScoreAchievedOnDifficulty).ToString();


        } else {
            levelIncompleteText.gameObject.SetActive(true);

            highScoreText.gameObject.SetActive(false);
            achievedOnDifficultyText.gameObject.SetActive(false);
        }

        //For ever plane used in this level get the gameobject in LevelInfo and change alpha value to opaque
        foreach (PlaneData.AircraftType planeType in levelConfigs[id].usedAircraft) {
            aircraftUsedHolder.Find(planeType.ToString()).GetComponent<Image>().color = Color.white;
        }
    }

    private void ResetLevelInfo() {
        infoHolder.SetActive(true);

        highScoreText.gameObject.SetActive(true);
        achievedOnDifficultyText.gameObject.SetActive(true);
        levelIncompleteText.gameObject.SetActive(false);

        //Change all the planes back to being almost transparent
        foreach (Transform plane in aircraftUsedHolder) {
            plane.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.2f);
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