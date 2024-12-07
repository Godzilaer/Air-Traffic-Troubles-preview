using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaneSpawn : MonoBehaviour
{
    [SerializeField] private GameManager gm;

    [SerializeField] private Transform[] planesToSpawn;
    [SerializeField] private Transform planeHolder;
    [SerializeField] private float planeSpawnCooldown;
    [SerializeField] private Position[] radarEdgeSpawnPositions;

    private List<Position> usedRadarEdgeSpawnPositions;

    [System.Serializable]
    public class Position
    {
        public Vector2 pos;
        public float rot;
    }

    public enum Area
    {
        Takeoff, RadarEdge, Gate
    };

    private void Start()
    {
        usedRadarEdgeSpawnPositions = new List<Position>();
        StartCoroutine(PlaneSpawnLoop());
    }

    //Continuously spawns planes after planeSpawnCooldown seconds
    IEnumerator PlaneSpawnLoop() {
        while (!gm.gameOver) {
            SpawnPlane(Area.RadarEdge);
            yield return new WaitForSecondsRealtime(planeSpawnCooldown);
        }
    }

    //Temporary function for testing
    public void TempSpawn() {
        SpawnPlane(Area.RadarEdge);
    }

    private void SpawnPlane(Area spawnPos) {
        Transform chosenPlane = planesToSpawn[Random.Range(0, planesToSpawn.Length)];
        Position chosenPos = GetSpawnPos(spawnPos);

        Transform newPlane = Instantiate(chosenPlane, planeHolder);
        newPlane.position = chosenPos.pos;
        newPlane.Rotate(0f, 0f, chosenPos.rot + Random.Range(-40f, 40f));
    }

    private Position GetSpawnPos(Area spawnPos)
    {
        if (spawnPos == Area.RadarEdge) {
            var validValues = radarEdgeSpawnPositions.Where(value => !usedRadarEdgeSpawnPositions.Contains(value)).ToArray();

            if(validValues.Length == 0)
            {
                usedRadarEdgeSpawnPositions = new List<Position>();
                validValues = radarEdgeSpawnPositions;
            }

            Position pos = validValues[Random.Range(0, validValues.Length)];
            usedRadarEdgeSpawnPositions.Add(pos);

            return pos;
        //Temp code, no other positions added yet
        } else {
            return new Position();
        }
    }
}
