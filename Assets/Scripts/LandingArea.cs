using System.Collections.Generic;
using UnityEngine;

public class LandingArea : MonoBehaviour {
    public Type type;

    [HideInInspector]
    public Transform oppositeRunway;

    public List<PlaneData.AircraftType> allowedAircraft = new List<PlaneData.AircraftType>();

    public enum Type {
        LongRunway, ShortRunway, Helipad
    }

    private void Awake() {
        if (type != Type.Helipad) {
            string targetName = gameObject.name == "1" ? "2" : "1";
            oppositeRunway = transform.parent.Find(targetName);
        }

        switch (type) {
            case Type.Helipad:
                allowedAircraft.Add(PlaneData.AircraftType.Helicopter);
                break;
            case Type.ShortRunway:
                allowedAircraft.Add(PlaneData.AircraftType.GeneralAviation);
                allowedAircraft.Add(PlaneData.AircraftType.RegionalJet);
                break;
            case Type.LongRunway:
                allowedAircraft.Add(PlaneData.AircraftType.GeneralAviation);
                allowedAircraft.Add(PlaneData.AircraftType.RegionalJet);
                allowedAircraft.Add(PlaneData.AircraftType.DualJet);
                allowedAircraft.Add(PlaneData.AircraftType.QuadJet);
                break;
        }

        RenderClickBox();
    }

    private void RenderClickBox() {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        Vector3[] points = new Vector3[4];

        lineRenderer.positionCount = 0;
        lineRenderer.positionCount = 4;

        float x = collider.size.x / 2f;
        float y = collider.size.y / 2f;
        Vector2 offset = collider.offset;

        // Points with offset
        points[0] = transform.TransformPoint(new Vector3(x, offset.y + y));
        points[1] = transform.TransformPoint(new Vector3(-x, offset.y + y));
        points[2] = transform.TransformPoint(new Vector3(-x, offset.y - y));
        points[3] = transform.TransformPoint(new Vector3(x, offset.y - y));

        lineRenderer.SetPositions(points);
    }
}
