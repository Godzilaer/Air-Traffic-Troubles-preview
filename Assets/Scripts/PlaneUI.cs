using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneUI : MonoBehaviour
{
    //Public to allow PlaneMovement to access
    public List<Vector2> waypoints;

    private GameManager gm;
    private SpriteRenderer sr;
    private bool planeSelected;
 
    private List<GameObject> waypointNodes;
    private List<WaypointType> waypointTypes;

    private Transform waypointNodeHolder;
    private LineRenderer waypointPathRenderer;

    private bool planeScheduledToLand;

    private enum WaypointType
    {
        Path, Transition, Terminus
    }

    private Dictionary<WaypointType, GameObject> waypointTypeToGameObject;

    private void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        waypointNodeHolder = GameObject.Find("Radar/Waypoints").transform;
        waypointPathRenderer = GetComponent<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();

        waypointTypeToGameObject.Add(WaypointType.Path, Resources.Load<GameObject>("PathNode"));
        waypointTypeToGameObject.Add(WaypointType.Transition, Resources.Load<GameObject>("TransitionNode"));
        waypointTypeToGameObject.Add(WaypointType.Terminus, Resources.Load<GameObject>("TerminusNode"));

        waypoints = new List<Vector2>();
        waypointNodes = new List<GameObject>();
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

            bool inBounds = false;
            bool inTouchdownZone = false;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);
            
            //Checks if raycast hit the RadarBackground
            //If so, it means the mouse is in bounds
            if (hit.collider != null)
            {
                if(hit.collider.CompareTag("RadarBackground"))
                {
                    inBounds = true;
                }
                
                if(hit.collider.CompareTag("TouchdownZone"))
                {
                    inTouchdownZone = true;
                }
            }

            if(inBounds)
            {
                //Left button: Place new waypoint
                if (Input.GetMouseButtonDown(0))
                {


                    AddWaypoint(WaypointType.Path, mouseWorldPos);
                }

                //Right button: Delete waypoint
                if (Input.GetMouseButtonDown(1))
                {
                    for (int i = 0; i < waypoints.Count; i++)
                    {
                        //If waypoint is close enough to cursor remove it
                        if (Vector2.Distance(waypoints[i], mouseWorldPos) < 0.1f)
                        {
                            RemoveWaypoint(i);
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
            points[i] = waypoints[i - 1];
        }

        //Update position count and finally set the positions
        waypointPathRenderer.positionCount = waypoints.Count + 1;
        waypointPathRenderer.SetPositions(points);
    }

    //Called when player clicks on plane
    public void Selected()
    {
        //Enable waypoint nodes
        foreach (GameObject waypointNode in waypointNodes)
        {
            waypointNode.SetActive(true);
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
        foreach (GameObject waypointNode in waypointNodes)
        {
            waypointNode.SetActive(false);
        }

        //Revert back to normal coloring
        waypointPathRenderer.startColor = Color.white;
        waypointPathRenderer.endColor = Color.white;
        sr.color = Color.white;

        planeSelected = false;
    }

    private void AddWaypoint(WaypointType type, Vector2 pos)
    {
        waypoints.Add(pos);

        GameObject newNode = Instantiate(waypointTypeToGameObject[type], pos, Quaternion.identity, waypointNodeHolder);
        newNode.SetActive(true);
        waypointNodes.Add(newNode);
    }

    public void RemoveWaypoint(int i)
    {
        waypoints.RemoveAt(i);

        Destroy(waypointNodes[i]);
        waypointNodes.RemoveAt(i);
    }
}
