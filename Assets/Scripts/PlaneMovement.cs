using UnityEngine;

public class PlaneMovement : MonoBehaviour
{
    [SerializeField]
    private float speed;

    private GameManager gm;
    private PlaneUI planeUI;

    private void Start()
    {
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        planeUI = GetComponent<PlaneUI>();
    }

    private void Update()
    {
        if (gm.gameOver) { return; }

        //If there are waypoints
        if (planeUI.waypoints.Count > 0)
        {
            //Once plane has flown close enough to the next waypoint, delete it
            if (Vector2.Distance(planeUI.waypoints[0], transform.position) < 0.0001f)
            {
                planeUI.RemoveWaypoint(0);
            }
            //Turn towards next waypoint
            else
            {
                transform.up = planeUI.waypoints[0] - (Vector2)transform.position;
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
            print("Aircraft collision!");
            gm.GameOver(aircraftCollision: true, collisionPos: transform.position);
        }
    }
}
