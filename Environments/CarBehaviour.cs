using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    [SerializeField] float Speed;
    private bool _isItGoingRight;
    private Renderer _renderer;
    private bool _hasItAppeared;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _isItGoingRight = transform.localScale.x < 0;
        if (!_isItGoingRight)
            Speed = -Speed;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        transform.position = new Vector3(transform.position.x + Speed * Time.deltaTime, transform.position.y, transform.position.z);

        if(!_hasItAppeared)
        {
            if (_renderer.isVisible)
                _hasItAppeared = true;
        }

        // If car has appeared on camera then left the camera, destroy the object.
        if (_hasItAppeared && !_renderer.isVisible)
            Destroy(gameObject, 3f);
    }
}
