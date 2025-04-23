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
        public SerializableDictionary<int, LevelInfo> completedLevelInfo = new SerializableDictionary<int, LevelInfo>();

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
            //Player can always access Level 1
            if (id == 0) {
                return true;
            }

            //If previous level was never completed then lock level
            if (!Instance.levelCompletion.completedLevelInfo.ContainsKey(id - 1)) {
                return false;
            }

            //If high score in the previous level is below 75 then lock level
            if (Instance.levelCompletion.completedLevelInfo[id - 1].highScore < 75f) {
                return false;
            }

            //Otherwise the level is unlocked
            return true;
        }

        public static void CompleteLevel(int id, int score, int difficulty) {
            LevelInfo newLevelInfo = new LevelInfo(score, difficulty);

            //If level has been completed before
            if (Instance.levelCompletion.completedLevelInfo.ContainsKey(id)) {
                //If this completion has a higher high score then update the completion data
                if (Instance.levelCompletion.completedLevelInfo[id].highScore < score) {
                    Instance.levelCompletion.completedLevelInfo[id] = newLevelInfo;
                }
            } else {
                Instance.levelCompletion.completedLevelInfo[id] = newLevelInfo;
            }
        }
    }

    public static void Initialize() {
        if (File.Exists(filePath)) {
            Load();
            return;
        }

        Instance = new UserData();
    }

    private static void Load() {
        using (StreamReader reader = new StreamReader(filePath)) {
            Instance = JsonUtility.FromJson<UserData>(reader.ReadToEnd());
        }

        Instance.levelCompletion.completedLevelInfo.Load();
    }

    public static void Save() {
        Instance.levelCompletion.completedLevelInfo.Save();

        using (StreamWriter writer = new StreamWriter(filePath)) {
            string json = JsonUtility.ToJson(Instance);
            Debug.Log(json);
            Debug.Log(filePath);
            writer.Write(json);
        }
    }
}

public enum LevelDifficulty {
    Easy, Medium, Hard, Impossible
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue> {
    [Serializable]
    private struct KeyValue {
        public TKey Key;
        public TValue Value;
    }

    [SerializeField] private List<KeyValue> items = new List<KeyValue>();

    public void Load() {
        foreach (var pair in items) {
            this[pair.Key] = pair.Value;
        }
    }

    public void Save() {
        items.Clear();
        foreach (var kvp in this) {
            items.Add(new KeyValue { Key = kvp.Key, Value = kvp.Value });
        }
    }
}