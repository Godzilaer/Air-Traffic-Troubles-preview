using System.Collections;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public static bool gameOver { get; private set; }
    public static int score { get; private set; }
    public static int aircraftServed { get; private set; }
    public static int delayStrikes { get; private set; }
    public static float time { get; private set; }

    public static float radarSpawnRadius { get; private set; }
    public float radarRadiusConstant;

    //For tutorial
    public static event Action planeSelectedEvent;
    public static event Action planeLandedEvent;

    [Header("Objects")]
    [SerializeField] private GameObject explosion;
    [SerializeField] private Transform radarBackground;

    [SerializeField] private CameraControl cameraControl;

    [Header("Set Values")]
    public bool isTutorial;
    [SerializeField] private int maxDelayStrikes;

    public static GameObject selectedPlane { get; private set; }

    public enum GameOverType {
        Collision,
        Fuel,
        Delays,
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        Application.targetFrameRate = 60;
        Time.timeScale = 1f;

        gameOver = false;
        score = 0;
        aircraftServed = 0;
        delayStrikes = 0;
        time = 0;
        radarSpawnRadius = radarRadiusConstant * radarBackground.localScale.x;

        UserData.Initialize();
    }
    private void Update() {
        if (gameOver) { return; }

        time = Time.time;

        //Left MB clicked and no plane already selected then attempt to select a plane
        if (Input.GetMouseButtonDown(0)) {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

            //If a collider was hit and if its the PlaneClickBox then select the plane
            if (hit && hit.CompareTag("PlaneClickBox")) {
                GameObject oldSelected = selectedPlane;
                GameObject newPlane = hit.transform.parent.gameObject;
                PlaneControl newPlaneControl = newPlane.GetComponent<PlaneControl>();

                DeselectPlane();

                //If the clicked plane is not the plane that is already selected and the plane is in the air
                if (newPlane != oldSelected && !newPlaneControl.planeData.onGround) {
                    selectedPlane = newPlane;
                    newPlaneControl.OnSelect();

                    if (isTutorial && TutorialManager.waitingForPlaneSelection) {
                        planeSelectedEvent();
                    }
                }
            }
        }

        if (selectedPlane) {
            UpdateSelectedPlane();
        }
    }

    private void UpdateSelectedPlane() {
        var planeControl = selectedPlane.GetComponent<PlaneControl>();

        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        if (!planeControl.planeData.onGround) {
            //Delete one waypoint
            //if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteWaypoint)) {
            if (Input.GetMouseButtonDown(2)) {
                planeControl.DeleteClosestWaypointToMousePos(mouseWorldPos);
            }

            //Delete all waypoints
            if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteAllSelectedPlaneWaypoints)) {
                planeControl.DeleteAllWaypoints();
            }
        }

        if(!Input.GetMouseButtonDown(0) || planeControl.planeData.routedToRunway) { return; }

        
        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);
        bool hitRadarArea = false;

        foreach (Collider2D hit in hits) {
            if(hit.CompareTag("RunwayZone")) {
                Transform currentRunwayTransform = hit.transform;
                Runway currentRunway = currentRunwayTransform.GetComponent<Runway>();

                Vector2 currentRunwayZonePos = currentRunwayTransform.position;
                Vector2 oppositeRunwayZonePos = currentRunway.oppositeRunway.position;

                planeControl.AddWaypoint(Waypoint.Type.Approach, currentRunwayTransform.parent.position + currentRunwayTransform.up * planeControl.planeData.finalDistance);
                planeControl.AddWaypoint(Waypoint.Type.Transition, currentRunwayZonePos);
                planeControl.AddWaypoint(Waypoint.Type.Terminus, oppositeRunwayZonePos);

                planeControl.planeData.routedToRunway = true;

                return;
            }

            if (hit.CompareTag("RadarArea")) {
                hitRadarArea = true;
            }
        }

        if(hitRadarArea) {
            planeControl.AddWaypoint(Waypoint.Type.Path, mouseWorldPos);
        }
    }

    public void DeselectPlane() {
        if (!selectedPlane) { return; }

        selectedPlane.GetComponent<PlaneControl>().OnDeselect();
        selectedPlane = null;
    }

    public void PlaneLanded(float delay) {
        aircraftServed += 1;

        //+15 delay = 15 score 
        //-5 delay = 0 score
        int scoreToAdd = Mathf.RoundToInt(Mathf.Min(Mathf.Max(0.75f * delay + 3.75f, 0f), 15f));
        score += scoreToAdd;

        StartCoroutine(UIManager.Instance.ScoreAddedVisual(scoreToAdd));

        if(isTutorial) {
            planeLandedEvent();
        }
    }

    public IEnumerator GameOver(Vector2 posToZoom, GameOverType type) {
        if (gameOver) { yield break; }
        gameOver = true;

        if (type == GameOverType.Collision) {
            //Spawn and play explosion effect at aircraft collision position
            GameObject newExplosion = Instantiate(explosion, posToZoom, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();

            AudioManager.Instance.PlayExplosionSound();
        }

        //Wait until camera has finished animation
        yield return cameraControl.FocusOnTarget(posToZoom);
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOverScreen(type);
    }

    public void AddDelayStrike(Vector2 pos) {
        delayStrikes++;

        if (delayStrikes >= maxDelayStrikes) {
            StartCoroutine(GameOver(pos, GameOverType.Delays));
        }
    }
}