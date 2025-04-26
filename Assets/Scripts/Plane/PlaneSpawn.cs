using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneSpawn : MonoBehaviour {
    [Header("Objects")]
    [SerializeField] private Transform planeHolder;
    [SerializeField] private Transform radarBlipHolder;
    [SerializeField] private Transform radarBlip;

    private float spawnCooldown;
    private int radarSpawnNum;
    private float radarSpawnOffsetDegrees;

    private List<Transform> planesToSpawn = new List<Transform>();

    private int maxColorSteps = 20;
    private int currentColorStep;

    public enum Area {
        Takeoff, RadarEdge
    };

    private void Start() {
        foreach (PlaneData.AircraftType aircraftType in GameManager.Instance.levelConfig.usedAircraft) {
            planesToSpawn.Add(Resources.Load<Transform>("Aircraft/" + aircraftType.ToString()));
        }

        spawnCooldown = GameManager.Instance.levelConfig.spawnCooldown;

        //Do not automatically spawn planes if the tutorial is active
        if (!GameManager.Instance.isTutorial) {      
            spawnCooldown *= UserData.Instance.levelCompletion.spawnDelayMultiplier;
            StartCoroutine(PlaneSpawnLoop());
        }
    }

    //Continuously spawns planes after planeSpawnCooldown seconds
    public IEnumerator PlaneSpawnLoop() {
        radarSpawnOffsetDegrees = Random.Range(0f, 360f);

        yield return new WaitForSeconds(1f);

        while (!GameManager.gameOver) {
            SpawnPlane(Area.RadarEdge);
            yield return new WaitForSeconds(spawnCooldown);
        }
    }

    public void SpawnPlane(Area spawnArea, PlaneData.AircraftType? chosenAircraftTypeForTutorial = null) {
        //If its the tutorial then chosenAircraftTypeForTutorial is set
        Transform chosenPlane = chosenAircraftTypeForTutorial == null ? planesToSpawn[Random.Range(0, planesToSpawn.Count)] : planesToSpawn[(int)chosenAircraftTypeForTutorial];
        Transform newPlane = Instantiate(chosenPlane, planeHolder);

        switch (spawnArea) {
            //For 3 plane spawns, the degrees change by 90. Then, on the fourth (radarSpawnNum==4), it changes by 23 degrees more than 90 (radarSpawnOffsetDegrees+=23) and repeats.
            case Area.RadarEdge:
                newPlane.position = Vector2.zero;

                //Plane rotated at 0,0
                newPlane.Rotate(0f, 0f, (90f * radarSpawnNum) + radarSpawnOffsetDegrees);
                //Plane moved backwards, outside radar view and is now facing 0, 0
                newPlane.Translate(-transform.up * GameManager.radarSpawnRadius);
                //Another random rotation so planes can face directions other than 0, 0
                newPlane.Rotate(0f, 0f, Random.Range(-30f, 30f));

                radarSpawnNum++;

                if (radarSpawnNum == 4) {
                    radarSpawnNum = 0;
                    radarSpawnOffsetDegrees += 29;
                }

                break;
        }

        PlaneControl planeControl = newPlane.GetComponent<PlaneControl>();

        if (currentColorStep == maxColorSteps) {
            currentColorStep = 0;
        }

        planeControl.planeData.pathColor = Color.HSVToRGB(currentColorStep / (float)maxColorSteps, 1f, 1f);
        currentColorStep++;


        Transform newBlip = Instantiate(radarBlip, radarBlipHolder);
        newBlip.position = (newPlane.position - Vector3.zero).normalized * (GameManager.radarSpawnRadius - 3f);
        newBlip.GetComponent<RadarBlip>().planeSpeed = planeControl.planeData.speed;
    }
}
