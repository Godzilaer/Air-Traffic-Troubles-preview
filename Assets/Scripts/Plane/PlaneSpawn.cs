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
    private int radarSpawnNum;
    private float radarSpawnOffsetDegrees;
    [Header("Plane Paths")]
    [SerializeField] private int maxColorSteps;
    private int currentColorStep;

    public enum Area {
        Takeoff, RadarEdge
    };

    private void Start() {
        //Do not automatically spawn planes if the tutorial is active
        if (!GameManager.Instance.isTutorial) {
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

    public void SpawnPlane(Area spawnArea, bool forTutorial = false) {
        Transform chosenPlane = forTutorial ? planesToSpawn[1] : planesToSpawn[Random.Range(0, planesToSpawn.Length)];
        Transform newPlane = Instantiate(chosenPlane, planeHolder);

        switch (spawnArea) {
            //For 3 plane spawns, the degrees change by 90. Then, on the fourth (radarSpawnNum==4), it changes by 23 degrees more than 90 (radarSpawnOffsetDegrees+=23) and repeats.
            case Area.RadarEdge:
                newPlane.position = Vector2.zero;

                //Plane rotated at 0,0
                newPlane.Rotate(0f, 0f, (90f * radarSpawnNum) + radarSpawnOffsetDegrees);
                //Plane moved backwards, outside radar view and is now facing 0, 0
                newPlane.Translate(-transform.up * GameManager.radarSpawnRadius);

                if(!forTutorial) {
                    //Another random rotation so planes can face directions other than 0, 0
                    newPlane.Rotate(0f, 0f, Random.Range(-40f, 40f));
                }

                radarSpawnNum++;

                if(radarSpawnNum == 4) {
                    radarSpawnNum = 0;
                    //Prime number so the spawn is more unpredictable
                    radarSpawnOffsetDegrees += 23;
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
