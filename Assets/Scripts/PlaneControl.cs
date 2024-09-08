using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneControl : MonoBehaviour
{
    public List<Waypoint> waypoints = new List<Waypoint>();
    public bool onGround;

    private GameManager gm;
    private SpriteRenderer sr;
    private bool planeSelected;

    private Transform waypointNodeHolder;
    private LineRenderer waypointPathRenderer;

    private bool routedToRunway;

    private Dictionary<WaypointType, GameObject> waypointTypeToGameObject;

    private void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        waypointNodeHolder = GameObject.Find("Radar/Waypoints").transform;
        waypointPathRenderer = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();

        waypointTypeToGameObject = new Dictionary<WaypointType, GameObject>
        {
            { WaypointType.Path, Resources.Load<GameObject>("PathNode") },
            { WaypointType.Transition, Resources.Load<GameObject>("TransitionNode") },
            { WaypointType.Terminus, Resources.Load<GameObject>("TerminusNode") }
        };
    }

    private void Update()
    {
        if (gm.gameOver) { return; }

        //Only runs if the player has selected this plane
        if (planeSelected)
        {
            //Gets mouse pos in world space
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            bool inRunwayZone = false;
            Vector2 currentRunwayZonePos = Vector2.zero, oppositeRunwayZonePos = Vector2.zero;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);

            
            //Checks if raycast hit the RadarBackground
            //If so, it means the mouse is in bounds
            foreach(RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("RunwayZone"))
                {
                    inRunwayZone = true;
                    currentRunwayZonePos = hit.collider.transform.position;

                    Transform runwayParent = hit.collider.transform.parent;
                    //Gets opposite runway position by checking which runway's position matches with the current and getting the other
                    oppositeRunwayZonePos = (Vector2) runwayParent.Find("1").position == currentRunwayZonePos ? oppositeRunwayZonePos = runwayParent.Find("2").position : oppositeRunwayZonePos = runwayParent.Find("1").position;
                }
            }

            //Left button: Place new waypoint if not routed to runway
            if (Input.GetMouseButtonDown(0) && !routedToRunway)
            {
                if(inRunwayZone)
                {
                    routedToRunway = true;
                    print("Routed to runway, locked route.");
                    AddWaypoint(WaypointType.Transition, currentRunwayZonePos);
                    AddWaypoint(WaypointType.Terminus, oppositeRunwayZonePos);
                }
                else
                {
                    AddWaypoint(WaypointType.Path, mouseWorldPos);
                }
            }

            //Right button: Delete waypoint if not on ground
            if (Input.GetMouseButtonDown(1) && !onGround)
            {
                bool deleteTransitionAndTerminus = false;

                foreach (Waypoint waypoint in waypoints)
                {
                    //If waypoint is close enough to cursor remove it
                    if (Vector2.Distance(waypoint.position, mouseWorldPos) < 0.1f)
                    {
                        //If the waypoint is Transition or Terminus remove it, schedule removing of remaining waypoint also
                        //Otherwise just simply delete the waypoint
                        if (waypoint.type == WaypointType.Transition || waypoint.type == WaypointType.Terminus)
                        {
                            RemoveWaypoint(waypoint);
                            deleteTransitionAndTerminus = true;
                            routedToRunway = false;
                        }
                        else
                        {
                            RemoveWaypoint(waypoint);
                        }
                            
                        break;
                    }
                }

                if(deleteTransitionAndTerminus)
                {
                    //Loops through all waypoints again in order to delete the remaining Transition or Terminus waypoint
                    foreach (Waypoint waypoint in waypoints)
                    {
                        if (waypoint.type == WaypointType.Transition || waypoint.type == WaypointType.Terminus)
                        {
                            RemoveWaypoint(waypoint);
                            break;
                        }
                    }
                }
            }
        }

        Vector3[] points = new Vector3[waypoints.Count + 1];

        //Sets first point as current position
        points[0] = transform.position;

        //Loops through all waypoints and adds them to points
        //Offset by 1 to account for extra point from current position (line above)
        for (int i = 1; i < waypoints.Count + 1; i++)
        {
            points[i] = waypoints[i - 1].position;
        }

        //Update position count and finally set the positions
        waypointPathRenderer.positionCount = waypoints.Count + 1;
        waypointPathRenderer.SetPositions(points);
    }

    //Called when player clicks on plane
    public void Selected()
    {
        //Enable waypoint nodes
        foreach (Waypoint waypoint in waypoints)
        {
            waypoint.node.SetActive(true);
        }

        //Change to enhanced coloring to indicate selection
        waypointPathRenderer.startColor = Color.yellow;
        waypointPathRenderer.endColor = Color.yellow;
        sr.color = Color.cyan;

        planeSelected = true;
    }

    //Called when player deselects plane
    public void Deselected()
    {
        //Hide waypoint nodes
        foreach (Waypoint waypoint in waypoints)
        {
            waypoint.node.SetActive(false);
        }

        //Revert back to normal coloring
        waypointPathRenderer.startColor = Color.white;
        waypointPathRenderer.endColor = Color.white;
        sr.color = Color.white;

        planeSelected = false;
    }

    private void AddWaypoint(WaypointType type, Vector2 pos)
    {
        Waypoint newWaypoint = new Waypoint();
        newWaypoint.position = pos;
        newWaypoint.type = type;

        GameObject newNode = Instantiate(waypointTypeToGameObject[type], pos, Quaternion.identity, waypointNodeHolder);
        newNode.SetActive(true);

        newWaypoint.node = newNode;

        waypoints.Add(newWaypoint);
    }

    public void RemoveWaypoint(Waypoint waypoint)
    {
        Destroy(waypoint.node);
        waypoints.Remove(waypoint);
    }
}