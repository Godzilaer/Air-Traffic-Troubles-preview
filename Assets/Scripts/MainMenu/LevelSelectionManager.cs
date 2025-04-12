using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionManager : MonoBehaviour {
    [SerializeField] private Level[] levels;
    [Header("Objects")]
    [SerializeField] private Transform levelHolder;
    [SerializeField] private GameObject levelObject;

    private class Level {
        public int id;
        public float highScore;
        //Easy, Medium, Hard
        public float baseSpawnRate;
    }

    private void Awake() {
        UserData.Initialize();
    }

    private void Start() {
        int maxY = Mathf.CeilToInt(levels.Length / 3f);

        for(int y = 0; y < maxY; y++) {
            //Max of 3 horizontal
            for(int x = 0; x < Mathf.Min(levels.Length - y * 3, 3); x++) {
                Transform newLevel = Instantiate(levelObject, levelHolder).transform;
                newLevel.position = new Vector2(-380f + x * 380f, y * 350f);
            }
        }
        
    }

    public void ReturnToTitleScreen() {
        SceneManager.LoadScene("TitleScreen");
    }

    private void EnterLevel(int levelNum) {
        SceneManager.LoadScene("Level" + levelNum);
    }
}