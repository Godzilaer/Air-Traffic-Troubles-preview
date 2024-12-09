using UnityEngine;
using System.Linq;

public class PlaneMovement : MonoBehaviour {
    private PlaneControl planeControl;
    //private planeControl.planeData planeControl.planeData;
    private Transform planeHitbox;

    private void Start() {
        planeControl = GetComponent<PlaneControl>();
        //planeControl.planeData = planeControl.planeControl.planeData;
        planeHitbox = transform.Find("Hitbox");
    }

    private void Update() {
        if (GameManager.gameOver) { return; }

        planeControl.planeData.realPos = planeHitbox.position;

        //If there are waypoints
        if (planeControl.planeData.internalWaypoints.Count > 0) {
            Waypoint.Internal nextInternalWaypoint = planeControl.planeData.internalWaypoints[0];

            //Once plane has flown close enough to the next waypoint, delete it
            if (Vector2.Distance(nextInternalWaypoint.position, planeControl.planeData.realPos) < 0.01f) {
                if (nextInternalWaypoint.type == Waypoint.Type.Transition) {
                    planeControl.planeData.onGround = true;
                    planeControl.planeData.speed *= 0.8f;
                    //Makes the plane slightly smaller to show that it is on the ground
                    transform.localScale = new Vector3(transform.localScale.x * 0.8f, transform.localScale.y * 0.8f, transform.localScale.z);
                }

                if (nextInternalWaypoint.type == Waypoint.Type.Terminus) {
                    if (GameManager.selectedPlane == gameObject) {
                        GameManager.Instance.DeselectPlane();
                    }

                    GameManager.Instance.PlaneLanded();
                    Destroy(gameObject);
                }

                planeControl.DeleteInternalWaypoint(nextInternalWaypoint);
            }
            //Turn towards next waypoint
            else {
                planeHitbox.up = nextInternalWaypoint.position - planeControl.planeData.realPos;
            }
        }

        //Continuously moves aircraft forward at its speed
        planeHitbox.Translate(planeControl.planeData.speed * Time.deltaTime * Vector2.up);
    }

    public void UpdateVisualPosition() {
        transform.SetPositionAndRotation(planeHitbox.position, planeHitbox.rotation);

        // Resets hitbox position as the visual has caught up with the offset
        planeHitbox.localPosition = Vector2.zero;
    }
}
