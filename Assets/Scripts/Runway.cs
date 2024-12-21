using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : MonoBehaviour {
    [HideInInspector]
    public Transform oppositeRunway;

    private void Awake() {
        string targetName = gameObject.name == "1" ? "2" : "1";
        oppositeRunway = transform.parent.Find(targetName);
    }
}
