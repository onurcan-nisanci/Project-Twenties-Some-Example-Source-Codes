using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodyBulletController : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private float _scaleX;
    private float _randConstrainRigidTime;
    private bool _allowMoving;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _scaleX = transform.localScale.x;
        _allowMoving = true;
        _randConstrainRigidTime = Random.Range(0.15f, 0.5f);
    }

    void FixedUpdate()
    {
        ApplyForce();
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
        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX;
    }

    void ApplyForce()
    {
        if (!_allowMoving)
            return;

        if (_scaleX > 0)
            _rigidbody2D.velocity = new Vector2(-50f * Time.fixedDeltaTime, _rigidbody2D.velocity.y);
        else
            _rigidbody2D.velocity = new Vector2(50f * Time.fixedDeltaTime, _rigidbody2D.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _allowMoving = false;
        Invoke("ConstrainRigidbodyXMovement", _randConstrainRigidTime);
        InvokeRepeating("DecreaseOpacityThenDestroy", 1f, 0.1f);
    }
}
