using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Runway : MonoBehaviour {
    [HideInInspector]
    public Transform oppositeRunway;

    private LineRenderer lineRenderer;
    private BoxCollider2D boxCollider;

    private void Awake() {
        string targetName = gameObject.name == "1" ? "2" : "1";
        oppositeRunway = transform.parent.Find(targetName);

        lineRenderer = GetComponent<LineRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        Vector3[] points = new Vector3[4];

        lineRenderer.positionCount = 0;
        lineRenderer.positionCount = 4;

        float x = boxCollider.size.x / 2f;
        float y = boxCollider.size.y / 2f;
        points[0] = transform.TransformPoint(new Vector3(x, y));
        points[1] = transform.TransformPoint(new Vector3(-x, y));
        points[2] = transform.TransformPoint(new Vector3(-x, -y));
        points[3] = transform.TransformPoint(new Vector3(x, -y));
        
        lineRenderer.SetPositions(points);
    }
}
