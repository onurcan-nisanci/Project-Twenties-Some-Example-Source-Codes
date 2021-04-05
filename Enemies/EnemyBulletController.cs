using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletController : MonoBehaviour
{
    [SerializeField] float Speed = 4f;
    [SerializeField] WeaponType WeaponType;
    [SerializeField] GameObject CollisionEffect;
    [SerializeField] GameObject CollisionEffect02;
    [SerializeField] GameObject ElectricityCollisionEffect;
    [SerializeField] GameObject CoverableCollisionEffect01;
    [SerializeField] LayerMask WhatToHit;

    #region Fields
    private SpriteRenderer _spriteRenderer;
    private PlayerController _player;
    private Color _colorOfSprite;
    private bool _hasBottleBroken;
    #endregion

    // Boundary values
    private Vector2 _minBoundary;
    private Vector2 _maxBoundary;
    [HideInInspector] public float ScaleXValue;

    // Start is called before the first frame update
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _colorOfSprite = _spriteRenderer.GetComponent<SpriteRenderer>().color;

        _minBoundary = Camera.main.ViewportToWorldPoint(new Vector2(1, 1)); // top-right
        _maxBoundary = Camera.main.ViewportToWorldPoint(new Vector2(0, 0)); // bottom-left

        if(ScaleXValue == 1f)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        _player = GameManager.Instance.PlayerGO;
    }

    // Update is called once per frame
    void Update()
    {
        BulletMovement();
        UpdateOffensiveState();
    }

    void BulletMovement()
    {
        Vector2 pos = transform.position;

        if (ScaleXValue == 1)
            pos = new Vector2(pos.x + Speed * Time.deltaTime, pos.y);
        else
            pos = new Vector2(pos.x - Speed * Time.deltaTime, pos.y);

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
        Vector2 dir = (ScaleXValue == 1) ? Vector2.right : Vector2.left;


        BulletRay(firePos, dir);
    }

    void UpdateOffensiveState()
    {
        if (CheckIfPlayerIsRolling())
        {
            _colorOfSprite.a = 0.25f;
        }
        else
        {
            _colorOfSprite.a = 1f;
        }

        _spriteRenderer.GetComponent<SpriteRenderer>().color = _colorOfSprite;
    }

    bool CheckIfPlayerIsRolling()
    {
        if(_player.IsPlayerRolling())
        {
            if ((ScaleXValue == 1f && transform.position.x < _player.transform.position.x) ||
                (ScaleXValue == -1f && transform.position.x > _player.transform.position.x))
            {
                return true;
            }            
        }

        return false;
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

            if (hitInfo.transform.gameObject.tag == "Foreground" || hitInfo.transform.gameObject.tag.Equals("Door"))
            {
                // First instantiate collision effect then destroy the bullet
                var collEffectGO = Instantiate(CollisionEffect, transform.position, Quaternion.identity);
                if (ScaleXValue < 0)
                    collEffectGO.transform.localScale = new Vector2(-1f, 1f);

                Destroy(gameObject);
            }

            if(hitInfo.transform.gameObject.tag == "Coverable Object")
            {
                float additionalCollEffectPosX = -0.2f;
                if (ScaleXValue < 0)
                    additionalCollEffectPosX = Mathf.Abs(additionalCollEffectPosX);

                Vector2 collEffectPos = new Vector2(transform.position.x + additionalCollEffectPosX, transform.position.y);

                var collEffectGO = Instantiate(CollisionEffect02, collEffectPos, Quaternion.identity);
                if (ScaleXValue < 0)
                    collEffectGO.transform.localScale = new Vector2(-1f, 1f);

                Instantiate(CoverableCollisionEffect01, transform.position, Quaternion.identity);

                CoverPerformableObjectBehaviour coverObj = hitInfo.transform.parent.GetComponent<CoverPerformableObjectBehaviour>();

                if (coverObj != null)
                    coverObj.GetDamage(15f);

                Destroy(gameObject);
            }

            if (hitInfo.transform.gameObject.tag == "Bottle")
            {
                // Break bottle
                BottleBehaviour bottle = hitInfo.transform.GetComponent<BottleBehaviour>();

                if (bottle != null && !_hasBottleBroken)
                {
                    bool result = bottle.BreakBottle();
                    _hasBottleBroken = result;
                }
            }

            PlayerController player = hitInfo.transform.GetComponent<PlayerController>();

            if (player != null)
            {
                // Check if bullet is colliding with umbrella
                if(player.IsPlayerCoveredByUmbrella() && !ScaleXValue.Equals(player.transform.localScale.x))
                {
                    Instantiate(ElectricityCollisionEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                    return;
                }

                Destroy(gameObject);
                // If bullet collided with player

                player.GetDamage(1f, WeaponType, 0);
            }
        }
    }
}
