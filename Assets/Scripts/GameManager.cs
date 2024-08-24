using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Values")]
    public bool gameOver;

    [SerializeField]
    private float planeSpawnCooldown;

    [Header("Objects")]
    [SerializeField]
    private Transform[] allowedPlanes;
    [SerializeField]
    private Transform planeHolder;
    [SerializeField]
    private GameObject explosion;

    private GameObject selectedPlane;

    private enum SpawnPosition
    {
       Takeoff, RadarEdge, Gate
    };

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
                selectedPlane.GetComponent<PlaneUI>().Selected();
            }
        }
    }

    public void DeselectPlane()
    {
        //If the player tries to deselect a plane when none is selected just ignore it
        if (!selectedPlane) { return; }

        //Runs the Deselected function in the plane
        selectedPlane.GetComponent<PlaneUI>().Deselected();
        selectedPlane = null;
    }

    //Temporary function for testing
    public void TempSpawn()
    {
        SpawnPlane(SpawnPosition.RadarEdge);
    }

    public void GameOver(bool aircraftCollision, Vector2 collisionPos)
    {
        //In a collision both planes will call this function
        //This ensures that this function is only run once
        if (gameOver) { return; }
        gameOver = true;

        //If the game ended due to an aircraft collision
        if (aircraftCollision)
        {
            //Spawn and play explosion effect at aircraft collision position
            GameObject newExplosion = Instantiate(explosion, collisionPos, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();
        }

        //To be added: Code that shows some kind of after-game screen with score, time, planes serviced, etc...
    }

    private void SpawnPlane(SpawnPosition spawnPos)
    {
        //Chooses random plane from allowed planes
        Transform chosenPlane = allowedPlanes[Random.Range(0, allowedPlanes.Length)];

        if(spawnPos == SpawnPosition.RadarEdge)
        {
            Transform newPlane = Instantiate(chosenPlane, planeHolder);

            //Rotates plane randomly
            newPlane.Rotate(0f, 0f, Random.Range(0f, 359f));
            //Moves plane out of bounds backwards relative to the random rotation
            //This means that the plane is now spawned outside the radar view and facing 0, 0
            newPlane.Translate(-transform.up * 6f);
            //Another random rotation so planes can face directions other than 0, 0
            newPlane.Rotate(0f, 0f, Random.Range(-40f, 40f));
        }
    }

    //Continuously spawns planes after planeSpawnCooldown seconds
    IEnumerator PlaneSpawnLoop()
    {
        while(!gameOver)
        {
            SpawnPlane(SpawnPosition.RadarEdge);
            yield return new WaitForSecondsRealtime(planeSpawnCooldown);
        }
    }
}
