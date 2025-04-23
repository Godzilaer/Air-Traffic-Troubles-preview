using UnityEngine;

public class PlaneHitbox : MonoBehaviour {
    public PlaneControl planeControl { get; private set; }
    public PlaneData planeData { get; private set; }

    private void Awake() {
        planeControl = transform.GetComponentInParent<PlaneControl>();
        planeData = planeControl.planeData;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        //If collider is not another plane
        if (!collision.gameObject.CompareTag("PlaneHitbox")) {
            return;
        }

        PlaneHitbox hitbox = collision.gameObject.GetComponent<PlaneHitbox>();

        //If one plane is on the ground and one is in the air
        if (hitbox.planeData.onGround != planeData.onGround) {
            return;
        }

        //Update visual position so player can see how the planes collided
        hitbox.planeControl.OnRadarScan();
        StartCoroutine(GameManager.Instance.GameOver(transform.position, GameManager.GameOverType.Collision));
    }
}
