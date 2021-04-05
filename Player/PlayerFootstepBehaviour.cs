using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFootstepBehaviour : MonoBehaviour
{
    private AudioSource _audioSource;
    private PlayerController _player;

    #region SFX 
    [SerializeField] AudioClip StartRunningClip;
    [SerializeField] AudioClip RunningClip;
    [SerializeField] AudioClip StoppingClip;
    #endregion

    #region VFX
    [SerializeField] ParticleSystem StartRunningDustEffect;
    [SerializeField] public ParticleSystem RunningDustEffect;
    [SerializeField] public ParticleSystem StopRunningDustEffect;
    [SerializeField] public ParticleSystem RollingDustEffect;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = transform.GetComponent<AudioSource>();
        _player = transform.parent.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayFootstepsByConditions();
    }

    public void InstantiateEffect(ParticleSystem desiredEffect, float extraXPos = 0)
    {
        Vector2 pos = new Vector2(transform.position.x + extraXPos, transform.position.y - 0.35f);
        Instantiate(desiredEffect, pos, Quaternion.Euler(-90, 0, 0));
    }

    void PlayFootstepsByConditions()
    {
        PlayerMovementState movementState = _player.GetPlayerMovementState();

        switch (movementState)
        {
            case PlayerMovementState.RunningStarted:
                if(_audioSource.clip != StartRunningClip)
                {
                    _audioSource.clip = StartRunningClip;
                    InstantiateEffect(StartRunningDustEffect);
                }
                break;
            case PlayerMovementState.Running:
                if (_audioSource.isPlaying)
                    return;

                _audioSource.clip = RunningClip;
                break;
            case PlayerMovementState.Stopping:
                if(_audioSource.clip != StoppingClip)
                    _audioSource.clip = StoppingClip;
                break;
            case PlayerMovementState.NotRelated:
                _audioSource.Stop();
                return;
            default:
                break;
        }

        if (!_audioSource.isPlaying)
            _audioSource.Play();
    }

    public void DestroyGameobject()
    {
        Destroy(gameObject);
    }
}
