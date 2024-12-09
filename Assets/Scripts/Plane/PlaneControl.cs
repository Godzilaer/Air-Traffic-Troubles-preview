using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneControl : MonoBehaviour {
    public PlaneData planeData;

    public static Transform waypointHolder;

    private SpriteRenderer sr;
    private LineRenderer waypointPathRenderer;

    private void Start() {
        waypointHolder = GameObject.Find("Radar/Waypoints").transform;
        waypointPathRenderer = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();

        planeData.OnSpawn(transform.position);
    }

    private void Update() {
        if (GameManager.gameOver) { return; }

        //UpdatePlaneDisplayText();

        if (!planeData.isSelected) {
            return;
        }

        //Gets mouse pos in world space
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        bool inRunwayZone = false;
        Vector2 currentRunwayZonePos = Vector2.zero, oppositeRunwayZonePos = Vector2.zero;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

        foreach (RaycastHit2D hit in hits) {
            if(hit.collider.CompareTag("PlaneClickBox") || hit.collider.CompareTag("UIItem")){
                return;
            }

            if (!hit.collider.CompareTag("RunwayZone")) {
                continue;
            }

            inRunwayZone = true;
            currentRunwayZonePos = hit.collider.transform.position;
            Transform runwayParent = hit.collider.transform.parent;
            //Gets opposite runway position by checking which runway's position matches with the current and getting the other
            oppositeRunwayZonePos = (Vector2)runwayParent.Find("1").position == currentRunwayZonePos ? oppositeRunwayZonePos = runwayParent.Find("2").position : oppositeRunwayZonePos = runwayParent.Find("1").position;
        }

        //Left button: Place new waypoint if not routed to runway
        if (Input.GetMouseButtonDown(0) && !planeData.routedToRunway) {
            if (inRunwayZone) {
                planeData.routedToRunway = true;

                AddWaypoint(Waypoint.Type.Transition, currentRunwayZonePos);
                AddWaypoint(Waypoint.Type.Terminus, oppositeRunwayZonePos);
            } else {
                AddWaypoint(Waypoint.Type.Path, mouseWorldPos);
            }
        }

        //Right button: Delete waypoint if not on ground
        if (Input.GetMouseButtonDown(1) && !planeData.onGround) {
            foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
                if (Vector2.Distance(internalWaypoint.position, mouseWorldPos) > 0.1f) {
                    continue;
                }

                //If the waypoint is Transition or Terminus, remove both of them
                //Otherwise just simply delete the waypoint
                if (internalWaypoint.type == Waypoint.Type.Transition || internalWaypoint.type == Waypoint.Type.Terminus) {
                    DeleteTransitionAndTerminusWaypoints();
                    planeData.routedToRunway = false;
                } else {
                    DeleteInternalWaypoint(internalWaypoint);
                }

                break;
            }
        }

        UpdateWaypointPathRenderer();
    }

    //Called when player clicks on plane
    public void Selected() {
        //Enable waypoint nodes
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.SetVisibility(true);
        }

        //Change to enhanced coloring to indicate selection
        waypointPathRenderer.startColor = Color.yellow;
        waypointPathRenderer.endColor = Color.yellow;
        sr.color = Color.cyan;

        planeData.isSelected = true;
    }

    public void Deselected() {
        //Hide waypoint nodes
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.SetVisibility(false);
        }

        //Revert back to normal coloring
        waypointPathRenderer.startColor = Color.white;
        waypointPathRenderer.endColor = Color.white;
        sr.color = Color.white;

        planeData.isSelected = false;
    }

    public void UpdateVisualWaypoints() {
        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            visualWaypoint.DeleteNode();
        }

        planeData.visualWaypoints.Clear();

        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            planeData.visualWaypoints.Add(new Waypoint.Visual(internalWaypoint.type, internalWaypoint.position, waypointHolder));
        }
    }

    private void AddWaypoint(Waypoint.Type type, Vector2 pos) {
        var internalWaypoint = new Waypoint.Internal(type, pos);
        var visualWaypoint = new Waypoint.Visual(type, pos, waypointHolder);

        planeData.internalWaypoints.Add(internalWaypoint);
        planeData.visualWaypoints.Add(visualWaypoint);
    }

    public void DeleteInternalWaypoint(Waypoint.Internal waypoint) {
        planeData.internalWaypoints.Remove(waypoint);
    }

    public void DeleteVisualWaypoint(Waypoint.Visual waypoint) {
        planeData.visualWaypoints.Remove(waypoint);
    }

    private void UpdateWaypointPathRenderer() {
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
        foreach (Waypoint.Internal internalWaypoint in planeData.internalWaypoints) {
            if (internalWaypoint.type == Waypoint.Type.Transition || internalWaypoint.type == Waypoint.Type.Terminus) {
                DeleteInternalWaypoint(internalWaypoint);
                break;
            }
        }

        foreach (Waypoint.Visual visualWaypoint in planeData.visualWaypoints) {
            if (visualWaypoint.type == Waypoint.Type.Transition || visualWaypoint.type == Waypoint.Type.Terminus) {
                DeleteVisualWaypoint(visualWaypoint);
                break;
            }
        }
    }
}