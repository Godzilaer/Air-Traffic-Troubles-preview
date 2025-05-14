using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour {
    [SerializeField] private PlaneSpawn planeSpawn;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject returnToMainMenuButton;
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Transform aircraftHolder;

    public static bool waitingForPlaneSelection;

    private bool continueButtonPressed;
    private bool planeSelected;
    private bool planeLanded;
    //If the user lands the plane too early in the tutorial this will be true. Then, another plane will be spawned so the user can land that one.
    private bool planeLandedEarly;
    private bool waitingForPlaneLanding;

    private void Start() {
        GameManager.planeLandedEvent += BindToPlaneLanding;
        StartCoroutine(Sequence());
    }

    public void OnContinueButtonPressed() {
        if(!UIManager.isPauseMenuActive) {
            continueButtonPressed = true;
        }
    }

    public void OnReturnToMainMenuButtonPressed() {
        SceneManager.LoadScene("TitleScreen");
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
            if(planeLandedEarly) {
                planeLandedEarly = false;
                GameManager.planeLandedEvent += BindToPlaneLanding;
                planeSpawn.SpawnPlane(PlaneSpawn.Area.RadarEdge, chosenAircraftTypeForTutorial: PlaneData.AircraftType.DualJet);
            }

            yield return null;
        }

        waitingForPlaneSelection = false;
        planeSelected = false;
    }

    private IEnumerator WaitForPlaneLanded() {
        waitingForPlaneLanding = true;

        //If a plane didn't land early the event is already binded, so only bind it if the plane landed early
        if(planeLandedEarly) {
            GameManager.planeLandedEvent += BindToPlaneLanding;
        }

        while (!planeLanded) {
            yield return null;
        }

        waitingForPlaneLanding = false;
        planeLanded = false;
    }

    private IEnumerator WaitForAllAircraftLanded() {
        while(aircraftHolder.childCount > 0) {
            yield return null;
        }
    }

    private void BindToFirstPlaneSelection() {
        GameManager.planeSelectedEvent -= BindToFirstPlaneSelection;
        planeSelected = true;
    }

    private void BindToPlaneLanding() {
        GameManager.planeLandedEvent -= BindToPlaneLanding;

        if(!waitingForPlaneLanding) {
            planeLandedEarly = true;
        } else {
            planeLanded = true;
        }
    }

    private IEnumerator Sequence() {
        ShowTutorialMessage("Welcome to Air Traffic Troubles! This short training will get you controlling aircraft in no time. Let's begin!", true);
        yield return WaitForContinuation();

        planeSpawn.SpawnPlane(PlaneSpawn.Area.RadarEdge, chosenAircraftTypeForTutorial: PlaneData.AircraftType.DualJet);
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

        ShowTutorialMessage("I've paused the game for a moment. Planes need to land. When you click on one of the yellow boxes on a landing area with a plane selected, you are routing it to land.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Once you have routed a plane to land, 3 waypoints are created. One in the air infront of the runway (blue), one where the plane lands (purple), and one where the plane completes its flight (green).", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("The bigger the plane, the further away the blue waypoint in front of the runway will be placed. Then, once a plane passes the purple waypoint, it's on the ground.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Finally, the green waypoint is the end of route where points will be collected. Also, note that deleting any of the 3 landing waypoints will delete all of them.", true);
        yield return WaitForContinuation();

        ShowTutorialMessage("Now, note the 3 different landing areas. The long runway can land any plane besides a helicopter. The short runway can land only Regional Jets and General Aviation aircraft. The helipad can land only helicopters.", true);
        yield return WaitForContinuation();

        if(planeLandedEarly) {
            planeSpawn.SpawnPlane(PlaneSpawn.Area.RadarEdge, chosenAircraftTypeForTutorial: PlaneData.AircraftType.DualJet);
        }
        ShowTutorialMessage("Now, go ahead and try to get this plane landed safely before continuing. This is a Dual Jet, so you should bring it to the long runway. You can also zoom with the scroll wheel and pan the camera by dragging right click .");
        yield return WaitForPlaneLanded();

        planeSpawn.SpawnPlane(PlaneSpawn.Area.RadarEdge, chosenAircraftTypeForTutorial: PlaneData.AircraftType.Helicopter);
        ShowTutorialMessage("Nice! Here's a helicopter. They are landed the same a way a plane is, but only on a helipad. However, note that helicopters take 3 seconds to leave the helipad after landing. Bring this one in for landing now.");
        yield return WaitForContinuation();

        planeSpawn.SpawnPlane(PlaneSpawn.Area.RadarEdge, chosenAircraftTypeForTutorial: PlaneData.AircraftType.RegionalJet);
        ShowTutorialMessage("Now land this Regional Jet too. Remember that you can land this aircraft on either the long or short runway, it's your choice! Also, make sure to avoid hitting the helicopter.");
        yield return WaitForContinuation();

        ShowTutorialMessage("Wait for all the aircraft to land.");
        yield return WaitForAllAircraftLanded();

        StartCoroutine(planeSpawn.PlaneSpawnLoop());
        ShowTutorialMessage("Congratulations! You've grasped the basics. Now you can keep practicing here or go straight into the levels. Please don't crash any planes... and good luck!");

        returnToMainMenuButton.SetActive(true);
    }

    private void ShowTutorialMessage(string message, bool freezeTime = false) {
        if (freezeTime) {
            Time.timeScale = 0f;
        }

        tutorialText.text = message;
    }
}