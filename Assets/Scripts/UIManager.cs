using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Text UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI aircraftServedText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Update()
    {
        if (GameManager.gameOver) {
            return;
        }

        scoreText.text = "Score: " + GameManager.score.ToString();
        aircraftServedText.text = "Aircraft Served: " + GameManager.aircraftServed.ToString();
        timeText.text = GetReadableTime();
    }

    private string GetReadableTime()
    {
        var minutes = Mathf.Floor(Time.time / 60f);
        string seconds = Mathf.Floor(Time.time - 60f * minutes).ToString();

        if (int.Parse(seconds) < 10)
        {
            seconds = "0" + seconds;
        }

        return "Time: " + minutes + ":" + seconds;
    }
}
