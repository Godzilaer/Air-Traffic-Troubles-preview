using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "LevelConfig/New")]
public class LevelConfig : ScriptableObject {
    public float spawnCooldown;
    public PlaneData.AircraftType[] usedAircraft;
}