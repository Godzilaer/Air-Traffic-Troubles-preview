using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarBlip : MonoBehaviour
{
    public float planeSpeed;

    private void Start() {
        StartCoroutine(DeleteAfterTime()//);
    }

    public IEnumerator DeleteAfterTime() {
        float t = 2.5f / planeSpeed;
        yield return new WaitForSeconds(t);

        Destroy(gameObject);
    }

}
