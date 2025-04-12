using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour {
    public static LevelSelectionManager Instance { get; private set; }

    [SerializeField] private Level.LevelSettings[] levelSettings;

    [Header("Objects")]
    [SerializeField] private Transform levelHolder;
    [SerializeField] private GameObject levelObject;

    [Header("Level Info")]
    [SerializeField] private TextMeshProUGUI levelTitle;
    [SerializeField] private GameObject infoHolder;

    [Header("High Score")]
    [SerializeField] private TextMeshProUGUI levelIncompleteText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Planes Used")]
    [SerializeField] private Transform planesUsedHolder;

    [Header("Play")]
    [SerializeField] private Dropdown difficultySelection;

    private void Awake() {
        if(Instance != null) {
            Destroy(this);
        } else {
            Instance = this;
        }

        UserData.Initialize();
    }

    private void Start() {
        int maxY = Mathf.CeilToInt(levelSettings.Length / 3f);

        for(int y = 0; y < maxY; y++) {
            //Max of 3 horizontal
            for(int x = 0; x < Mathf.Min(levelSettings.Length - y * 3, 3); x++) {
                int id = y * 3 + x + 1;

                Transform newLevel = Instantiate(levelObject, levelHolder).transform;
                newLevel.GetComponent<RectTransform>().anchoredPosition = new Vector2(-380f + x * 380f, 300f + y * -350f);

                newLevel.GetComponent<Level>().id = id;
            }
        }  
    }

    public void LevelClicked(int id) {
        print("clicked " + id);
    }

    public void ReturnToTitleScreen() {
        SceneManager.LoadScene("TitleScreen");
    }

    private void EnterLevel(int levelNum) {
        SceneManager.LoadScene("Level" + levelNum);
    }
}