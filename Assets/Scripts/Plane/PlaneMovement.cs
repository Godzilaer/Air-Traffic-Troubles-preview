using UnityEngine;
using System.Linq;

public class PlaneMovement : MonoBehaviour {
    private PlaneControl planeControl;
    //private planeControl.planeData planeControl.planeData;
    private Transform planeHitbox;
    private bool planeScaledDown;

    private void Start() {
        planeControl = GetComponent<PlaneControl>();
        //planeControl.planeData = planeControl.planeControl.planeData;
        planeHitbox = transform.Find("Hitbox");
    }

    private void Update() {
        if (GameManager.gameOver) { return; }

        bool isWithinRadar = Vector2.Distance(planeHitbox.position, Vector2.zero) < 19f;

        //If the plane has already entered the radar before and is now not within the radar, turn it back
        if (planeControl.planeData.hasEnteredRadar) {
            if (!isWithinRadar) {
                planeControl.planeData.hasEnteredRadar = false;
                planeHitbox.up = Vector3.zero - planeHitbox.position;
            }
        } else if (isWithinRadar) {
            planeControl.planeData.hasEnteredRadar = true;
        }

        planeControl.planeData.realPos = planeHitbox.position;

        //Continuously moves aircraft forward at its speed
        planeHitbox.Translate(planeControl.planeData.speed * Time.deltaTime * Vector2.up);

        //If no waypoints skip
        if (planeControl.planeData.internalWaypoints.Count == 0) {
            return;
        }

        Waypoint.Internal nextInternalWaypoint = planeControl.planeData.internalWaypoints[0];

        //Once plane has flown close enough to the next waypoint, delete it
        if (Vector2.Distance(nextInternalWaypoint.position, planeControl.planeData.realPos) < 0.01f) {
            //Aircraft transitions to on ground
            if (nextInternalWaypoint.type == Waypoint.Type.Transition) {
                planeControl.planeData.onGround = true;
                planeControl.planeData.speed *= 0.8f;
            }

            //Aircraft has officially landed
            if (nextInternalWaypoint.type == Waypoint.Type.Terminus) {
                //Deselect if selected
                if (GameManager.selectedPlane == gameObject) {
                    GameManager.Instance.DeselectPlane();
                }

                if (planeControl.planeData.aircraftType == PlaneData.AircraftType.Helicopter) {
                    planeControl.planeData.onGround = true;
                    planeControl.planeData.speed = 0f;
                    planeControl.WaitAfterHelicopterLands();
                } else {
                    planeControl.OnLanded();
                }
            }

            planeControl.DeleteInternalWaypoint(nextInternalWaypoint);
        }
        //Turn towards next waypoint
        else {
            planeHitbox.up = nextInternalWaypoint.position - planeControl.planeData.realPos;
        }
    }

    public void UpdateVisualPosition() {
        transform.SetPositionAndRotation(planeHitbox.position, planeHitbox.rotation);
        // Resets hitbox position as the visual has caught up with the offset
        planeHitbox.localPosition = Vector2.zero;

        if (planeControl.planeData.onGround && !planeScaledDown) {
            planeScaledDown = true;
            transform.localScale = Vector3.Scale(transform.localScale, new Vector3(0.6f, 0.6f, 0.6f));
        }
    }
}
