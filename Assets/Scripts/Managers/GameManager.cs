using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    public static bool gameOver { get; private set; }
    public static int score { get; private set; }
    public static int aircraftServed { get; private set; }
    public static int delayStrikes { get; private set; }
    public static float time { get; private set; }

    public static float radarSpawnRadius { get; private set; }
    public float radarRadiusConstant;

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            //If a collider was hit and if its the PlaneClickBox then select the plane
            if (hit.collider && hit.collider.CompareTag("PlaneClickBox")) {
                GameObject oldSelected = selectedPlane;
                GameObject newPlane = hit.collider.transform.parent.gameObject;
                PlaneControl newPlaneControl = newPlane.GetComponent<PlaneControl>();

                DeselectPlane();

                //If the clicked plane is not the plane that is already selected and the plane is in the air
                if (newPlane != oldSelected && !newPlaneControl.planeData.onGround) {
                    selectedPlane = newPlane;
                    newPlaneControl.OnSelect();
                }
            }
        }

        if (selectedPlane) {
            UpdateSelectedPlane();
        }
    }

    private void UpdateSelectedPlane() {
        var planeControl = selectedPlane.GetComponent<PlaneControl>();

        //Gets mouse pos in world space
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        bool inRunwayZone = false;
        Transform currentRunwayTransform = null;
        Vector2 currentRunwayZonePos = Vector2.zero, oppositeRunwayZonePos = Vector2.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.CompareTag("PlaneClickBox") || hit.collider.CompareTag("UIItem") || hit.collider.CompareTag("Background")) {
                print(hit.collider.tag);
                return;
            }

            if (hit.collider.CompareTag("RunwayZone")) {
                inRunwayZone = true;

                currentRunwayTransform = hit.collider.transform;
                currentRunwayZonePos = currentRunwayTransform.position;
                Runway currentRunway = currentRunwayTransform.GetComponent<Runway>();
                oppositeRunwayZonePos = currentRunway.oppositeRunway.position;

                break;
            }
        }

        //Left button: Place new waypoint if not routed to runway
        if (Input.GetMouseButtonDown(0) && !planeControl.planeData.routedToRunway) {
            if (inRunwayZone) {
                planeControl.planeData.routedToRunway = true;

                planeControl.AddWaypoint(Waypoint.Type.Approach, currentRunwayTransform.parent.position + currentRunwayTransform.up * planeControl.planeData.finalDistance);
                planeControl.AddWaypoint(Waypoint.Type.Transition, currentRunwayZonePos);
                planeControl.AddWaypoint(Waypoint.Type.Terminus, oppositeRunwayZonePos);
            } else {
                planeControl.AddWaypoint(Waypoint.Type.Path, mouseWorldPos);
            }
        }

        if(!planeControl.planeData.onGround) {
            //Delete one waypoint
            //if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteWaypoint)) {
            if(Input.GetMouseButtonDown(2)) {
                planeControl.DeleteClosestWaypointToMousePos(mouseWorldPos);
            }

            //Delete all waypoints
            if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteAllSelectedPlaneWaypoints)) {
                planeControl.DeleteAllWaypoints();
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

        //+15 delay = 15 score 
        //-5 delay = 0 score
        int scoreToAdd = Mathf.RoundToInt(Mathf.Min(Mathf.Max(0.75f * delay + 3.75f, 0f), 15f));
        score += scoreToAdd;

        StartCoroutine(UIManager.Instance.ScoreAddedVisual(scoreToAdd));
    }

    public IEnumerator GameOver(Vector2 posToZoom, GameOverType type) {
        if (gameOver) { yield return null; }
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