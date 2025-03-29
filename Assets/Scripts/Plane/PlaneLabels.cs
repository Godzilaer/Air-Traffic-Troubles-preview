using UnityEngine;
using TMPro;

public class PlaneLabels : MonoBehaviour {
    public float yOffset;

    private Transform callsign;
    private Transform delay;
    private TextMeshPro callsignText;
    private TextMeshPro delayText;

    private PlaneControl planeControl;

    private void Start() {
        Transform labelHolder = GameObject.Find("PlaneLabels").transform;

        callsign = Instantiate(Resources.Load<Transform>("Callsign"), transform.position, Quaternion.identity, labelHolder);
        callsignText = callsign.GetComponent<TextMeshPro>();
        delay = Instantiate(Resources.Load<Transform>("Delay"), transform.position, Quaternion.identity, labelHolder);
        delayText = delay.GetComponent<TextMeshPro>();

        planeControl = GetComponent<PlaneControl>();
    }

    public void DeleteLabels() {
        Destroy(callsign.gameObject);
        Destroy(delay.gameObject);
    }

    private void Update() {
        callsignText.text = planeControl.planeData.callsign;

        float delayTime = Mathf.Round(planeControl.planeData.delayTime * 10f) / 10f;

        string delayTimeText;
        if (delayTime > 0) {
            delayTimeText = "+" + delayTime.ToString("0.0");
        } else {
            delayTimeText = delayTime.ToString("0.0");
        }
        delayText.text = delayTimeText;

        Color delayColor = Color.white;

        if (delayTime < 10f) {
            delayColor = Color.yellow;
        } else if (delayTime < -10f) {
            delayColor = new Color(0.94f, 0.56f, 0.12f); //Orange
        } else if (delayTime < -20f) {
            delayColor = Color.red;
        }

        delayText.color = delayColor;

        Vector3 yOffsetVec = new Vector3(0f, yOffset, 0f);
        callsign.position = transform.position + yOffsetVec;
        delay.position = transform.position - yOffsetVec;
    }
}
