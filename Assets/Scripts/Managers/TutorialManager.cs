using System.Collections;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private PlaneSpawn planeSpawn;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject returnToMainMenuButton;
    [SerializeField] private TextMeshProUGUI tutorialText;

    public static bool waitingForPlaneSelection = false;

    private bool continueButtonPressed = false;
    private bool planeSelected = false;
    private bool planeLanded = false;

    private void Start() {
        StartCoroutine(Sequence());
    }

    public void OnContinueButtonPressed() {
        continueButtonPressed = true;
    }

    public void OnReturnToMainMenuButtonPressed() {
        SceneManager.LoadScene(0);
    }

    private IEnumerator WaitForContinuation() {
        continueButton.SetActive(true);

        while (!continueButtonPressed) {
            yield return null;
        }

        continueButtonPressed = false;
        Time.timeScale = 1f;

        continueButton.SetActive(false);
    }

    private IEnumerator WaitForPlaneSelected() {
        waitingForPlaneSelection = true;
        GameManager.planeSelectedEvent += BindToFirstPlaneSelection;

        while (!planeSelected) {
            yield return null;
        }

        waitingForPlaneSelection = false;
    }

    private IEnumerator WaitForPlaneLanded() {
        GameManager.planeLandedEvent += BindToPlaneLanding;

        while (!planeLanded) {
            yield return null;
        }
    }

    private void BindToFirstPlaneSelection() {
        GameManager.planeSelectedEvent -= BindToFirstPlaneSelection;
        planeSelected = true;
    }

    private void BindToPlaneLanding() {
        GameManager.planeLandedEvent -= BindToPlaneLanding;
        planeLanded = true;
    }

    private IEnumerator Sequence() {
        ShowTutorialMessage("Welcome to Air Traffic Troubles! This is a short tutorial that will get you oriented with the game mechanics. Let's begin!", true);
        yield return WaitForContinuation();

        planeSpawn.SpawnPlane(PlaneSpawn.Area.RadarEdge);
        ShowTutorialMessage("Look! Your first plane is coming. This marker indicates a plane has spawned off radar and will appear soon.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Wait a few seconds for it fly further into our radar...");
        yield return new WaitForSeconds(8.5f);

        ShowTutorialMessage("The top label is the callsign which is just a way of identifying a plane.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("The bottom label is the delay. A positive delay means the plane is early, 0 is on time, and negative is late.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Be careful! At +15 seconds, the plane's score begins to drop. At -15 seconds, you recieve a delay strike (3 of these is Game Over). At -30 seconds, the plane will run out of fuel!", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Now, try clicking the plane to select it.");
        yield return WaitForPlaneSelected();

        ShowTutorialMessage("Nice! See the box in the bottom right? That's the Plane Info section. This contains information about the selected plane including aircraft type, speed, and route estimated time.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Now, it's time to control the plane! Left clicking anywhere with a plane selected creates a waypoint that the plane will navigate towards.");
        yield return WaitForContinuation();

        ShowTutorialMessage("If you ever need to delete a waypoint, you must select a plane, hover over a waypoint and middle click. Also, Pressing C will clear all of a selected planes waypoints.");
        yield return WaitForContinuation();

        ShowTutorialMessage("I've paused the game for a moment. Planes need to land. When you click on one of the yellow boxes on a runway with a plane selected, you are routing it to land.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Once you have routed a plane to land, 3 waypoints are created. One in the air infront of the runway (blue), one where the plane lands (purple), and one where the plane completes its flight (green).", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("The bigger the plane, the further away the blue waypoint in front of the runway will be placed. Then, once a plane passes the purple waypoint, it's on the ground.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Finally, the green waypoint is the end of route where points will be collected. Also, note that deleting any of the 3 landing waypoints will delete all of them.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Now, go ahead and try to get this plane landed safely before continuing. You can also pan the camera with right click and zoom with the scroll wheel.");
        yield return WaitForPlaneLanded();

        ShowTutorialMessage("Congratulations! You're now a totally certified Air Traffic Controller! Please don't crash any planes... and good luck! I've turned on plane spawning so you can practice or go to the main menu.");
        StartCoroutine(planeSpawn.PlaneSpawnLoop());
        returnToMainMenuButton.SetActive(true);
    }

    private void ShowTutorialMessage(string message, bool freezeTime = false) {
        if(freezeTime) {
            Time.timeScale = 0f;
        }

        tutorialText.text = message;
    }
}