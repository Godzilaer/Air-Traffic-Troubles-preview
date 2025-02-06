using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RadarLineDrawerData))]
public class RadarLineDrawer : Editor {
    private RadarLineDrawerData data;

    private void OnEnable() {
        data = (RadarLineDrawerData)target;
        EditorApplication.update += DrawCircles; // Subscribe to the update loop
    }

    private void OnDisable() {
        EditorApplication.update -= DrawCircles; // Unsubscribe to avoid memory leaks
    }
    
    void DrawCircles() {
        int segments = data.segments;
        float[] radii = data.radii;
        LineRenderer lineRenderer = data.lineRenderer;

        lineRenderer.positionCount = 0;
        lineRenderer.positionCount = (segments + 2) * radii.Length;

        float angleStep = 360f / segments;
        Vector3[] points = new Vector3[lineRenderer.positionCount];

        for(int r = 0; r < radii.Length; r++) {
            for (int i = 0; i < segments; i++) {
                float angle = Mathf.Deg2Rad * i * angleStep;
                Vector3 point = new Vector3(Mathf.Cos(angle) * radii[r], Mathf.Sin(angle) * radii[r], 0f);

                //Segments + 2 because each circle gets an extra 2 points at the end
                int relativeI = i + r * (segments + 2);
                points[relativeI] = point;
                if(i == segments - 1) {
                    points[relativeI + 1] = points[relativeI - i];
                    //Crazy value that cuts the line here so that the next circle is separate
                    points[relativeI + 2] = new Vector3(0f, 0f, 1000000f);
                }
            }


        }

        lineRenderer.SetPositions(points);
    }
}