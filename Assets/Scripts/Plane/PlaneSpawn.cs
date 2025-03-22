using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSpawn : MonoBehaviour {
    [Header("Objects")]
    [SerializeField] private Transform[] planesToSpawn;
    [SerializeField] private Transform planeHolder;
    [SerializeField] private Transform radarBlipHolder;
    [SerializeField] private Transform radarBlip;
    [Header("Plane Spawn Values")]
    [SerializeField] private float spawnCooldown;
    [SerializeField] private float spawnMinimumDistance; //The minimum distance the new plane's spawn location must be from the old plane's spawn location
    [SerializeField] private int maxPreviousSpawnPosListLength; //The maximum length of previousSpawnPositions length
    [Header("Plane Paths")]
    [SerializeField] private int maxColorSteps;
    private int currentColorStep;

    [SerializeField] private List<Vector2> previousSpawnPositions = new List<Vector2>();

    [System.Serializable]
    public class Position {
        public Vector2 pos;
        public float rot;
    }

    public enum Area {
        Takeoff, RadarEdge, Gate
    };

    private void Start() {
        StartCoroutine(PlaneSpawnLoop());
    }

    //Continuously spawns planes after planeSpawnCooldown seconds
    IEnumerator PlaneSpawnLoop() {
        while (!GameManager.gameOver) {
            SpawnPlane(Area.RadarEdge);
            yield return new WaitForSecondsRealtime(spawnCooldown);
        }
    }

    //Temporary function for testing
    public void TempSpawn() {
        SpawnPlane(Area.RadarEdge);
    }

    private void SpawnPlane(Area spawnArea) {
        Transform chosenPlane = planesToSpawn[Random.Range(0, planesToSpawn.Length)];
        Transform newPlane = Instantiate(chosenPlane, planeHolder);

        int crashPrevention = 0;

        do {
            if(crashPrevention > 500) {
                previousSpawnPositions.RemoveAt(0);
                Debug.LogError("Plane Spawn Crash Prevention Triggered!");
            }

            switch (spawnArea) {
                case Area.RadarEdge:
                    newPlane.position = Vector2.zero;

                    //Rotates plane randomly
                    newPlane.Rotate(0f, 0f, Random.Range(0f, 360f));
                    //Moves plane out of 
                    //This means that the plane is now spawned outside the radar view and facing 0, 0
                    newPlane.Translate(-transform.up * GameManager.radarSpawnRadius);
                    //Another random rotation so planes can face directions other than 0, 0
                    newPlane.Rotate(0f, 0f, Random.Range(-40f, 40f));
                    break;
                case Area.Takeoff:
                    break;
            }

            crashPrevention++;
        } while (IsPlanePositionCloseToPrevious(newPlane.position));

        if(previousSpawnPositions.Count >= maxPreviousSpawnPosListLength) {
            previousSpawnPositions.RemoveAt(0);
        }
        
        previousSpawnPositions.Add(newPlane.position);

        PlaneControl planeControl = newPlane.GetComponent<PlaneControl>();

        if (currentColorStep == maxColorSteps) {
            currentColorStep = 0;
        }

        planeControl.planeData.pathColor = Color.HSVToRGB(currentColorStep / (float) maxColorSteps, 1f, 1f);
        currentColorStep++;


        Transform newBlip = Instantiate(radarBlip, radarBlipHolder);
        newBlip.position =  (newPlane.position - Vector3.zero).normalized * (GameManager.radarSpawnRadius - 3f);
        newBlip.GetComponent<RadarBlip>().planeSpeed = planeControl.planeData.speed;
    }

    private bool IsPlanePositionCloseToPrevious(Vector2 planePos) {
        foreach(Vector2 spawnPos in previousSpawnPositions) {
            if(Vector2.Distance(spawnPos, planePos) < spawnMinimumDistance) {
                return true;
            }
        }

        return false;
    }
}
