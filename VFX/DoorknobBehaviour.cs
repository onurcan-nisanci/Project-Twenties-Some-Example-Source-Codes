using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorknobBehaviour : MonoBehaviour
{
    private AudioSource _audioSource;
    private bool _isSfxPlayed;
    private BoxCollider2D _boxCollider2d;
    private Rigidbody2D _rigidbody2d;
    private bool _allowMoving;
    private float _scaleX;


    // Start is called before the first frame update
    void Start()
    {
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _boxCollider2d = GetComponent<BoxCollider2D>();
        _audioSource = GetComponent<AudioSource>();
        _scaleX = transform.localScale.x;
        _allowMoving = true;
    }

    void FixedUpdate()
    {
        ApplyForce();
    }

    void ApplyForce()
    {
        if (!_allowMoving)
            return;

        if (_scaleX > 0)
            _rigidbody2d.velocity = new Vector2(-25f * Time.fixedDeltaTime, _rigidbody2d.velocity.y);
        else
            _rigidbody2d.velocity = new Vector2(25 * Time.fixedDeltaTime, _rigidbody2d.velocity.y);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Foreground")
        {
            _audioSource.Play();
            _allowMoving = false;
        }

        Destroy(_rigidbody2d, 1f);
        Destroy(_boxCollider2d, 1f);
        Destroy(_audioSource, 2f);
        Destroy(this, 2f);
    }

}
