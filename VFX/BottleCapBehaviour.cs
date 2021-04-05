using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleCapBehaviour : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        GetComponent<Rigidbody2D>().AddForce(new Vector2(15f, 0f));
    }

    void DecreaseOpacityThenDestroy()
    {
        if(_spriteRenderer.color.a >= 0.1f)
        {
            Color curColor = _spriteRenderer.color;
            curColor.a -= 0.1f;
            _spriteRenderer.color = curColor;
        } else
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Foreground")
        {
            InvokeRepeating("DecreaseOpacityThenDestroy", 0, 0.5f);
            _audioSource.PlayOneShot(_audioSource.clip);
        }
    }

}
