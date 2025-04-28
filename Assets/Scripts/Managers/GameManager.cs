using System.Collections;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public static bool gameOver { get; private set; }
    public static int score { get; private set; }
    public static int aircraftServed { get; private set; }
    public static int delayStrikes { get; private set; }

    public static GameObject selectedPlane { get; private set; }

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

    private MouseOverlapData mouseOverlapData;

    public enum GameOverType {
        Collision,
        Fuel,
        Delays,
    }

    private class MouseOverlapData {
        public Vector2 pos;
        public GameObject hitPlane;
        public Transform hitLandingArea;
        public bool isRadarHit;

        public MouseOverlapData() {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            pos = mouseWorldPos;

            Collider2D[] hits = Physics2D.OverlapPointAll(mouseWorldPos);

            foreach (Collider2D hit in hits) {
                if (hit.CompareTag("RunwayZone")) {
                    hitLandingArea = hit.transform;
                }

                if (hit.CompareTag("PlaneClickBox")) {
                    hitPlane = hit.transform.parent.gameObject;
                }

                if (hit.CompareTag("RadarArea")) {
                    isRadarHit = true;
                }
            }
        }
    }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        #if !UNITY_WEBGL
        Application.targetFrameRate = 60;
        #endif

        Time.timeScale = 1f;
        gameOver = false;
        score = 0;
        aircraftServed = 0;
        delayStrikes = 0;
        radarSpawnRadius = radarRadiusConstant * radarBackground.localScale.x;

        UserData.Initialize();

        if(isTutorial) {
            levelConfig = Resources.Load<LevelConfig>("LevelConfigs/Tutorial");
        } else {
            levelConfig = Resources.Load<LevelConfig>("LevelConfigs/" + (UserData.Instance.levelCompletion.selectedLevelId + 1));
        }
    }

    private void Update() {
        if (gameOver || UIManager.isPauseMenuActive) { return; }

        mouseOverlapData = new MouseOverlapData();

        if (selectedPlane) {
            PlaneControl planeControl = selectedPlane.GetComponent<PlaneControl>();

            if (!planeControl.planeData.onGround) {
                //Delete one waypoint
                //if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteWaypoint)) {
                if (Input.GetMouseButtonDown(2)) {
                    planeControl.DeleteClosestWaypointToMousePos(mouseOverlapData.pos);
                }

                //Delete all waypoints
                if (Input.GetKeyDown(UserData.Instance.settings.keybinds.deleteAllSelectedPlaneWaypoints)) {
                    planeControl.DeleteAllWaypoints();
                }
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            bool wasPlaneRoutedToLand = false;

            if (selectedPlane) {
                PlaneControl planeControl = selectedPlane.GetComponent<PlaneControl>();

                if (!planeControl.planeData.routedToRunway) {
                    TryPlaceNewWaypoints(planeControl, out wasPlaneRoutedToLand);
                }
            }

            //If the player clicked the runway to route a plane then a new plane shouldn't be selected
            if (mouseOverlapData.hitPlane && !wasPlaneRoutedToLand) {
                PlaneControl newPlaneControl = mouseOverlapData.hitPlane.GetComponent<PlaneControl>();

                GameObject oldSelected = selectedPlane;
                DeselectPlane();

                //Make sure the new plane is not the same as the old plane and that the new plane is on the ground
                if (mouseOverlapData.hitPlane != oldSelected && !newPlaneControl.planeData.onGround) {
                    selectedPlane = mouseOverlapData.hitPlane;
                    newPlaneControl.OnSelect();

                    if (isTutorial && TutorialManager.waitingForPlaneSelection) {
                        planeSelectedEvent();
                    }
                }
            }
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
            planeLandedEvent?.Invoke();
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

    private void TryPlaceNewWaypoints(PlaneControl planeControl, out bool wasPlaneRoutedToLand) {
        wasPlaneRoutedToLand = false;

        if (mouseOverlapData.hitLandingArea) {
            LandingArea currentLandingArea = mouseOverlapData.hitLandingArea.GetComponent<LandingArea>();

            //If selected plane is not allowed on this runway return
            if (!currentLandingArea.allowedAircraft.Contains(planeControl.planeData.aircraftType)) { return; }

            Vector2 firstLandingAreaPos = mouseOverlapData.hitLandingArea.position;
            Vector2 finalLandingAreaPos;

            if (planeControl.planeData.aircraftType == PlaneData.AircraftType.Helicopter) {
                finalLandingAreaPos = firstLandingAreaPos;
            } else {
                finalLandingAreaPos = currentLandingArea.oppositeRunway.position;
                planeControl.AddWaypoint(Waypoint.Type.Approach, mouseOverlapData.hitLandingArea.parent.position + mouseOverlapData.hitLandingArea.up * planeControl.planeData.finalDistance);
                planeControl.AddWaypoint(Waypoint.Type.Transition, firstLandingAreaPos);
            }

            planeControl.AddWaypoint(Waypoint.Type.Terminus, finalLandingAreaPos);
            planeControl.planeData.routedToRunway = true;

            wasPlaneRoutedToLand = true;

            return;
        }

        if (mouseOverlapData.isRadarHit && !mouseOverlapData.hitPlane) {
            planeControl.AddWaypoint(Waypoint.Type.Path, mouseOverlapData.pos);
        }
    }
}