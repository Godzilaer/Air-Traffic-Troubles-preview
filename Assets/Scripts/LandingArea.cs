using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingArea : MonoBehaviour {
    public Type type;

    [HideInInspector]
    public Transform oppositeRunway;

    public enum Type {
        LongRunway, ShortRunway, Helipad
    }

    private void Awake() {
        if(type != Type.Helipad) {
            string targetName = gameObject.name == "1" ? "2" : "1";
            oppositeRunway = transform.parent.Find(targetName);
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
        points[0] = transform.TransformPoint(new Vector3(x, y));
        points[1] = transform.TransformPoint(new Vector3(-x, y));
        points[2] = transform.TransformPoint(new Vector3(-x, -y));
        points[3] = transform.TransformPoint(new Vector3(x, -y));

        lineRenderer.SetPositions(points);
    }
}
