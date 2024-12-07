using UnityEngine;

// 
public class CameraControl : MonoBehaviour
{
    private Vector3 mouseDragPos;
    private bool mouseDragging = false;

    // Save a reference to the Camera.main
    [SerializeField] private Camera mainCamera;
    public float zoomSpeed = 2f;       // Speed of zooming in and out
    public float minZoom = 5f;         // Minimum zoom level (zoomed out)
    public float maxZoom = 20f;        // Maximum zoom level (zoomed in)
    public float panSpeed = 0.2f;      // Speed of panning when zoomed in

    private Vector3 originalPosition;  // Store the original camera position
    private float originalZoomLevel;   // Store the original zoom level (initial orthographicSize)
    private Vector3 previousMousePosition; // Store the previous mouse position for smooth panning

    public static bool IsCameraPanning
    {
        get;
        set;
    } = true;

    private void Start()
    {
        // Store the original camera position and zoom level
        originalPosition = mainCamera.transform.position;
        originalZoomLevel = mainCamera.orthographicSize;
        previousMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        previousMousePosition.z = 0; // Make sure it's at the same plane as the camera
    }

    //From ChatGPT edited by me
    private void Zoom()
    {
        // Zoom in and out using the mouse scroll wheel
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");
        float newZoom = mainCamera.orthographicSize - zoomInput * zoomSpeed;

        // Clamp the zoom to be between minZoom and maxZoom
        mainCamera.orthographicSize = Mathf.Clamp(newZoom, minZoom, maxZoom);
    }

    // From https://faramira.com/implement-camera-pan-and-zoom-controls-in-unity2d/
    private void Pan()
    {
        // Camera panning is disabled when a tile is selected.
        if (!IsCameraPanning)
        {
            mouseDragging = false;
            return;
        }

        // Save the position in worldspace.
        if (Input.GetMouseButtonDown(1))
        {
            mouseDragPos = mainCamera.ScreenToWorldPoint(
              Input.mousePosition);
            mouseDragging = true;
        }
        if (Input.GetMouseButton(1) && mouseDragging)
        {
            Vector3 diff = mouseDragPos - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            diff.z = 0.0f;
            mainCamera.transform.position += diff;
        }
        if (Input.GetMouseButtonUp(1))
        {
            mouseDragging = false;
        }
    }

    void Update()
    {
        Zoom();
        Pan();
    }
}
