using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelToSelect : MonoBehaviour, IPointerClickHandler {
    //id 0 is level 1
    public int id;

    private void Start() {
        gameObject.name = (id + 1).ToString(); 
        transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "Level " + gameObject.name;

        Transform thumbnail = transform.Find("Thumbnail");
        Transform highScore = transform.Find("HighScore");

        if (!UserData.LevelCompletion.CanUserAccessLevel(id)) {
            thumbnail.Find("Lock").gameObject.SetActive(true);
            highScore.gameObject.SetActive(false);
        //If this isn't true the label will just stay at its default of "No highscore yet"
        } else if (UserData.Instance.levelCompletion.completedLevelInfo.ContainsKey(id)) {
            highScore.GetComponent<TextMeshProUGUI>().text = "High Score: " + UserData.Instance.levelCompletion.completedLevelInfo[id].highScore.ToString();
        }

        //Instantiate(Resources.Load<GameObject>("MainMenu/LevelRunwayPreviews/" + (id + 1)), thumbnail);

        thumbnail.Find("RunwayPreview").GetComponent<Image>().sprite = Resources.Load<Sprite>("MainMenu/LevelRunwayPreviews/" + (id + 1));
    }

    public void OnPointerClick(PointerEventData eventData) {
        LevelSelectionManager.Instance.OnLevelClicked(id);
    }
}