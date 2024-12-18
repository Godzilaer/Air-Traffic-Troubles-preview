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

    private void Update() {
        callsignText.text = planeControl.planeData.callsign;
        delayText.text = planeControl.planeData.delay.ToString();

        Vector3 yOffsetVec = new Vector3(0f, yOffset, 0f);
        callsign.position = transform.position + yOffsetVec;
        delay.position = transform.position - yOffsetVec;
    }
}
