using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehaviour : MonoBehaviour
{
    [SerializeField] bool IsItLeft;
    [SerializeField] DoorknobBehaviour DoorknobGO;
    private PlayerController _player;
    private Animator _animator;
    private BoxCollider2D _boxCollider2D;
    private SpriteRenderer _spriteRenderer;
    private AudioSource _audioSource;
    private bool _hasItOpened;
    private bool _hasDustEffectInstantiated;
    private short _type;

    #region SFX
    [SerializeField] AudioClip[] ImpactEffects;
    #endregion

    #region VFX
    [SerializeField] ParticleSystem DustEffectRight;
    [SerializeField] ParticleSystem DustEffectLeft;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _player = FindObjectOfType<PlayerController>();
        _type = 1;
    }

    void Update()
    {
        if (IsPlayerCloseEnough() && !_hasItOpened)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Check if player is looking correct direction while he is kicking door
                if((IsItLeft && _player.IsFacingRight()) || (!IsItLeft && !_player.IsFacingRight()))
                    _player.Kicking(this);
            }

            if (_player.IsPlayerKickedByEnemy() && ((IsItLeft && !_player.IsFacingRight()) || (!IsItLeft && _player.IsFacingRight())))
            {
                _hasItOpened = true;
                _type = 2;
                Invoke("SetIsOpened", 0.2f);
            }
        }

        if(HasEnemyFallenToDoor())
        {
            _hasItOpened = true;
            _type = 2;
            Invoke("SetIsOpened", 0.1f);
        }
    }

    bool IsPlayerCloseEnough()
    {
        bool result = false;
        float castDist = 0.35f;

        if (IsItLeft)
            castDist = -castDist;

        Vector2 endPos = transform.position + Vector3.right * castDist;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, endPos, 1 << LayerMask.NameToLayer("Player"));

        if (hit.collider != null)
        {
            result = true;
            Debug.DrawLine(transform.position, hit.point, Color.green);
        }
        else
        {
            result = false;
            Debug.DrawLine(transform.position, endPos, Color.red);
        }

        return result;
    }

    bool HasEnemyFallenToDoor()
    {
        bool result = false;
        float castDist = 0.35f;

        if (IsItLeft)
            castDist = -castDist;

        Vector2 endPos = transform.position + Vector3.right * castDist;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, endPos, 1 << LayerMask.NameToLayer("Breakables Collider"));

        if (hit.collider?.tag.Equals("Door Collider") ?? false)
            result = true;
        else
            result = false;

        return result;
    }

    public void SetIsOpened()
    {
        _hasItOpened = true;

        if (_animator != null)
        {
            _animator.SetBool("IsItLeft", IsItLeft);

            if (_type == 1)
                _animator.SetBool("IsTypeOneOpened", true);
            else
            {
                InstantiateDustEffect();
                _animator.SetBool("IsTypeTwoOpened", true);
            }
        }
    }

    void InstantiateDoorknobObject()
    {
        float additionalXPos = IsItLeft ? 0.4f : -0.4f;
        float additionalYPos = 0.09f;

        Vector2 doorknobPos = new Vector2(transform.position.x + additionalXPos, transform.position.y + additionalYPos);
        DoorknobBehaviour doorknobBehaviourGO = Instantiate(DoorknobGO, doorknobPos, Quaternion.identity);
        if (IsItLeft)
            doorknobBehaviourGO.transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void InstantiateDustEffect()
    {
        if (_hasDustEffectInstantiated)
            return;

        _hasDustEffectInstantiated = true;

        if (IsItLeft)
        {
            Vector2 dustPos = new Vector2(transform.position.x + 0.1f, transform.position.y);
            Instantiate(DustEffectLeft, dustPos, Quaternion.Euler(-90, 0, 0));
        }
        else
        {
            Vector2 dustPos = new Vector2(transform.position.x - 0.1f, transform.position.y);
            Instantiate(DustEffectRight, dustPos, Quaternion.Euler(-90, 0, 0));
        }
    }

    void PlayKickSfx()
    {
        if (_audioSource.isPlaying)
            return;

        if (_type == 1)
            _audioSource.clip = ImpactEffects[0];
        else
            _audioSource.clip = ImpactEffects[1];


        _audioSource.Play();
    }

    void PlayGroundCollideSfx()
    {
        _audioSource.clip = ImpactEffects[2];
        _audioSource.Play();
    }

    void DestroyColliders()
    {
        Destroy(transform.GetChild(0).gameObject);
        Destroy(transform.GetChild(1).gameObject);
        Destroy(_boxCollider2D);
    }

    void DestroyComponents()
    {
        Destroy(_animator);
        Destroy(_audioSource, 1f);
        Destroy(this);
    }
}
