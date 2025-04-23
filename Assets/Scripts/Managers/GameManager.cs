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

    [HideInInspector] public LevelConfig levelConfig;

    [Header("Objects")]
    [SerializeField] private GameObject explosion;
    [SerializeField] private Transform radarBackground;

    [SerializeField] private CameraControl cameraControl;

    [Header("Set Values")]
    public bool isTutorial;
    private int maxDelayStrikes = 3;

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

        levelConfig = Resources.Load<LevelConfig>("LevelConfigs/" + (UserData.Instance.levelCompletion.selectedLevelId + 1));
    }

    private void Update() {
        if (gameOver) { return; }

        time = Time.time;

        //Left MB clicked and no plane already selected then attempt to select a plane
        if (Input.GetMouseButtonDown(0)) {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);

            foreach (Collider2D hit in hits) {
                if (hit.CompareTag("PlaneClickBox")) {
                    GameObject oldSelected = selectedPlane;
                    GameObject newPlane = hit.transform.parent.gameObject;
                    PlaneControl newPlaneControl = newPlane.GetComponent<PlaneControl>();

                    DeselectPlane();

                    //Make sure the new plane is not the same as the old plane and that the new plane is on the ground
                    if (newPlane != oldSelected && !newPlaneControl.planeData.onGround) {
                        selectedPlane = newPlane;
                        newPlaneControl.OnSelect();

                        if (isTutorial && TutorialManager.waitingForPlaneSelection) {
                            planeSelectedEvent();
                        }
                    }

                    break;
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
            if (Input.GetKeyDown(UserData.Instance.settings.keybinds.deleteAllSelectedPlaneWaypoints)) {
                planeControl.DeleteAllWaypoints();
            }
        }

        if (!Input.GetMouseButtonDown(0) || planeControl.planeData.routedToRunway) { return; }


        Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);
        bool hitRadarArea = false;
        bool hitPlane = false;

        foreach (Collider2D hit in hits) {
            if (hit.CompareTag("RunwayZone")) {
                Transform currentLandingAreaTransform = hit.transform;
                LandingArea currentLandingArea = currentLandingAreaTransform.GetComponent<LandingArea>();

                //Every aircraft besides a helicopter can land on a LongRunway
                if (currentLandingArea.type == LandingArea.Type.LongRunway && planeControl.planeData.aircraftType == PlaneData.AircraftType.Helicopter) {
                    return;
                }

                //Only GA and RegJet can land on a ShortRunway
                if (currentLandingArea.type == LandingArea.Type.ShortRunway && !(planeControl.planeData.aircraftType == PlaneData.AircraftType.GeneralAviation || planeControl.planeData.aircraftType == PlaneData.AircraftType.RegionalJet)) {
                    return;
                }

                //Only a Helicopter can land on a Helipad
                if (currentLandingArea.type == LandingArea.Type.Helipad && planeControl.planeData.aircraftType != PlaneData.AircraftType.Helicopter) {
                    return;
                }

                Vector2 firstLandingAreaPos = currentLandingAreaTransform.position;
                Vector2 finalLandingAreaPos;

                //If its a helicopter then the final landing pos is the same pos as the landing runway else its the opposite runway
                if (planeControl.planeData.aircraftType == PlaneData.AircraftType.Helicopter) {
                    finalLandingAreaPos = firstLandingAreaPos;
                } else {
                    finalLandingAreaPos = currentLandingArea.oppositeRunway.position;
                }

                if (currentLandingArea.type != LandingArea.Type.Helipad) {
                    planeControl.AddWaypoint(Waypoint.Type.Approach, currentLandingAreaTransform.parent.position + currentLandingAreaTransform.up * planeControl.planeData.finalDistance);
                    planeControl.AddWaypoint(Waypoint.Type.Transition, firstLandingAreaPos);
                }
                planeControl.AddWaypoint(Waypoint.Type.Terminus, finalLandingAreaPos);
                planeControl.planeData.routedToRunway = true;

                return;
            }

            if (hit.CompareTag("RadarArea")) {
                hitRadarArea = true;
            }

            if (hit.CompareTag("PlaneClickBox")) {
                hitPlane = true;
            }
        }

        if (hitRadarArea && !hitPlane) {
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

        //+15 delay = 10 score 
        //-5 delay = 0 score
        int scoreToAdd = Mathf.RoundToInt(Mathf.Min(Mathf.Max(0.75f * delay + 3.75f, 0f), 10f) * UserData.Instance.levelCompletion.scoreMultiplier);
        score += scoreToAdd;

        StartCoroutine(UIManager.Instance.ScoreAddedVisual(scoreToAdd));

        if (isTutorial) {
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

        if (!isTutorial) {
            UserData.LevelCompletion.CompleteLevel(UserData.Instance.levelCompletion.selectedLevelId, score, UserData.Instance.levelCompletion.selectedDifficulty);
            UserData.Save();
        }

        //Wait until camera has finished animation
        yield return cameraControl.FocusOnTarget(posToZoom);
        Time.timeScale = 0f;
        UIManager.Instance.ShowGameOverScreen(type);
    }

    public void AddDelayStrike(Vector2 pos) {
        delayStrikes++;

        AudioManager.Instance.PlayAlertAudio();

        if (delayStrikes >= maxDelayStrikes) {
            StartCoroutine(GameOver(pos, GameOverType.Delays));
        }
    }
}