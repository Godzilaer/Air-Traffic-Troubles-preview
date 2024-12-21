using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlaneData {
    [Header("Set These Values")]
    public float speed;
    public string[] possibleAirlines;
    [Range(0f, 1f)]
    public float generalAviationCallsignChance;

    [Header("Main Data")]
    public string callsign;
    public float delay;
    //The current "real" position of the plane. Once the radar scan line touches the plane its position is set to this value
    public Vector2 realPos = Vector2.zero;

    public List<Waypoint.Internal> internalWaypoints = new List<Waypoint.Internal>();
    public List<Waypoint.Visual> visualWaypoints = new List<Waypoint.Visual>();

    public Color pathColor;

    [Header("Booleans")]
    public bool isSelected = false;
    public bool onGround = false;
    public bool routedToRunway = false;

    public void Initialize(Vector2 pos) {
        string airline = null;
        bool useGeneralAviationCallsign = false;
        
        if(generalAviationCallsignChance > Random.Range(0f, 1f)) {
            useGeneralAviationCallsign = true;
        } else {
            airline = possibleAirlines[Random.Range(0, possibleAirlines.Length - 1)];
        }

        callsign = useGeneralAviationCallsign ? CallsignManager.GetGeneralAviationCallsign() : CallsignManager.GetAirlinerCallsign(airline);
        realPos = pos;
    }
}
