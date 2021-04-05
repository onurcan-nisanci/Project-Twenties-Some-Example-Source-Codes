using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBulletController : MonoBehaviour
{
    [SerializeField] float Speed = 6f;
    [SerializeField] GameObject CollisionEffect;
    [SerializeField] LayerMask WhatToHit;
    [SerializeField] RotationType BulletRotationType;
    [SerializeField] bool CanInstantiateBloodEffect;
    private PlayerController _playerController;
    private bool _hasBottleBroken;

    // Boundary values
    private Vector2 _minBoundary;
    private Vector2 _maxBoundary;
    private float _scaleXValue;
    private float _rotationAngle = 5f;
    private float _pushForce = 58f;
    private short _amountOfDamageToReduce;

    // Start is called before the first frame update
    void Start()
    {
        _playerController = FindObjectOfType<PlayerController>();

        _scaleXValue = _playerController.transform.localScale.x;

        if (_scaleXValue == 1f && _playerController.IsAimingToLeftOnCover())
            _scaleXValue = -1f;
        else if (_scaleXValue == -1f && _playerController.IsAimingToLeftOnCover())
            _scaleXValue = 1f;

        if (_scaleXValue == -1f)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        SetBulletRotations();

        _minBoundary = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)); // top-right
        _maxBoundary = Camera.main.ViewportToWorldPoint(new Vector2(0, 0)); // bottom-left

        InvokeRepeating("DecreasePushForceAndDamage", 0.1f, 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        BulletMovement();
    }

    void SetBulletRotations()
    {
        if(BulletRotationType == RotationType.RotationDown)
        {
            if (_scaleXValue == 1)
                transform.rotation = Quaternion.Euler(0, 0, -_rotationAngle);
            else
                transform.rotation = Quaternion.Euler(0, 0, _rotationAngle);

        } else if(BulletRotationType == RotationType.RotationUp)
        {
            if (_scaleXValue == 1)
                transform.rotation = Quaternion.Euler(0, 0, _rotationAngle);
            else
                transform.rotation = Quaternion.Euler(0, 0, -_rotationAngle);
        }
    }

    void BulletMovement()
    {
        Vector3 pos = transform.position;

        if (_scaleXValue == 1)
        {
            //pos = new Vector2(pos.x + Speed * Time.deltaTime, pos.y);
            pos += transform.right * Time.deltaTime * Speed;
        }
        else
        {
            //pos = new Vector2(pos.x - Speed * Time.deltaTime, pos.y);
            pos -= transform.right * Time.deltaTime * Speed;
        }

        transform.position = pos;

        if (pos.x > _minBoundary.x + 0.25f || pos.x < _maxBoundary.x - 0.25f)
        {
            // Release bullet trail from gameobject
            if (transform.childCount > 0)
            {
                GameObject bulletTrail = transform.GetChild(0).gameObject;
                bulletTrail.transform.parent = null;
                bulletTrail.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
            }

             Destroy(gameObject);
        }

        Vector2 firePos = new Vector2(transform.position.x, transform.position.y);
        Vector2 dir = (_scaleXValue == 1) ? Vector2.right : Vector2.left;


        BulletRay(firePos, dir);
    }

    void DecreasePushForceAndDamage()
    {
        _pushForce -= 2f;
        _amountOfDamageToReduce += 1;
    }

    void BulletRay(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hitInfo;

        hitInfo = Physics2D.Raycast(origin, direction, -0.5f, WhatToHit);

        if (hitInfo.collider != null)
        {
            // Release bullet trail from gameobject
            if (transform.childCount > 0)
            {
                GameObject bulletTrail = transform.GetChild(0).gameObject;
                bulletTrail.transform.parent = null;
                bulletTrail.transform.localScale = new Vector3(0.05f, 0.05f, 1f);
            }

            if (hitInfo.transform.gameObject.tag.Equals("Foreground") || hitInfo.transform.gameObject.tag.Equals("Door"))
            {
                //First instantiate collision effect then destroy the bullet.

                float additionalCollEffectPosX = -0.2f;
                if (_scaleXValue < 0)
                    additionalCollEffectPosX = Mathf.Abs(additionalCollEffectPosX);

                Vector2 collEffectPos = new Vector2(transform.position.x + additionalCollEffectPosX, transform.position.y);

                var collEffectGO = Instantiate(CollisionEffect, collEffectPos, Quaternion.identity);
                if (_scaleXValue < 0)
                    collEffectGO.transform.localScale = new Vector2(-1f, 1f);

                float additionalCollEffect2PosX = -0.1f;

                if (_scaleXValue < 0)
                    additionalCollEffect2PosX = Mathf.Abs(additionalCollEffect2PosX);

                Destroy(gameObject);
            }

            if (hitInfo.transform.gameObject.tag.Equals("Bottle"))
            {
                // Break bottle
                BottleBehaviour bottle = hitInfo.transform.GetComponent<BottleBehaviour>();

                if (bottle != null && !_hasBottleBroken)
                {
                    bool result = bottle.BreakBottle();
                    _hasBottleBroken = result;
                }
            }

            EnemyController enemy = hitInfo.transform.GetComponent<EnemyController>();

            if (enemy != null)
            {
                Destroy(gameObject);
                enemy.GetDamage(_scaleXValue, WeaponType.Shotgun, _amountOfDamageToReduce, _pushForce, CanInstantiateBloodEffect);
            }
        }
    }
}
