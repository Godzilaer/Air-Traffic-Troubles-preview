using UnityEngine;

public class PlaneMovement : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private GameManager gm;
    private PlaneControl planeControl;

    private void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        planeControl = GetComponent<PlaneControl>();
    }

    private void Update()
    {
        if (gm.gameOver) { return; }

        //If there are waypoints
        if (planeControl.waypoints.Count > 0)
        {
            Waypoint nextWaypoint = planeControl.waypoints[0];

            //Once plane has flown close enough to the next waypoint, delete it
            if (Vector2.Distance(nextWaypoint.position, transform.position) < 0.001f)
            {
                if(nextWaypoint.type == WaypointType.Transition)
                {
                    planeControl.onGround = true;
                    speed *= 0.8f;
                    //Makes the plane slightly smaller to show that it is on the ground
                    transform.localScale = new Vector3(transform.localScale.x * 0.8f, transform.localScale.y * 0.8f, transform.localScale.z);
                }

                if(nextWaypoint.type == WaypointType.Terminus)
                {
                    if(gm.selectedPlane == gameObject)
                    {
                        gm.DeselectPlane();
                    }

                    gm.PlaneLanded();
                    Destroy(gameObject);
                }

                planeControl.RemoveWaypoint(nextWaypoint);
            }
            //Turn towards next waypoint
            else
            {
                transform.up = nextWaypoint.position - (Vector2)transform.position;
            }
        }

        //Continuously moves aircraft forward at its speed
        transform.Translate(speed * Time.deltaTime * Vector2.up);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If other collider is another plane
        if(collision.gameObject.CompareTag("Plane"))
        {
            //If both planes are on the ground or both in the air
            if(collision.gameObject.GetComponent<PlaneControl>().onGround == planeControl.onGround)
            {
                print("Aircraft collision!");
                gm.GameOver(aircraftCollision: true, collisionPos: transform.position);
            }
        }
    }
}
