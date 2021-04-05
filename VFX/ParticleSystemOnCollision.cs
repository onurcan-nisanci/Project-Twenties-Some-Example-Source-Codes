using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemOnCollision : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private bool _hasSfxPlayed;

    // Start is called before the first frame update
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        float totalDuration = _particleSystem.main.duration;
        Invoke("PauseParticleSystem", totalDuration);
    }

    private void PauseParticleSystem()
    {
        GetComponent<ParticleSystem>().Pause();
    }


    private void OnParticleCollision(GameObject other)
    {
        if (_hasSfxPlayed)
            return;

        if (other.tag == "Foreground")
        {
            _hasSfxPlayed = true;
            GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
        }
    }
}
