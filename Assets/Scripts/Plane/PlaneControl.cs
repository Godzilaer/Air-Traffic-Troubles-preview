using UnityEngine;
using System.Collections.Generic;

public class PlaneControl : MonoBehaviour {
    public PlaneData planeData;

    public static Transform waypointHolder;

    private PlaneMovement planeMovement;
    private PlaneLabels planeLabels;
    private SpriteRenderer sr;
    private LineRenderer waypointPathRenderer;

    private void Start() {
        waypointHolder = GameObject.Find("Radar/Waypoints").transform;
        planeMovement = GetComponent<PlaneMovement>();
        planeLabels = GetComponent<PlaneLabels>();
        waypointPathRenderer = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();

        planeData.Initialize(transform.position);
    }

    private void Update() {
        if (GameManager.gameOver)
        {
            return;
        }

        UpdateWaypointPathRenderer();
        planeData.delayTime -= Time.deltaTime;
    }

    public void OnLanded()
    {
        GameManager.Instance.PlaneLanded(planeData.delayTime);
        planeLabels.DeleteLabels();

        Destroy(gameObject);
    }

    public void OnRadarScan() {
        UpdateVisualWaypoints();
        planeMovement.UpdateVisualPosition();
    }

    //Called when player clicks on plane
    public void Selected() {
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

    public void Deselected() {
        //Hide waypoint nodes
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.SetVisibility(false);
        }

        //Revert back to normal coloring
        waypointPathRenderer.startColor = planeData.pathColor;
        waypointPathRenderer.endColor = planeData.pathColor;
        sr.color = Color.white;

        planeData.isSelected = false;
    }

    public void UpdateVisualWaypoints() {
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.DeleteNode();
        }

        planeData.visualWaypoints.Clear();

        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            bool isPlaneSelected = (GameManager.selectedPlane == gameObject);
            planeData.visualWaypoints.Add(new Waypoint.Visual(internalWaypoint.type, internalWaypoint.position, waypointHolder, isVisible: isPlaneSelected));
        }
    }

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
        if(planeData.onGround) { return; }

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

    private void DeleteTransitionAndTerminusWaypoints() {
        List<Waypoint.Internal> waypointsToDelete = new List<Waypoint.Internal>();

        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            if (internalWaypoint.type == Waypoint.Type.Transition || internalWaypoint.type == Waypoint.Type.Terminus) {
                waypointsToDelete.Add(internalWaypoint);
            }
        }

        foreach(Waypoint.Internal waypoint in waypointsToDelete) {
            DeleteInternalWaypoint(waypoint);
            UpdateVisualWaypoints();
        }
    }

    public void AttemptDeleteWaypoint(Vector3 mouseWorldPos) {
        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            if (Vector2.Distance(internalWaypoint.position, mouseWorldPos) > 0.1f) {
                continue;
            }

            //If the waypoint is Transition or Terminus, remove both of them
            //Otherwise just simply delete the waypoint
            if (internalWaypoint.type == Waypoint.Type.Transition || internalWaypoint.type == Waypoint.Type.Terminus) {
                DeleteTransitionAndTerminusWaypoints();
                planeData.routedToRunway = false;
                break;
            } else {
                DeleteInternalWaypoint(internalWaypoint);
                UpdateVisualWaypoints();
                break;
            }
        }
    }
}