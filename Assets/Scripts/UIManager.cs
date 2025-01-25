using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour {
    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI aircraftServedText;
    [SerializeField] private TextMeshProUGUI delayStrikesText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Update() {
        scoreText.text = "Score: " + GameManager.score.ToString();
        aircraftServedText.text = "Aircraft Served: " + GameManager.aircraftServed.ToString();
        delayStrikesText.text = "Delay Strikes: " + GameManager.delayStrikes.ToString() + "/3";
        timeText.text = GetReadableTime();
    }

    private string GetReadableTime() {
        var minutes = Mathf.Floor(GameManager.time / 60f);
        string seconds = Mathf.Floor(GameManager.time - 60f * minutes).ToString();

        if (int.Parse(seconds) < 10) {
            seconds = "0" + seconds;
        }

        return "Time: " + minutes + ":" + seconds;
    }
}