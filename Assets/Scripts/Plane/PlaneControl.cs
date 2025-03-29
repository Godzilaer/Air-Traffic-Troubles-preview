using UnityEngine;
using System.Collections.Generic;

public class PlaneControl : MonoBehaviour {
    public PlaneData planeData;

    public static Transform waypointHolder;

    private PlaneMovement planeMovement;
    private PlaneLabels planeLabels;
    private SpriteRenderer sr;
    private LineRenderer waypointPathRenderer;

    private BoxCollider2D clickBoxCollider;
    private LineRenderer selectionBoxRenderer;

    private void Start() {
        waypointHolder = GameObject.Find("Radar/Waypoints").transform;
        planeMovement = GetComponent<PlaneMovement>();
        planeLabels = GetComponent<PlaneLabels>();
        waypointPathRenderer = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();

        Transform clickBox = transform.Find("ClickBox");
        clickBoxCollider = clickBox.GetComponent<BoxCollider2D>();
        selectionBoxRenderer = clickBox.GetComponent<LineRenderer>();

        planeData.Initialize(transform.position);
    }

    private void Update() {
        if (GameManager.gameOver) {
            OnDeselect();
            return;
        }

        UpdateWaypointPathRenderer();

        if (!planeData.onGround) {
            planeData.delayTime -= Time.deltaTime;

            if (planeData.delayTime <= -30f) {
                StartCoroutine(GameManager.Instance.GameOver(transform.position, GameManager.GameOverType.Fuel));
            }
        }

        if (!planeData.delayStrike && planeData.delayTime < -10f) {
            planeData.delayStrike = true;
            GameManager.Instance.AddDelayStrike(transform.position);
        }

        if (planeData.isSelected) {
            DrawSelectionBox();
        }
    }

    public void OnLanded() {
        GameManager.Instance.PlaneLanded(planeData.delayTime);
        planeLabels.DeleteLabels();

        Destroy(gameObject);
    }

    public void OnRadarScan() {
        UpdateVisualWaypoints();
        planeMovement.UpdateVisualPosition();
    }

    //Called when player clicks on plane
    public void OnSelect() {
        //Enable waypoint nodes
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.SetVisibility(true);
        }

        //Change to enhanced coloring to indicate selection
        waypointPathRenderer.startColor = Color.white;
        waypointPathRenderer.endColor = Color.white;
        sr.color = Color.cyan;

        planeData.isSelected = true;
    }

    public void OnDeselect() {
        //Hide waypoint nodes
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.SetVisibility(false);
        }

        //Revert back to normal coloring
        waypointPathRenderer.startColor = planeData.pathColor;
        waypointPathRenderer.endColor = planeData.pathColor;
        sr.color = Color.white;

        planeData.isSelected = false;
        selectionBoxRenderer.positionCount = 0;
    }

    //Sets visual waypoint positions to be the same as internal waypoints
    public void UpdateVisualWaypoints() {
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.DeleteNode();
        }

        planeData.visualWaypoints.Clear();

        bool isPlaneSelected = (GameManager.selectedPlane == gameObject);
        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            planeData.visualWaypoints.Add(new Waypoint.Visual(internalWaypoint.type, internalWaypoint.position, waypointHolder, isVisible: isPlaneSelected));
        }
    }

    //Adds new internal and visual waypoint
    public void AddWaypoint(Waypoint.Type type, Vector2 pos) {
        var internalWaypoint = new Waypoint.Internal(type, pos);
        var visualWaypoint = new Waypoint.Visual(type, pos, waypointHolder);

        planeData.internalWaypoints.Add(internalWaypoint);
        planeData.visualWaypoints.Add(visualWaypoint);
    }

    public void DeleteInternalWaypoint(Waypoint.Internal waypoint) {
        planeData.internalWaypoints.Remove(waypoint);
    }

    public void DeleteVisualWaypoint(Waypoint.Visual waypoint) {
        waypoint.DeleteNode();
        planeData.visualWaypoints.Remove(waypoint);
    }

    public void DeleteAllWaypoints() {
        planeData.routedToRunway = false;
        planeData.internalWaypoints.Clear();

        UpdateVisualWaypoints();
    }

    public void UpdateWaypointPathRenderer() {
        Vector3[] points = new Vector3[planeData.visualWaypoints.Count + 1];

        //Sets first point as current position
        points[0] = transform.position;

        //Loops through all planeData.waypoints and adds them to points
        //Offset by 1 to account for extra point from current position (line above)
        for (int i = 1; i < planeData.visualWaypoints.Count + 1; i++) {
            points[i] = planeData.visualWaypoints[i - 1].position;
        }

        //Update position count and finally set the positions
        waypointPathRenderer.positionCount = planeData.visualWaypoints.Count + 1;
        waypointPathRenderer.SetPositions(points);
    }

    private void DeleteAllLandingWaypoints() {
        List<Waypoint.Internal> waypointsToDelete = new List<Waypoint.Internal>();

        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            if (IsInternalWaypointForLanding(internalWaypoint)) {
                waypointsToDelete.Add(internalWaypoint);
            }
        }

        foreach (Waypoint.Internal waypoint in waypointsToDelete) {
            DeleteInternalWaypoint(waypoint);
        }

        UpdateVisualWaypoints();
    }

    public void DeleteClosestWaypointToMousePos(Vector3 mouseWorldPos) {
        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            if (Vector2.Distance(internalWaypoint.position, mouseWorldPos) > 0.2f) {
                continue;
            }

            if (IsInternalWaypointForLanding(internalWaypoint)) {
                DeleteAllLandingWaypoints();
                planeData.routedToRunway = false;
                break;
            } else {
                DeleteInternalWaypoint(internalWaypoint);
                UpdateVisualWaypoints();
                break;
            }
        }
    }

    private bool IsInternalWaypointForLanding(Waypoint.Internal waypoint) {
        return waypoint.type == Waypoint.Type.Approach || waypoint.type == Waypoint.Type.Transition || waypoint.type == Waypoint.Type.Terminus;
    }

    private void DrawSelectionBox() {
        Vector3[] points = new Vector3[4];

        selectionBoxRenderer.positionCount = 0;
        selectionBoxRenderer.positionCount = 4;

        float x = clickBoxCollider.size.x / 2f;
        float y = clickBoxCollider.size.y / 2f;
        points[0] = transform.TransformPoint(new Vector3(x, y));
        points[1] = transform.TransformPoint(new Vector3(-x, y));
        points[2] = transform.TransformPoint(new Vector3(-x, -y));
        points[3] = transform.TransformPoint(new Vector3(x, -y));

        selectionBoxRenderer.SetPositions(points);
    }
}