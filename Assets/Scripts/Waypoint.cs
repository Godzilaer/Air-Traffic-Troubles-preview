using UnityEngine;

public class Waypoint
{
    public Vector2 position;
    public WaypointType type;
    public GameObject node;
}
public enum WaypointType
{
    Path, Transition, Terminus
}