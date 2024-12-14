using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneData {
    [Header("Set These Values")]
    public float speed;
    public bool hasAirlinerCallsign;

    [Header("Main Data")]
    public string callsign;
    //The current "real" position of the plane. Once the radar scan line touches the plane its position is set to this value
    public Vector2 realPos = Vector2.zero;

    public List<Waypoint.Internal> internalWaypoints = new List<Waypoint.Internal>();
    public List<Waypoint.Visual> visualWaypoints = new List<Waypoint.Visual>();

    [Header("Booleans")]
    public bool isSelected = false;
    public bool onGround = false;
    public bool routedToRunway = false;

    public void Initialize(Vector2 pos) {
        callsign = hasAirlinerCallsign ? CallsignManager.GetAirlinerCallsign() : CallsignManager.GetGeneralAviationCallsign();
        realPos = pos;
    }
}
