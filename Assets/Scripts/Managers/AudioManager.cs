using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sounds")]
    [SerializeField] private AudioSource explosionAudio;
    [SerializeField] private AudioSource radarPingAudio;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
    }

    public void PlayExplosionSound() {
        explosionAudio.Play();
    }

    public void PlayRadarPingSound() {
        radarPingAudio.Play();
    }
}
