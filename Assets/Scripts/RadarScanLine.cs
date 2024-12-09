using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarScanLine : MonoBehaviour {
    [SerializeField]
    private float rotateSpeed;

    void Update() {
        if (GameManager.gameOver) {
            return;
        }

        transform.Rotate(new Vector3(0f, 0f, -rotateSpeed * Time.deltaTime));
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("PlaneHitbox")) {
            return;
        }

        collision.gameObject.GetComponent<PlaneHitbox>().planeMovement.UpdateVisualPosition();
        collision.gameObject.GetComponent<PlaneHitbox>().planeControl.UpdateVisualWaypoints();
    }
}