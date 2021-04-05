using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponComponentsDestroyer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] bool IsItRevolverFired;
    [SerializeField] AudioClip ImpactClip;
    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Foreground")
        {
            Destroy(GetComponent<Rigidbody2D>(), 2f);
            Destroy(GetComponent<PolygonCollider2D>(), 2f);

            if(!IsItRevolverFired)
            {
                if (!_audioSource.isPlaying)
                    _audioSource.Play();

                Destroy(_audioSource, 2f);
                Destroy(this, 2f);
                return;
            }

            _audioSource.clip = ImpactClip;
            if(!_audioSource.isPlaying)
                _audioSource.Play();

            Destroy(_audioSource, 2f);
            Destroy(GetComponent<Animator>(), 2f);
            Destroy(this, 2f);
        }
    }
}