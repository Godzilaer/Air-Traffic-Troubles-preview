using System.Collections;
using UnityEngine;

public class CameraControl : MonoBehaviour {
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    [SerializeField] private float panSpeed;
    [SerializeField] private Vector2 limits;

    private Vector3 originalPos;
    private Vector3 mouseDragPos;
    private bool mouseDragging = false;

    private void Start() {
        originalPos = mainCamera.transform.position;
    }

    private void Update() {
        if (GameManager.gameOver) {
            return;
        }

        Zoom();
        Pan();
    }

    public IEnumerator FocusOnTarget(Vector3 targetPos) {
        Vector3 originalPos = transform.position;
        targetPos.z = transform.position.z;
        float currentZoom = mainCamera.orthographicSize;
        float targetZoom = 3f;

        float duration = 1.5f;
        float time = 0f;

        while (time < duration) {
            transform.position = Vector3.Lerp(originalPos, targetPos, time / duration);
            mainCamera.orthographicSize = Mathf.Lerp(currentZoom, targetZoom, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    //From ChatGPT edited by me
    private void Zoom() {
        // Zoom in and out using the mouse scroll wheel
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        float newZoom = mainCamera.orthographicSize - zoomInput * zoomSpeed;

        mainCamera.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);

        //If fully zoomed out return to original position
        if (mainCamera.orthographicSize == maxZoom) {
            transform.position = originalPos;
        }
    }

    // From https://faramira.com/implement-camera-pan-and-zoom-controls-in-unity2d/
    private void Pan() {
        //If fully zoomed out don't pan camera
        if (mainCamera.orthographicSize == maxZoom) {
            return;
        }

        // Save the position in worldspace.
        if (Input.GetMouseButtonDown(1)) {
            mouseDragPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseDragging = true;
        }

        if (Input.GetMouseButton(1) && mouseDragging) {
            Vector3 diff = (mouseDragPos - mainCamera.ScreenToWorldPoint(Input.mousePosition)) * panSpeed;
            diff.z = 0f;

            Vector3 result = mainCamera.transform.position + diff;

            if(result.x > limits.x || result.x < -limits.x) {
                diff.x = 0f;
            }

            if(result.y > limits.y || result.y < -limits.y) {
                diff.y = 0f;
            }

            mainCamera.transform.position += diff;
        }

        if (Input.GetMouseButtonUp(1)) {
            mouseDragging = false;
        }
    }
}
