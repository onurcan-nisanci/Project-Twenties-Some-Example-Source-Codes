using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleBehaviour : MonoBehaviour
{
    [SerializeField] GameObject BottleCap;
    [SerializeField] ParticleSystem BeverageEffect;
    private Animator _animator;
    private AudioSource _audioSource;
    private Rigidbody2D _rigidbody2d;
    private SpriteRenderer _spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public bool BreakBottle()
    {
        if (_animator == null)
            return false;

        Destroy(GetComponent<PolygonCollider2D>());
        _animator.SetBool("HasShattered", true);
        _audioSource.PlayOneShot(_audioSource.clip);
        InvokeRepeating("DecreaseOpacityThenDestroy", 0, 0.25f);

        return true;
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

    void InstantiateBottleCap()
    {
        Vector2 pos = new Vector2(transform.position.x + 0.048f, transform.position.y + 0.062f);
        Instantiate(BottleCap, pos, Quaternion.identity);

        // Instantiate beverage
        Instantiate(BeverageEffect, transform.position, Quaternion.identity);
    }

    void DestroyComponents()
    {
        Destroy(_rigidbody2d);
        Destroy(_animator);
        Destroy(_audioSource, 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Foreground")
            BreakBottle();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Breakables Collider")
            BreakBottle();
    }
}
