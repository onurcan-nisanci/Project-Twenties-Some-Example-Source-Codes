using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemDestroyer : MonoBehaviour
{
    private ParticleSystem _particleSystem;

    // Start is called before the first frame update
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        float totalDuration = _particleSystem.duration + _particleSystem.startLifetime;
        Destroy(gameObject, totalDuration);
    }
}
