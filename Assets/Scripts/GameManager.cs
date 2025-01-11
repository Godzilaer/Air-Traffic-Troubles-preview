using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public static bool gameOver { get; private set; }
    public static int score { get; private set; }
    public static float time { get; private set; }
    public static int aircraftServed { get; private set; }
    public static float radarSpawnRadius { get; private set; }
    public float radarRadiusConstant;

    [Header("Objects")]
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private GameObject explosion;
    [SerializeField] private Transform radarBackground;

    public static GameObject selectedPlane { get; private set; }

    private void Awake() {
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }

        radarSpawnRadius = radarRadiusConstant * radarBackground.localScale.x;

        UserData.Initialize();
    }

    private void Update() {
        if (gameOver) { return; }
        
        //Deselect hotkey
        if (Input.GetKeyDown(UserData.data.keybinds.deselectPlane)) {
            DeselectPlane();
        }

        //Left MB clicked and no plane already selected then attempt to select a plane
        if (Input.GetMouseButtonDown(0) && selectedPlane == null) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            //If a collider was hit and if its the PlaneClickBox then select the plane
            if (hit.collider && hit.collider.CompareTag("PlaneClickBox")) {
                selectedPlane = hit.collider.transform.parent.gameObject;
                selectedPlane.GetComponent<PlaneControl>().Selected();
            }
        }

        if (selectedPlane) {
            var planeControl = selectedPlane.GetComponent<PlaneControl>();

            UpdateSelectedPlane(planeControl);

            if (Input.GetKeyDown(UserData.data.keybinds.deleteAllSelectedPlaneWaypoints)) {
                planeControl.DeleteAllWaypoints();
            }
        }
    }

    private void UpdateSelectedPlane(PlaneControl planeControl) {
        //Gets mouse pos in world space
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        bool inRunwayZone = false;
        Vector2 currentRunwayZonePos = Vector2.zero, oppositeRunwayZonePos = Vector2.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.CompareTag("PlaneClickBox") || hit.collider.CompareTag("UIItem")) {
                return;
            }

            if (!hit.collider.CompareTag("RunwayZone")) {
                continue;
            }

            inRunwayZone = true;

            currentRunwayZonePos = hit.collider.transform.position;
            Runway currentRunway = hit.collider.transform.GetComponent<Runway>();
            oppositeRunwayZonePos = currentRunway.oppositeRunway.position;
        }

        //Left button: Place new waypoint if not routed to runway
        if (Input.GetMouseButtonDown(0) && !planeControl.planeData.routedToRunway) {
            if (inRunwayZone) {
                planeControl.planeData.routedToRunway = true;

                planeControl.AddWaypoint(Waypoint.Type.Transition, currentRunwayZonePos);
                planeControl.AddWaypoint(Waypoint.Type.Terminus, oppositeRunwayZonePos);
            } else {
                planeControl.AddWaypoint(Waypoint.Type.Path, mouseWorldPos);
            }
        }

        //Right button: Delete waypoint if not on ground
        if (Input.GetKeyDown(UserData.data.keybinds.deleteWaypoint) && !planeControl.planeData.onGround) {
            planeControl.AttemptDeleteWaypoint(mouseWorldPos);
        }
    }

    public void DeselectPlane() {
        if (!selectedPlane) { return; }

        selectedPlane.GetComponent<PlaneControl>().Deselected();
        selectedPlane = null;
    }

    public void GameOver(bool isAircraftCollision = false, Vector2 collisionPos = default) {
        //In a collision both planes will call this function
        //This ensures that this function is only run once
        if (gameOver) { return; }
        gameOver = true;

        if (isAircraftCollision) {
            //Spawn and play explosion effect at aircraft collision position
            GameObject newExplosion = Instantiate(explosion, collisionPos, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();

            StartCoroutine(cameraControl.FocusOnCollision(collisionPos));
        }

        //To be added: Code that shows some kind of after-game screen with score, time, planes serviced, etc...
    }

    public void PlaneLanded(float delay) {
        aircraftServed += 1;

        //+25 = 15 score 
        //0 = 5 score
        //-10 = 0 score
        int score_to_add = Mathf.RoundToInt(10 + Mathf.Min(Mathf.Max(0.4f * delay - 5, -10), 5));
        score += score_to_add;
    }
}