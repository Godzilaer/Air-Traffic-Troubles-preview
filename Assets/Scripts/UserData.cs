using System.IO;
using UnityEngine;

[System.Serializable]
public class UserData {
    public static UserData data { get; private set; }
    private static string filePath = Path.Combine(Application.persistentDataPath, "settings.json");

    public Settings settings = new Settings();

    [System.Serializable]
    public class Settings {
        [System.Serializable]
        public class Keybinds {
            public KeyCode deselectPlane = KeyCode.Q;
            public KeyCode deleteAllSelectedPlaneWaypoints = KeyCode.C;
            public KeyCode deleteWaypoint = KeyCode.E;
        }

        public Keybinds keybinds = new Keybinds();

        public float volume = 1f; //Not implemented
        public bool enableDifferentPlanePathColors = true; //Not implemented
    }

    public class Saves {

    }

    public static void Initialize() {
        if(File.Exists(filePath)) {
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
}
