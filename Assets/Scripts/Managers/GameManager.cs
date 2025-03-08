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
    [SerializeField] private int maxDelayStrikes;

    public static GameObject selectedPlane { get; private set; }

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
        
        //Deselect hotkey
        if (Input.GetKeyDown(UserData.data.settings.keybinds.deselectPlane)) {
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

            if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteAllSelectedPlaneWaypoints)) {
                planeControl.DeleteAllWaypoints();
            }
        }
    }

    private void UpdateSelectedPlane(PlaneControl planeControl) {
        //Gets mouse pos in world space
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        bool inRunwayZone = false;
        Transform currentRunwayTransform = null;
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

            currentRunwayTransform = hit.collider.transform;
            currentRunwayZonePos = currentRunwayTransform.position;
            Runway currentRunway = currentRunwayTransform.GetComponent<Runway>();
            oppositeRunwayZonePos = currentRunway.oppositeRunway.position;
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

        //Right button: Delete waypoint if not on ground
        if (Input.GetKeyDown(UserData.data.settings.keybinds.deleteWaypoint) && !planeControl.planeData.onGround) {
            planeControl.AttemptDeleteWaypoint(mouseWorldPos);
        }
    }

    public void DeselectPlane() {
        if (!selectedPlane) { return; }

        selectedPlane.GetComponent<PlaneControl>().Deselected();
        selectedPlane = null;
    }

    public void AddDelayStrike(Vector2 pos) {
        delayStrikes++;

        if(delayStrikes >= maxDelayStrikes) {
            GameOver(pos, GameOverType.Delays);
        }
    }

    public enum GameOverType {
        Collision,
        Fuel,
        Delays,
    }

    public void GameOver(Vector2 posToZoom, GameOverType type) {
        //In a collision both planes will call this function
        //This ensures that this function is only run once
        if (gameOver) { return; }
        gameOver = true;

        Time.timeScale = 0f;

        if (type == GameOverType.Collision) {
            //Spawn and play explosion effect at aircraft collision position
            GameObject newExplosion = Instantiate(explosion, posToZoom, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();
        }

        //In the future, game over screen will show cause of gameover.
        print("Gameover Type: " + type.ToString());

        StartCoroutine(cameraControl.FocusOnTarget(posToZoom));

        UIManager.Instance.ShowGameOverScreen(type);
    }

    public void PlaneLanded(float delay) {
        aircraftServed += 1;

        //+10 delay = 15 score 
        //0 delay = 5 score
        //-5 delay = 0 score
        int scoreToAdd = Mathf.RoundToInt(Mathf.Min(Mathf.Max(delay + 5, 0), 15));
        score += scoreToAdd;

        StartCoroutine(UIManager.Instance.ScoreAddedVisual(scoreToAdd));
    }
}