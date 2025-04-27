using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

[Serializable]
public class UserData {
    public static UserData Instance { get; private set; }
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "game-data.important");

    public Settings settings = new Settings();
    public LevelCompletion levelCompletion = new LevelCompletion();

    public static bool allLevelsUnlockedForTesting = false;

    [Serializable]
    public class Settings {
        [Serializable]
        public class Keybinds {
            //public KeyCode deselectPlane = KeyCode.Q;
            public KeyCode deleteAllSelectedPlaneWaypoints = KeyCode.C;
        }

        public Keybinds keybinds = new Keybinds();

        public float soundFxVolume = 1f; //Not implemented
        public float musicVolume = 1f; //Not implemented
        public float volume = 1f; //Not implemented
        public bool enableDifferentPlanePathColors = true; //Not implemented
    }

    [Serializable]
    public class LevelCompletion {
        //Level 1 is at key 0
        public List<LevelInfo> completedLevelInfo = new List<LevelInfo>();

        //These variables are set by LevelSelectionManager when a level is selected and are used by GameManager in the scene of the level
        public int selectedLevelId;
        public int selectedDifficulty;
        public float scoreMultiplier;
        public float spawnDelayMultiplier;

        [Serializable]
        public class LevelInfo {
            public int highScore;
            public int highScoreAchievedOnDifficulty;

            public LevelInfo(int highScore, int difficulty) {
                this.highScore = highScore;
                highScoreAchievedOnDifficulty = difficulty;
            }
        }

        public static bool CanUserAccessLevel(int id) {
            #if UNITY_EDITOR 
                if (allLevelsUnlockedForTesting) { 
                    return true;
                }
            #endif

            //Player can always access Level 1
            if (id == 0) {
                return true;
            }

            //If previous level was never completed then lock level
            if (Instance.levelCompletion.completedLevelInfo.Count <= id - 1) {
                return false;
            }

            //If high score in the previous level is below 75 then lock level
            if (Instance.levelCompletion.completedLevelInfo[id - 1].highScore < 100f) {
                return false;
            }

            //Otherwise the level is unlocked
            return true;
        }

        public static void CompleteLevel(int id, int score, int difficulty) {
            LevelInfo newLevelInfo = new LevelInfo(score, difficulty);

            //If level has been completed before
            if (Instance.levelCompletion.completedLevelInfo.Count > id) {
                //If this completion has a higher high score then update the completion data
                if (Instance.levelCompletion.completedLevelInfo[id].highScore < score) {
                    Instance.levelCompletion.completedLevelInfo[id] = newLevelInfo;
                }
            } else {
                Instance.levelCompletion.completedLevelInfo.Add(newLevelInfo);
            }

            Save();
        }
    }

    public static void Initialize() {
        if (File.Exists(filePath)) {
            Load();

            if (GameManager.Instance && GameManager.Instance.isTutorial) {
                Instance.levelCompletion.selectedDifficulty = 1;
                Instance.levelCompletion.scoreMultiplier = 1f;
                Instance.levelCompletion.spawnDelayMultiplier = 1f;
            }
            return;
        }

        Instance = new UserData();
    }

    private static void Load() {
        using (StreamReader reader = new StreamReader(filePath)) {
            Instance = JsonUtility.FromJson<UserData>(reader.ReadToEnd());
        }
    }

    public static void Save() {
        using (StreamWriter writer = new StreamWriter(filePath)) {
            string json = JsonUtility.ToJson(Instance);
            writer.Write(json);
        }
    }
}