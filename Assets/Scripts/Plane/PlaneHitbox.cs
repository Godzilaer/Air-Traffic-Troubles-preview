using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneHitbox : MonoBehaviour
{
    public PlaneMovement planeMovement { get; private set; }
    public PlaneControl planeControl { get; private set; }
    public PlaneData planeData { get; private set; }

    private void Awake() {
        planeMovement = transform.parent.GetComponent<PlaneMovement>();
        planeControl = transform.parent.GetComponent<PlaneControl>();
        planeData = planeControl.planeData;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        //If collider is not another plane
        if (!collision.gameObject.CompareTag("PlaneHitbox")) {
            return;
        }

        PlaneHitbox hitbox = collision.gameObject.GetComponent<PlaneHitbox>();

        //If one plane is on the ground and one is in the aire
        if (hitbox.planeData.onGround != planeData.onGround) {
            return;
        }

        print("Aircraft collision!");

        //Update visual position so player can see how the planes collided
        hitbox.planeMovement.UpdateVisualPosition();

        GameManager.Instance.GameOver(isAircraftCollision: true, collisionPos: transform.position);
    }
}
