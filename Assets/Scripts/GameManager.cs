using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("CurrentGameStatistics")]
    public int score;
    public float time;
    public int aircraftServed;
    
    [Header("Values")]
    public bool gameOver;

    [SerializeField] private PlaneSpawn.Position[] radarEdgeSpawnPositions;
    private PlaneSpawn.Position previousRadarEdgeSpawnPos;

    [SerializeField] private float planeSpawnCooldown;

    [Header("Objects")]
    public GameObject selectedPlane;

    [SerializeField] private Transform[] allowedPlanes;
    [SerializeField] private Transform planeHolder;
    [SerializeField] private GameObject explosion;
    
    private struct PlaneSpawn {
        [System.Serializable]
        public class Position {
            public Vector2 pos;
            public float rot;
        }

        public enum Area {
            Takeoff, RadarEdge, Gate
        };
    }

    private void Start()
    {
        StartCoroutine(PlaneSpawnLoop());
    }

    private void Update()
    {
        //Deselect hotkey
        if(Input.GetKeyDown(KeyCode.Q))
        {
            DeselectPlane();
        }

        //Left MB clicked and no plane already selected then attempt to select a plane
        if (Input.GetMouseButtonDown(0) && selectedPlane == null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            //If a collider was hit and if its the PlaneClickBox then select the plane
            if (hit.collider && hit.collider.CompareTag("PlaneClickBox"))
            {
                selectedPlane = hit.collider.transform.parent.gameObject;
                selectedPlane.GetComponent<PlaneControl>().Selected();
            }
        }
    }

    public void DeselectPlane()
    {
        if (!selectedPlane) { return; }

        selectedPlane.GetComponent<PlaneControl>().Deselected();
        selectedPlane = null;
    }

    public void GameOver(bool isAircraftCollision, Vector2 collisionPos)
    {
        //In a collision both planes will call this function
        //This ensures that this function is only run once
        if (gameOver) { return; }
        gameOver = true;

        if (isAircraftCollision)
        {
            //Spawn and play explosion effect at aircraft collision position
            GameObject newExplosion = Instantiate(explosion, collisionPos, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();
        }

        //To be added: Code that shows some kind of after-game screen with score, time, planes serviced, etc...
    }

    public void PlaneLanded() {
        aircraftServed += 1;
        score += 5;
    }

    //Temporary function for testing
    public void TempSpawn() {
        SpawnPlane(PlaneSpawn.Area.RadarEdge);
    }

    private void SpawnPlane(PlaneSpawn.Area spawnPos)
    {
        Transform chosenPlane = allowedPlanes[Random.Range(0, allowedPlanes.Length)];

        if(spawnPos == PlaneSpawn.Area.RadarEdge) {
            int i;

            //Choose random index and make sure it isn't the same as the last one
            do {
                i = Random.Range(0, radarEdgeSpawnPositions.Length);
            } while (radarEdgeSpawnPositions[i] == previousRadarEdgeSpawnPos);

            Transform newPlane = Instantiate(chosenPlane, planeHolder);
            newPlane.position = radarEdgeSpawnPositions[i].pos;
            newPlane.Rotate(0f, 0f, radarEdgeSpawnPositions[i].rot + Random.Range(-40f, 40f));
        }
    }

    //Continuously spawns planes after planeSpawnCooldown seconds
    IEnumerator PlaneSpawnLoop()
    {
        while(!gameOver)
        {
            SpawnPlane(PlaneSpawn.Area.RadarEdge);
            yield return new WaitForSecondsRealtime(planeSpawnCooldown);
        }
    }
}