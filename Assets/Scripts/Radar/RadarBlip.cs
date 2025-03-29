using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarBlip : MonoBehaviour
{
    public float planeSpeed;

    private void Start() {
        StartCoroutine(DeleteAfterTime());
        AudioManager.Instance.PlayRadarPingSound();
    }

    public IEnumerator DeleteAfterTime() {
        float t = 3f / planeSpeed;
        yield return new WaitForSeconds(t);

        Destroy(gameObject);
    }

}
