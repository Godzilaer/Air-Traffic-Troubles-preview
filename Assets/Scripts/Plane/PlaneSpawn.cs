using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaneSpawn : MonoBehaviour {
    [SerializeField] private Transform[] planesToSpawn;
    [SerializeField] private Transform planeHolder;
    [SerializeField] private float planeSpawnCooldown;

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
            yield return new WaitForSecondsRealtime(planeSpawnCooldown);
        }
    }

    //Temporary function for testing
    public void TempSpawn() {
        SpawnPlane(Area.RadarEdge);
    }

    private void SpawnPlane(Area spawnArea) {
        Transform chosenPlane = planesToSpawn[Random.Range(0, planesToSpawn.Length)];
        Transform newPlane = Instantiate(chosenPlane, planeHolder);

        //Rotates plane randomly
        newPlane.Rotate(0f, 0f, Random.Range(0f, 360f));
        //Moves plane out of 
        //This means that the plane is now spawned outside the radar view and facing 0, 0
        newPlane.Translate(-transform.up * GameManager.radarSpawnRadius);
        //Another random rotation so planes can face directions other than 0, 0
        newPlane.Rotate(0f, 0f, Random.Range(-40f, 40f));
    }
}
