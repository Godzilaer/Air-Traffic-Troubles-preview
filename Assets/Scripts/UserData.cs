using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class UserData {
    public static UserData data { get; private set; }
    private static readonly string filePath = Path.Combine(Application.persistentDataPath, "settings.json");

    public Settings settings = new Settings();
    public LevelCompletion levelCompletion = new LevelCompletion();

    [System.Serializable]
    public class Settings {
        [System.Serializable]
        public class Keybinds {
            public KeyCode deselectPlane = KeyCode.Q;
            public KeyCode deleteAllSelectedPlaneWaypoints = KeyCode.C;
            //public KeyCode deleteWaypoint = KeyCode.E;
        }

        public Keybinds keybinds = new Keybinds();

        public float soundFxVolume = 1f; //Not implemented
        public float musicVolume = 1f; //Not implemented
        public float volume = 1f; //Not implemented
        public bool enableDifferentPlanePathColors = true; //Not implemented
    }

    public class LevelCompletion {
        public Dictionary<int, LevelInfo> completedLevelInfo = new Dictionary<int, LevelInfo>();

        public class LevelInfo {
            public float highScore;
            public string highScoreAchievedOnDifficulty;

            public LevelInfo(float highScore, string difficulty) {
                this.highScore = highScore;
                this.highScoreAchievedOnDifficulty = difficulty;
            }
        }
    }

    public static void Initialize() {
        if (File.Exists(filePath)) {
            Load();
            return;
        }

        data = new UserData();
    }

    private static void Load() {
        using (StreamReader reader = new StreamReader(filePath)) {
            data = JsonUtility.FromJson<UserData>(reader.ReadToEnd());
        }
    }

    public static void Save() {
        using (StreamWriter writer = new StreamWriter(filePath)) {
            string json = JsonUtility.ToJson(data);
            Debug.Log(json);
            Debug.Log(filePath);
            writer.Write(json);
        }
    }

    public static bool CanUserAccessLevel(int id) {
        return id == 1 || data.levelCompletion.completedLevelInfo.ContainsKey(id-1);
    }

    public static void LevelCompleted(int id, float highScore, string difficulty) {
        LevelCompletion.LevelInfo newLevelInfo = new LevelCompletion.LevelInfo(highScore, difficulty);
        data.levelCompletion.completedLevelInfo[id] = newLevelInfo;        
    }
}
