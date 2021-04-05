using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    [SerializeField] bool isItSingleShell;
    [SerializeField] bool isItMagazine;
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private bool _isClipPlayed;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (isItSingleShell)
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 50f));
    }

    void DecreaseOpacityThenDestroy()
    {
        if (_spriteRenderer.color.a >= 0.1f)
        {
            Color curColor = _spriteRenderer.color;
            curColor.a -= 0.1f;
            _spriteRenderer.color = curColor;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void ConstrainRigidbodyXMovement()
    {
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!_isClipPlayed)
        {
            _audioSource.Play();
            _isClipPlayed = true;
        }

        Invoke("ConstrainRigidbodyXMovement", 1f);

        if(isItMagazine || isItSingleShell)
            InvokeRepeating("DecreaseOpacityThenDestroy", 3f, 0.1f);
        else
            Destroy(transform.parent.gameObject, 2f);
    }
}
