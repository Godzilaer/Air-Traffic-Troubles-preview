using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public static bool gameOver { get; private set; }
    public static int score { get; private set; }
    public static float time { get; private set; }
    public static int aircraftServed { get; private set; }

    [Header("Objects")]
    [SerializeField] private CameraControl cameraControl;
    [SerializeField] private GameObject explosion;

    public static GameObject selectedPlane { get; private set; }

    private void Awake() {
        Application.targetFrameRate = 60;

        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        } 
    }

    private void Update() {
        //Deselect hotkey
        if (Input.GetKeyDown(KeyCode.Q)) {
            DeselectPlane();
        }

        //Left MB clicked and no plane already selected then attempt to select a plane
        if (Input.GetMouseButtonDown(0) && selectedPlane == null) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

            //If a collider was hit and if its the PlaneClickBox then select the plane
            if (hit.collider && hit.collider.CompareTag("PlaneClickBox")) {
                selectedPlane = hit.collider.transform.parent.gameObject;
                selectedPlane.GetComponent<PlaneControl>().Selected();
            }
        }
    }

    public void DeselectPlane() {
        if (!selectedPlane) { return; }

        selectedPlane.GetComponent<PlaneControl>().Deselected();
        selectedPlane = null;
    }

    public void GameOver(bool isAircraftCollision = false, Vector2 collisionPos = default) {
        //In a collision both planes will call this function
        //This ensures that this function is only run once
        if (gameOver) { return; }
        gameOver = true;

        if (isAircraftCollision) {
            //Spawn and play explosion effect at aircraft collision position
            GameObject newExplosion = Instantiate(explosion, collisionPos, Quaternion.identity);
            newExplosion.GetComponent<ParticleSystem>().Play();

            StartCoroutine(cameraControl.FocusOnCollision(collisionPos));
        }

        //To be added: Code that shows some kind of after-game screen with score, time, planes serviced, etc...
    }

    public void PlaneLanded(float delay = 0) {
        aircraftServed += 1;
        score += 5;
    }
}