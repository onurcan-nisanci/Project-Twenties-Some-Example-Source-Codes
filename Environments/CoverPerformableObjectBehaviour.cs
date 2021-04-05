using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverPerformableObjectBehaviour : MonoBehaviour
{
    [SerializeField] bool HasItFlipped;
    [SerializeField] Sprite[] Damages;
    [SerializeField] AudioClip DestorySfx;
    [SerializeField] ParticleSystem CardScatterEffectRight;
    [SerializeField] ParticleSystem CardScatterEffectLeft;
    [SerializeField] ParticleSystem PokerChipsEffectRight;
    [SerializeField] ParticleSystem PokerChipsEffectLeft;
    private PlayerController _player;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private AudioSource _audioSource;
    private BoxCollider2D _boxCollider2d;
    private GameObject _coverColliderGo;
    private bool _hasDestroyed;
    private float _conditionValue;
    private bool _hasAnimationFinished;


    // Start is called before the first frame update
    void Start()
    {
        _player = FindObjectOfType<PlayerController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _boxCollider2d = GetComponent<BoxCollider2D>();
        _conditionValue = 100f;

        if (HasItFlipped)
            _coverColliderGo = transform.GetChild(0).gameObject;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Player" && !_hasDestroyed)
        {
            if(Input.GetKeyDown(KeyCode.F))
            {
                // Check if player is ready to take cover
                if (_player.CheckIfPlayerIsOnAction())
                    return;

                // If object hasn't flipped yet
                if (!HasItFlipped)
                {
                   var result = _player.TakeCoverToObject(transform.position.x, transform.localScale.x, true);
                   if(result)
                      Destroy(gameObject);
                }
                else
                {
                    _player.TakeCoverToObject(transform.position.x, transform.localScale.x, false, gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Set trigger collider for player off if player is on cover otherwise set it on.
    /// If player is on cover, set coverable object's collider on otherwise set it off.
    /// </summary>
    /// <param name="val"></param>
    public void SetPlayerTrigger(bool val)
    {
        if (_boxCollider2d == null || _coverColliderGo == null)
            return;

        _boxCollider2d.enabled = val;
        _coverColliderGo.SetActive(!val);
    }

    void AnimationHasFinished()
    {
        _hasAnimationFinished = true;
        _animator.enabled = false;
    }

    public void GetDamage(float amountOfDamage)
    {
        if (!_hasAnimationFinished)
            return;

        _conditionValue -= amountOfDamage;

        switch (_conditionValue)
        {
            case float n when (n >= 70 && n <= 100):
                _spriteRenderer.sprite = Damages[0];
                break;
            case float n when (n >= 40 && n <= 80):
                _spriteRenderer.sprite = Damages[1];
                break;
            case float n when (n >= 10 && n <= 40):
                _spriteRenderer.sprite = Damages[2];
                break;
            case float n when n <= 0:
                _player.LeaveCover(true);
                _animator.enabled = true;
                _hasDestroyed = true;
                _animator.SetBool("IsDestroyed", _hasDestroyed);
                break;
        }
    }

    void InstantiateCardScatterEffect()
    {
        if (transform.localScale.x > 0)
        {
            Vector2 pos = new Vector2(transform.position.x + 0.4f, transform.position.y + 0.08f);
            Instantiate(CardScatterEffectRight, pos, Quaternion.identity);
            Instantiate(PokerChipsEffectRight, pos, Quaternion.identity);
        } else
        {
            Vector2 pos = new Vector2(transform.position.x - 0.4f, transform.position.y + 0.08f);
            Instantiate(CardScatterEffectLeft, pos, Quaternion.identity);
            Instantiate(PokerChipsEffectLeft, pos, Quaternion.identity);
        }

    }

    void PlayDestroySfx()
    {
        _audioSource.clip = DestorySfx;
        _audioSource.Play();
    }

    void DestroyComponents()
    {
        Destroy(GetComponent<BoxCollider2D>());
        if (_coverColliderGo != null)
            Destroy(_coverColliderGo);

        Destroy(_animator);
        Destroy(_audioSource, 2f);
        Destroy(this, 2f);
    }
}