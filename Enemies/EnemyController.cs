using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IBaseHuman
{
    #region Settings
    [SerializeField] short Health = 100;
    [SerializeField] float Speed;
    [SerializeField] [Range(0, 10)] float SightRange;
    [SerializeField] short RevolverDamage;
    [SerializeField] short Pistol1911Damage;
    [SerializeField] short ShotgunDamage;
    [SerializeField] short SubmachineDamage;
    [SerializeField] GameObject ExitTriggerGO;
    [SerializeField] LayerMask WhatToSee;
    [SerializeField] LayerMask WhatToKickAndCollide;
    [SerializeField] bool NoFall;
    [SerializeField] bool OnlyDie03;
    #endregion

    #region Components
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody2d;
    private Animator _animator;
    private AudioSource _audioSource;
    private CapsuleCollider2D _capsuleCollider2D;
    private BoxCollider2D _boxCollider2D;
    #endregion

    #region Fields
    private Enemy _enemy;
    private PlayerController _player;
    private AnimatorOverrideController _animatorOverrideController;
    private short _currBloodEffectIndex;
    private short DieAnimIndex;
    private float _kickCastDistance;
    private WeaponType _lastWeaponType;
    private float _pushForce;
    private short _gruntSfxIndex;
    #endregion

    #region VFX
    [SerializeField] EnemyBulletController Bullet;
    [SerializeField] BloodyBulletController BloodyBullet;
    [SerializeField] GameObject SmokeEffect;
    [SerializeField] ParticleSystem WalkingDustEffect;
    [SerializeField] ParticleSystem DieDustEffect;
    [SerializeField] ParticleSystem[] BloodEffects;
    [SerializeField] ParticleSystem BloodFlow01;
    [SerializeField] GameObject[] BloodOnWalls;
    [SerializeField] GameObject[] ShotgunBloodOnWalls;
    [SerializeField] GameObject DropShell;
    [SerializeField] GameObject RevolverEntity;
    [SerializeField] GameObject RevolverFiredEntity;
    [SerializeField] AnimationClip[] HurtAnimations;
    [SerializeField] AnimationClip[] DieAnimations;
    #endregion

    #region SFX
    public AudioClip ShotClip;
    public AudioClip ChamberRemoveClip;
    public AudioClip ChamberLoadClip;
    [SerializeField] AudioClip[] Grunts;
    [SerializeField] AudioClip[] Coughs;
    [SerializeField] AudioClip[] ShotgunGrunts;
    [SerializeField] AudioClip[] FallingDowns;
    [SerializeField] AudioClip[] WhooshesAndSwooshes;
    [SerializeField] AudioClip[] BreakingBones;
    [SerializeField] AudioClip[] SurpriseGrasps;
    [SerializeField] AudioClip WalkingClip;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _enemy = new Enemy();

        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody2d = GetComponent<Rigidbody2D>();
        _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();
        _animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _animatorOverrideController;

        _audioSource = GetComponent<AudioSource>();
        _player = FindObjectOfType<PlayerController>();

        ExitTriggerGO.transform.parent = null;
        _enemy.AllowChangeDirection = true;
        _enemy.CanWalk = true;
        _enemy.AllowKicking = true;
        _enemy.AllowFlipping = true;
        _enemy.AllowShootingWhileMoving = true;
        _enemy.Health = Health;
        _kickCastDistance = 0.5f;
        _gruntSfxIndex = (short)Random.Range(0, Grunts.Length);
    }

    void FixedUpdate()
    {
        Movement();
    }

    // Update is called once per frame
    void Update()
    {
        if (_enemy.IsDead || !_spriteRenderer.isVisible)
            return;

        Shooting();
        SearchingPlayer();
        CanKickPlayerOrCollide();
    }

    #region SFX Methods

    public void PlayCoughSfx()
    {
        AudioClip coughSfx;

        if (_enemy.IsDead)
        {
            short randIndex = (short)Random.Range(0, 2);
            coughSfx = Coughs[randIndex];
        }
        else
            coughSfx = Coughs[2];

        _audioSource.PlayOneShot(coughSfx);
    }

    public void PlayKickWhooshSfx()
    {
        _audioSource.clip = WhooshesAndSwooshes[0];
        _audioSource.Play();
    }

    public void PlayBreakingFingersSfx()
    {
        _audioSource.PlayOneShot(BreakingBones[0]);
    }

    public void PlayArmBrokenGruntSfx()
    {
        _audioSource.PlayOneShot(Coughs[3]);
    }

    public void PlayGruntSfx()
    {
        if (_audioSource.clip == Grunts[_gruntSfxIndex] && _audioSource.isPlaying)
            return;

        _audioSource.clip = Grunts[_gruntSfxIndex];
        _audioSource.Play();
    }

    public void PlayShotgunGruntSfx()
    {
        short randIndex = (short)Random.Range(0, ShotgunGrunts.Length);
        _audioSource.PlayOneShot(ShotgunGrunts[randIndex]);
    }


    public void PlayFallingDownSfx()
    {
        if (DieAnimIndex == 2)
            _audioSource.clip = FallingDowns[1];
        else
            _audioSource.clip = FallingDowns[0];

        _audioSource.Play();
    }

    void PlayChamberLoad()
    {
        if (_audioSource.isPlaying)
            return;

        _audioSource.clip = ChamberLoadClip;
        _audioSource.Play();
        ReloadingFinished();
    }

    void PlayChamberRemove()
    {
        if (_audioSource.isPlaying)
            return;

        _audioSource.clip = ChamberRemoveClip;
        _audioSource.Play();
    }

    void PlaySurpiseGraspSfx()
    {
        // index 0 for fallen reaction
        var randNum = Random.Range(1, SurpriseGrasps.Length);
        _audioSource.PlayOneShot(SurpriseGrasps[randNum]);
    }

    void PlaySurpiseFallenGraspSfx()
    {
        _audioSource.PlayOneShot(SurpriseGrasps[0]);
    }

    void SetVolumeToFull()
    {
        _audioSource.volume = 1f;
    }
    #endregion

    #region Movement

    public void Movement()
    {
        if (_enemy.IsDead || IsOnTheHurtingState())
            return;

        if (!_spriteRenderer.isVisible || !_enemy.CanWalk || _enemy.IsShooting)
        {
            if (_audioSource.clip == WalkingClip || _audioSource.volume != 1f)
            {
                _audioSource.Stop();
                _audioSource.volume = 1f;
                _audioSource.clip = null;
            }
            if(!_enemy.IsShootingWhileWalkingTowards)
                return;
        }

        if (!_audioSource.isPlaying && !_enemy.IsShootingWhileWalkingTowards)
        {
            _audioSource.clip = WalkingClip;
            _audioSource.volume = Random.Range(0.2f, 0.275f);
            _audioSource.Play();
        }

        if (IsFacingRight())
            _rigidbody2d.velocity = new Vector2(Speed, 0f);
        else if (!IsFacingRight())
            _rigidbody2d.velocity = new Vector2(-Speed, 0f);
    }

    bool IsOnTheHurtingState()
    {
        if (_enemy.HasBulletTakenFromBack || _enemy.HasBulletTakenFromFront ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt_Front") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Hurt_Back")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Start_Injured") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Reaction01") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Reaction_Fallen01"))
            return true;
        else
            return false;
    }

    void SetCanWalk()
    {
        if (_enemy.IsDead || _enemy.HasBulletTakenFromBack || _enemy.HasBulletTakenFromFront)
            return;

        _enemy.CanWalk = true;
        if (_enemy.HasSeenPlayer || _enemy.HasSeenPlayerAndFallen)
            _enemy.HasReactionCompleted = true;
    }

    void SetAllowFlippingToFalse()
    {
        _enemy.AllowFlipping = false;
    }

    public bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }

    void TurnFaceToRight(bool toRight)
    {
        if (_enemy.IsDead)
            return;

        if(toRight)
            transform.localScale = new Vector2(1f, 1f);
        else
            transform.localScale = new Vector2(-1f, 1f);
    }

   void SetAllowChangeDirection()
    {
        _enemy.AllowChangeDirection = true;
    }

    void AddPushForce()
    {
        _pushForce = Mathf.Abs(_pushForce);

        if (IsFacingRight())
            _pushForce = -_pushForce;

        _rigidbody2d.AddForce(new Vector2(_pushForce, 0));
    }

    #endregion

    #region Shooting

    public void Shooting()
    {
        if (_enemy.IsShootingWhileWalkingTowards)
            return;

        if (CanSeePlayer(SightRange))
        {
            CancelInvoke("StopSeeingPlayer");

            // Check if reaction happened
            var reactionResult = SetReactionRandomly();

            if (!reactionResult)
                return;

            if (_enemy.AllowShootingWhileMoving && !_enemy.IsInjured)
            {
                _enemy.AllowShootingWhileMoving = false;
                SetShootingWhileWalking();
                return;
            }

            _enemy.CanWalk = false;
            _enemy.IsShooting = true;
            _enemy.IsSearching = true;
            _animator.SetBool("IsShooting", _enemy.IsShooting);
        }
        else
        {
            if(_enemy.IsSearching)
            {
                Invoke("StopSeeingPlayer", 1.5f);
            }
        }

        if (_enemy.IsShooting)
            AlertOtherEnemies();
    }

    public bool GetAllowShootingWhileMoving()
    {
        return _enemy.AllowShootingWhileMoving;
    }


    public void SetAllowShootingWhileMovingToFalse()
    {
        _enemy.AllowShootingWhileMoving = false;
    }

    public void SetShootingWhileWalking()
    {
        _enemy.IsShooting = false;
        _animator.SetBool("IsShooting", _enemy.IsShooting);
        LookAtThePlayer();
        _enemy.IsSearching = true;
        _enemy.IsShootingWhileWalkingTowards = true;
        _animator.SetBool("IsShootingWhileWalkingTowards", _enemy.IsShootingWhileWalkingTowards);
    }

    void StopShootingWhileWalking()
    {
        _enemy.IsShooting = false;
        _animator.SetBool("IsShooting", _enemy.IsShooting);
        _enemy.IsSearching = false;
        _enemy.IsShootingWhileWalkingTowards = false;
        _animator.SetBool("IsShootingWhileWalkingTowards", _enemy.IsShootingWhileWalkingTowards);
        _enemy.CanWalk = false;
        LookAtThePlayer();
    }

    void AlertOtherEnemies()
    {
        GameObject enemyGO = CanSendMessageToFriend(SightRange * 1.5f);

        if (enemyGO != null)
        {
            EnemyController enemyController = enemyGO.GetComponent<EnemyController>();

            if (enemyController?.Health > 0)
            {
                enemyController?.LookAtThePlayer();
                if(enemyController.GetAllowShootingWhileMoving())
                {
                    enemyController.SetReactionRandomly();
                    enemyController.SetAllowShootingWhileMovingToFalse();
                    enemyController.SetShootingWhileWalking();
                }
            }
        }
    }

    void InstatiateShells()
    {
        Vector2 shellPos;
        if (IsFacingRight())
            shellPos = new Vector2(transform.position.x + 0.23f, transform.position.y + 0.0334f);
        else
            shellPos = new Vector2(transform.position.x - 0.23f, transform.position.y + 0.0334f);

        Instantiate(DropShell, shellPos, Quaternion.identity);
    }

    public void ReloadingFinished()
    {
        if (_enemy.IsDead)
            return;

        _enemy.AmountOfFiredBullets = 0;
        _enemy.IsReloading = false;
        _animator.SetBool("IsReloading", _enemy.IsReloading);
    }

    void InstantiateBullet()
    {
        // Play shot sfx
        _audioSource.PlayOneShot(ShotClip, 1f);

        //Instantiate revolver smoke effect
        Vector2 bulletSmokeEffectPos;
        if (IsFacingRight())
            bulletSmokeEffectPos = new Vector2(transform.position.x + 0.294f, transform.position.y + 0.063f);
        else
            bulletSmokeEffectPos = new Vector2(transform.position.x - 0.294f, transform.position.y + 0.063f);

        Instantiate(SmokeEffect, bulletSmokeEffectPos, Quaternion.Euler(-90, 0, 0));

        Vector2 bulletPos;
        float additionYPos;

        if (!_enemy.IsInjured)
            additionYPos = 0.108f;
        else
            additionYPos = 0.064f;


        if (IsFacingRight())
            bulletPos = new Vector2(transform.position.x + 0.34f, transform.position.y + additionYPos);
        else
            bulletPos = new Vector2(transform.position.x - 0.34f, transform.position.y + additionYPos);

        EnemyBulletController bullet = Instantiate(Bullet, bulletPos, Quaternion.identity);

        if (bullet != null)
            bullet.ScaleXValue = IsFacingRight() ? 1 : -1;

        _enemy.AmountOfFiredBullets++;

        if (_enemy.AmountOfFiredBullets >= 2)
        {
            _enemy.IsReloading = true;
            _animator.SetBool("IsReloading", _enemy.IsReloading);
        }
    }

    #endregion

    #region Actions

    public void LookAtThePlayer()
    {
        if (!_enemy.AllowFlipping)
            return;

        SetAllowChangeDirection();

        if (transform.position.x < _player.transform.position.x)
            TurnFaceToRight(true);
        else
            TurnFaceToRight(false);
    }

    void SearchingPlayer()
    {
        if (!_enemy.IsSearching || _enemy.IsDead)
            return;

        LookAtThePlayer();
    }

    void StopSeeingPlayer()
    {
        if (_enemy.IsDead)
            return;

        _enemy.AllowShootingWhileMoving = true;
        _enemy.IsSearching = false;
        _enemy.IsShooting = false;
        _animator.SetBool("IsShooting", _enemy.IsShooting);
    }

    GameObject CanSendMessageToFriend(float distance)
    {
        GameObject result = null;

        if(!IsFacingRight())
        {
            Vector3 castStartPosRight = new Vector2(transform.position.x + 0.25f, transform.position.y + 0.15f);
            Vector2 endPosRight = castStartPosRight + Vector3.right * distance;
            RaycastHit2D hitRight = Physics2D.Linecast(castStartPosRight, endPosRight, 1 << LayerMask.NameToLayer("Enemies") | 1 << LayerMask.NameToLayer("Ground"));

            if (hitRight.collider != null)
            {
                if (hitRight.collider.gameObject.CompareTag("Enemy"))
                    result = hitRight.collider.gameObject;
                else
                    result = null;

                Debug.DrawLine(castStartPosRight, hitRight.point, Color.yellow);
            }
            else
            {
                Debug.DrawLine(castStartPosRight, endPosRight, Color.magenta);
            }
        } else
        {
            Vector3 castStartPosLeft = new Vector2(transform.position.x - 0.25f, transform.position.y + 0.15f);
            Vector2 endPosLeft = castStartPosLeft + Vector3.left * (distance);
            RaycastHit2D hitLeft = Physics2D.Linecast(castStartPosLeft, endPosLeft, 1 << LayerMask.NameToLayer("Enemies") | 1 << LayerMask.NameToLayer("Ground"));

            if (hitLeft.collider != null && !result)
            {
                if (hitLeft.collider.gameObject.CompareTag("Enemy"))
                    result = hitLeft.collider.gameObject;
                else
                    result = null;

                Debug.DrawLine(castStartPosLeft, hitLeft.point, Color.yellow);
            }
            else
            {
                Debug.DrawLine(castStartPosLeft, endPosLeft, Color.magenta);
            }
        }

        return result;
    }

    bool CanSeePlayer(float distance)
    {
        bool result = false;

        Vector3 castStartPosRight = new Vector2(transform.position.x + 0.25f, transform.position.y + -0.3f);
        Vector2 endPosRight = castStartPosRight + Vector3.right * distance;
        RaycastHit2D hitRight = Physics2D.Linecast(castStartPosRight, endPosRight, WhatToSee);

        Vector3 castStartPosLeft = new Vector2(transform.position.x - 0.25f, transform.position.y + -0.3f);
        Vector2 endPosLeft = castStartPosLeft + Vector3.right * -(distance);
        RaycastHit2D hitLeft = Physics2D.Linecast(castStartPosLeft, endPosLeft, WhatToSee);

        if (hitRight.collider != null)
        {
            if (hitRight.collider.gameObject.CompareTag("Player"))
                result = true;
            else
                result = false;

            Debug.DrawLine(castStartPosRight, hitRight.point, Color.green);
        }
        else
        {
            Debug.DrawLine(castStartPosRight, endPosRight, Color.red);
        }

        if (hitLeft.collider != null && !result)
        {
            if (hitLeft.collider.gameObject.CompareTag("Player"))
            {
                result = true;
                LookAtThePlayer();
            }
            else
                result = false;

            Debug.DrawLine(castStartPosLeft, hitLeft.point, Color.cyan);
        }
        else
        {
            Debug.DrawLine(castStartPosLeft, endPosLeft, Color.black);
        }

        return result;
    }

    bool CanKickPlayerOrCollide()
    {
        if (_enemy.HasArmBroken || _enemy.HasFallen)
            return false;

        bool result = false;
        Vector3 curPos = new Vector3(transform.position.x, transform.position.y - 0.2f);
        Vector2 endPos = curPos + (IsFacingRight() ? Vector3.right : Vector3.left) * _kickCastDistance;
        RaycastHit2D hit = Physics2D.Linecast(curPos, endPos, WhatToKickAndCollide);

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Player") && !_player.IsPlayerRolling())
                result = true;
            else
                result = false;

            // When enemy collides with the door, walls or stairs make him turn back
            if (hit.collider.gameObject.CompareTag("Door Collider") || hit.collider.gameObject.CompareTag("Foreground"))
            {
                if (_enemy.IsShootingWhileWalkingTowards)
                    StopShootingWhileWalking();

                _enemy.AllowChangeDirection = false;
                Invoke("SetAllowChangeDirection", 3f);
                if (IsFacingRight())
                    TurnFaceToRight(false);
                else
                    TurnFaceToRight(true);
            }

            Debug.DrawLine(curPos, endPos, Color.yellow);
        }
        else
        {
            Debug.DrawLine(curPos, endPos, Color.blue);
        }

        if (result && _enemy.AllowKicking)
        {
            _enemy.AllowKicking = false;
            float delayVal = 0;

            if (_enemy.IsInjured)
                delayVal = 0.5f;

            Invoke("StartKickingToPlayer", delayVal);
        }

        return result;
    }

    void StartKickingToPlayer()
    {
        if (_enemy.HasArmBroken)
            return;

        Invoke("SetAllowKicking", 2f);
        _enemy.CanWalk = false;
        _enemy.IsKicking = true;
        _animator.SetBool("IsKicking", _enemy.IsKicking);
    }

    void SetAllowKicking()
    {
        _enemy.AllowKicking = true;
    }

    public void EnemyKickedPlayer()
    {
        bool isDirLeft = false;

        if (!IsFacingRight())
            isDirLeft = true;

        _player.PlayerHasBeenKickedByEnemy(isDirLeft);

        _enemy.IsKicking = false;
        _animator.SetBool("IsKicking", _enemy.IsKicking);
        _enemy.CanWalk = true;
    }

    public bool SetReactionRandomly()
    {
        if (_enemy.HasSeenPlayer || _enemy.HasSeenPlayerAndFallen)
        {
            if(_enemy.HasReactionCompleted)
                return true;
            else
                return false;
        }

        SetVolumeToFull();
        _enemy.CanWalk = false;
        short randNum = 3;

        // If player has kicked the door then enemy shall be fallen
        if (!_player.IsOnKickingState())
            randNum = (short)Random.Range(0, 4);

        if(NoFall)
            randNum = (short)Random.Range(0, 3);

        if (randNum != 3) // 3 means falling while reacting
        {
            _enemy.HasSeenPlayer = true;
            _enemy.HasSeenPlayerAndFallen = true;
            _animator.SetBool("HasSeenPlayer", _enemy.HasSeenPlayer);
        } else
        {
            LookAtThePlayer();
            _enemy.HasSeenPlayer = true;
            _enemy.HasSeenPlayerAndFallen = true;
            _animator.SetBool("HasSeenPlayerAndFallen", _enemy.HasSeenPlayerAndFallen);
        }

        return false;
    }

    void ReactionHappened()
    {
        _animator.SetBool("HasSeenPlayer", false);
        _animator.SetBool("HasSeenPlayerAndFallen", false);
        _enemy.HasReactionCompleted = true;
    }

    void HasFallen()
    {
        _enemy.HasFallen = true;
        _animator.SetBool("HasFallen", _enemy.HasFallen);
        _enemy.AllowFlipping = false;
    }

    public void GetUp()
    {
        SetBulletTakenFrontToFalse();
        SetBulletTakenBackToFalse();
        _enemy.HasFallen = false;
        _animator.SetBool("HasFallen", _enemy.HasFallen);
        _enemy.AllowFlipping = true;
    }

    public void GetUpInjured()
    {
        SetShotgunBulletTakenToFalse();
        _enemy.HasFallen = false;
        _animator.SetBool("HasFallen", _enemy.HasFallen);
        _enemy.AllowFlipping = true;
    }
    #endregion

    #region Dust Effects

    void InstantiateWalkingDustEffect()
    {
        float extraXPos = 0f;

        if (IsFacingRight())
            extraXPos = 0.2f;
        else
            extraXPos = -0.2f;

        Vector2 dustPos = new Vector2(transform.position.x + extraXPos, transform.position.y - 0.3f);
        Instantiate(WalkingDustEffect, dustPos, Quaternion.Euler(90, 0, 0));
    }

    void InstantiateDieDustEffect()
    {
        float extraXPos = 0f;

        if (IsFacingRight())
            extraXPos = 0.2f;
        else
            extraXPos = -0.2f;

        Vector2 dustPos = new Vector2(transform.position.x + extraXPos, transform.position.y - 0.2f);
        Instantiate(DieDustEffect, dustPos, Quaternion.Euler(90, 0, 0));
    }

    void InstantiateDieDustEffectForHisBack()
    {
        float extraXPos = 0f;

        if (IsFacingRight())
            extraXPos = -0.2f;
        else
            extraXPos = 0.2f;

        Vector2 dustPos = new Vector2(transform.position.x + extraXPos, transform.position.y - 0.2f);
        Instantiate(DieDustEffect, dustPos, Quaternion.Euler(90, 0, 0));
    }

    #endregion

    #region Damage

    void SetBulletTakenFrontToFalse()
    {
        _enemy.HasBulletTakenFromFront = false;
        _animator.SetBool("HasBulletTakenFromFront", _enemy.HasBulletTakenFromFront);
    }

    void SetBulletTakenBackToFalse()
    {
        _enemy.HasBulletTakenFromBack = false;
        _animator.SetBool("HasBulletTakenFromBack", _enemy.HasBulletTakenFromBack);
    }

    void SetShotgunBulletTakenToFalse()
    {
        _enemy.HasShotgunBulletTaken = false;
        _animator.SetBool("HasShotgunBulletTaken", _enemy.HasShotgunBulletTaken);
        _enemy.AllowFlipping = false;
    }

    object[] GetBloodEffectInOrder(WeaponType weaponType)
    {
        short bloodEffectIndex = 0;

        if (weaponType == WeaponType.Revolver || weaponType == WeaponType.Pistol1911)
            bloodEffectIndex = _currBloodEffectIndex;
        else if (weaponType == WeaponType.Shotgun)
            bloodEffectIndex = 4;
        else if(weaponType == WeaponType.Submachine)
            bloodEffectIndex = 5;

        ParticleSystem selectedBloodEffect;
        Vector2 bloodPos;

        switch (bloodEffectIndex)
        {
            case 0:
                selectedBloodEffect = BloodEffects[0];
                bloodPos = new Vector2(0.116f, 0.094f);
                _animatorOverrideController[HurtAnimations[0].name] = HurtAnimations[0];
                break;
            case 1:
                selectedBloodEffect = BloodEffects[1];
                bloodPos = new Vector2(0.384f, 0.034f);
                _animatorOverrideController[HurtAnimations[0].name] = HurtAnimations[1];
                break;
            case 2:
                selectedBloodEffect = BloodEffects[2];
                bloodPos = new Vector2(0.401f, 0.036f);
                _animatorOverrideController[HurtAnimations[0].name] = HurtAnimations[2];
                break;
            case 3:
                selectedBloodEffect = BloodEffects[0];
                bloodPos = new Vector2(0.457f, 0.023f);
                _animatorOverrideController[HurtAnimations[0].name] = HurtAnimations[1];
                break;
            case 4: // Shotgun
                short shotgunRandIndex = (short) Random.Range(4, 6);
                selectedBloodEffect = BloodEffects[shotgunRandIndex];
                bloodPos = new Vector2(0.249f, 0.05f);
                break;
            case 5: // Submachine
                short subMacRandIndex = (short)Random.Range(6, 10);
                selectedBloodEffect = BloodEffects[subMacRandIndex];
                bloodPos = new Vector2(0.5f, 0.039f);
                break;
            default:
                selectedBloodEffect = new ParticleSystem();
                bloodPos = new Vector2();
                break;
        }

        _currBloodEffectIndex++;

        if (_currBloodEffectIndex == 4)
            _currBloodEffectIndex = 0;

        return new object[] { selectedBloodEffect, bloodPos };
    }

    void InstantiateBloodFlowEffect(float bulletScaleX, bool isDead = false)
    {
        // Instantiate bloody bullet which is coming out of the body.
        Vector2 bloodyBulletPos;
        Vector3 bloodyBulletScale;

        if (bulletScaleX < 0f)
        {
            bloodyBulletPos = new Vector2(transform.position.x + 0.050f, transform.position.y + 0.150f);
            bloodyBulletScale = new Vector3(0.4f, 0.4f, 1f);
        }
        else
        {
            bloodyBulletPos = new Vector2(transform.position.x - 0.050f, transform.position.y + 0.150f);
            bloodyBulletScale = new Vector3(-0.4f, 0.4f, 1f);
        }

        BloodyBulletController bloodyBulletGo = Instantiate(BloodyBullet, bloodyBulletPos, Quaternion.identity) as BloodyBulletController;
        bloodyBulletGo.gameObject.transform.localScale = bloodyBulletScale;

        // If enemy is dead then DO NOT instantiate back blood flow and front blood effect
        if (_enemy.IsDead)
            return;

        // Instantiate blood flow
        Vector2 bloodFlowPos;
        Vector3 bloodFlowEffectScale;

        if (IsFacingRight())
        {
            bloodFlowPos = new Vector2(transform.position.x + (-0.385f), transform.position.y + 0.053f);
            bloodFlowEffectScale = new Vector3(0.25f, 0.25f, 1f);
        }
        else
        {
            bloodFlowPos = new Vector2(transform.position.x - (-0.385f), transform.position.y + 0.053f);
            bloodFlowEffectScale = new Vector3(-0.25f, 0.25f, 1f);
        }

        ParticleSystem bloodFlowEffectGo = Instantiate(BloodFlow01, bloodFlowPos, Quaternion.identity) as ParticleSystem;
        bloodFlowEffectGo.gameObject.transform.localScale = bloodFlowEffectScale;

        // Instantiate front blood effect
        Vector2 bloodPos = new Vector2(0.457f, 0.023f);
        Vector3 bloodEffectScale;
        if (IsFacingRight())
        {
            bloodPos = new Vector2(transform.position.x + bloodPos.x, transform.position.y + bloodPos.y);
            bloodEffectScale = new Vector3(0.25f, 0.25f, 1f);
        }
        else
        {
            bloodPos = new Vector2(transform.position.x - bloodPos.x, transform.position.y + bloodPos.y);
            bloodEffectScale = new Vector3(-0.25f, 0.25f, 1f);
        }

        ParticleSystem bloodEffectGo = Instantiate(BloodEffects[3], bloodPos, Quaternion.identity) as ParticleSystem;
        bloodEffectGo.gameObject.transform.localScale = bloodEffectScale;
    }

    public bool HasEnemyInjured()
    {
        return _enemy.IsInjured;
    }

    public void ArmHasBroken()
    {
        if (!_enemy.IsInjured)
            return;

        _enemy.HasArmBroken = true;
        _animator.SetBool("HasArmBroken", _enemy.HasArmBroken);
        _enemy.IsDead = true;
    }

    void InstantiateBloodEffect(object[] bloodEffectVals)
    {
        ParticleSystem selectedBloodEffect = (ParticleSystem)bloodEffectVals[0];
        Vector2 selectedBloodPos = (Vector2)bloodEffectVals[1];

        Vector2 bloodPos;
        Vector3 bloodEffectScale;
        if (IsFacingRight())
        {
            bloodPos = new Vector2(transform.position.x + selectedBloodPos.x, transform.position.y + selectedBloodPos.y);
            bloodEffectScale = new Vector3(0.25f, 0.25f, 1f);
        }
        else
        {
            bloodPos = new Vector2(transform.position.x - selectedBloodPos.x, transform.position.y + selectedBloodPos.y);
            bloodEffectScale = new Vector3(-0.25f, 0.25f, 1f);
        }

        ParticleSystem bloodEffectGo = Instantiate(selectedBloodEffect, bloodPos, Quaternion.identity) as ParticleSystem;
        bloodEffectGo.gameObject.transform.localScale = bloodEffectScale;
    }

    public void InstantiateBloodOnWall(WeaponType weaponType)
    {
        Vector2 bloodOnWallPos;

        float extraXYPos = 0f;

        if (weaponType == WeaponType.Submachine)
            extraXYPos = 0.125f;

        float randXPos = Random.Range(-0.35f - extraXYPos, 0.35f + extraXYPos);
        float randYPos = Random.Range(-0.25f, 0.25f + extraXYPos);
        float randRotationZVal = Random.Range(0, -40f);

        bloodOnWallPos = new Vector2(transform.position.x + randXPos, transform.position.y + randYPos);
        short bloodOnWallIndex = 0;

        switch (weaponType)
        {
            case WeaponType.Revolver:
            case WeaponType.Pistol1911:
                bloodOnWallIndex = (short)Random.Range(0, BloodOnWalls.Length);
                Instantiate(BloodOnWalls[bloodOnWallIndex], bloodOnWallPos, Quaternion.Euler(0, 0, randRotationZVal));
                break;
            case WeaponType.Shotgun:
                bloodOnWallIndex = (short)Random.Range(0, ShotgunBloodOnWalls.Length);
                var wallBloodGo = Instantiate(ShotgunBloodOnWalls[bloodOnWallIndex], bloodOnWallPos, Quaternion.Euler(0, 0, randRotationZVal));
                float randScale = Random.Range(0.9f, 1f);
                wallBloodGo.transform.localScale = new Vector3(randScale, randScale);
                break;
            case WeaponType.Submachine:
                bloodOnWallIndex = (short)Random.Range(0, BloodOnWalls.Length);
                Instantiate(BloodOnWalls[bloodOnWallIndex], bloodOnWallPos, Quaternion.Euler(0, 0, randRotationZVal));
                break;
            default:
                break;
        }
    }

    public void GetDamage(float bulletScaleX, WeaponType weaponType, short amountOfDamageToReduce, float pushForce = 0, bool canInstantiateBloodEffect = true)
    {
        if (_enemy.IsDead)
        {
            InstantiateBloodFlowEffect(bulletScaleX, true);

            if (weaponType == WeaponType.Submachine)
            {
                InstantiateBloodOnWall(weaponType);
                object[] bloodEffectVals = GetBloodEffectInOrder(weaponType);
                InstantiateBloodEffect(bloodEffectVals);
            }

            return;
        }

        _lastWeaponType = weaponType;
        _pushForce = pushForce;

        // Accept as reaction already happened if it hasn't been yet
        if (!_enemy.HasSeenPlayer || !_enemy.HasSeenPlayerAndFallen)
        {
            _enemy.HasSeenPlayer = true;
            _enemy.HasReactionCompleted = true;
        }

        _enemy.CanWalk = false;

        // Damage
        switch (weaponType)
        {
            case WeaponType.Revolver:
                _enemy.Health -= (short) (RevolverDamage - amountOfDamageToReduce);
                break;
            case WeaponType.Pistol1911:
                _enemy.Health -= (short) (Pistol1911Damage - amountOfDamageToReduce);
                break;
            case WeaponType.Shotgun:
                _enemy.Health -= (short) (ShotgunDamage - amountOfDamageToReduce);
                break;
            case WeaponType.Submachine:
                _enemy.Health -= (short) (SubmachineDamage - amountOfDamageToReduce);
                break;
            default:
                break;
        }

        Health = _enemy.Health;

        // Instantiate bloods for walls
        if (canInstantiateBloodEffect)
            InstantiateBloodOnWall(weaponType);

        if (_enemy.Health <= 50 && !_enemy.IsInjured && weaponType != WeaponType.Shotgun)
        {
            _enemy.HasInjuryStarted = true;
            _animator.SetBool("HasInjuryStarted", _enemy.HasInjuryStarted);
            _enemy.IsKicking = false;
            _animator.SetBool("IsKicking", _enemy.IsKicking);
            _kickCastDistance = 0.25f;
        }

        if (_enemy.Health <= 0)
        {
            // If enemy is injured then instantiate blood flow effect.
            InstantiateBloodFlowEffect(bulletScaleX);
            if(weaponType == WeaponType.Shotgun)
            {
                SetShotgunHurt();
                return;
            }

            Invoke("SetIsDead", 0.2f);
            PlayGruntSfx();
            return;
        }

        // Instantiate blood effects
        if (canInstantiateBloodEffect)
        {
            object[] bloodEffectVals = GetBloodEffectInOrder(weaponType);
            InstantiateBloodEffect(bloodEffectVals);
        }

        // Play grunt sfx
        PlayGruntSfx();

        if (_enemy.IsInjured || _enemy.HasInjuryStarted)
        {
            LookAtThePlayer();
            return;
        }

        switch (weaponType)
        {
            case WeaponType.Revolver:
            case WeaponType.Pistol1911:
                SetHandgunHurt(bulletScaleX);
                break;
            case WeaponType.Shotgun:
                SetShotgunHurt();
                break;
            case WeaponType.Submachine:
                SetHandgunHurt(bulletScaleX);
                break;
            default:
                break;
        }
    }

    void SetHandgunHurt(float bulletScaleX)
    {
        _rigidbody2d.velocity = new Vector2(0, 0);

        if (bulletScaleX == 1f)
        {
            if (IsFacingRight())
            {
                _enemy.HasBulletTakenFromBack = true;
                _animator.SetBool("HasBulletTakenFromBack", _enemy.HasBulletTakenFromBack);
            }
            else
            {
                _enemy.HasBulletTakenFromFront = true;
                _animator.SetBool("HasBulletTakenFromFront", _enemy.HasBulletTakenFromFront);
            }
        }
        else
        {
            if (IsFacingRight())
            {
                _enemy.HasBulletTakenFromFront = true;
                _animator.SetBool("HasBulletTakenFromFront", _enemy.HasBulletTakenFromFront);
            }
            else
            {
                _enemy.HasBulletTakenFromBack = true;
                _animator.SetBool("HasBulletTakenFromBack", _enemy.HasBulletTakenFromBack);
            }
        }
    }

    void SetShotgunHurt()
    {
        LookAtThePlayer();
        _enemy.HasShotgunBulletTaken = true;
        _animator.SetBool("HasShotgunBulletTaken", _enemy.HasShotgunBulletTaken);
        HasFallen();
        SetIsInjured();
    }

    void CheckIfHeCanLive()
    {
        if (_enemy.Health <= 0)
        {
            SetIsDead();
        }
    }

    void SetIsInjured()
    {
        _enemy.HasInjuryStarted = false;
        _animator.SetBool("HasInjuryStarted", _enemy.HasInjuryStarted);
        _enemy.IsInjured = true;
        _animator.SetBool("IsInjured", _enemy.IsInjured);
        _enemy.IsKicking = false;
        _animator.SetBool("IsKicking", _enemy.IsKicking);

        if (_enemy.IsShootingWhileWalkingTowards)
            StopShootingWhileWalking();

        LookAtThePlayer();
    }

    public void SetIsDead()
    {
        if(_lastWeaponType != WeaponType.Shotgun)
            DieAnimIndex = (short) Random.Range(0, 3);
        else
            DieAnimIndex = 3;

        if(OnlyDie03)
            DieAnimIndex = 2;

        _animatorOverrideController[DieAnimations[0].name] = DieAnimations[DieAnimIndex];

        _enemy.IsDead = true;
        _enemy.IsInjured = false;
        _enemy.HasBulletTakenFromFront = false;
        _enemy.HasBulletTakenFromBack = false;
        _animator.SetBool("IsDead", _enemy.IsDead);
        _animator.SetBool("IsInjured", _enemy.IsInjured);
        _animator.SetBool("HasBulletTakenFromFront", _enemy.HasBulletTakenFromFront);
        _animator.SetBool("HasBulletTakenFromBack", _enemy.HasBulletTakenFromBack);
    }

    public bool IsEnemyDead()
    {
        return _enemy.IsDead;
    }

    public void DropWeapon()
    {
        Vector2 revolverPos;
        if (IsFacingRight())
        {
            revolverPos = new Vector2(transform.position.x + 0.17f, transform.position.y + -0.067f);
        }
        else
        {
            revolverPos = new Vector2(transform.position.x - 0.17f, transform.position.y + -0.067f);
        }

        Instantiate(RevolverEntity, revolverPos, Quaternion.identity);
    }

    public void DropFiredWeapon()
    {
        Vector2 revolverPos;
        if (IsFacingRight())
        {
            revolverPos = new Vector2(transform.position.x + 0.17f, transform.position.y + 0.1f);
        }
        else
        {
            revolverPos = new Vector2(transform.position.x - 0.17f, transform.position.y + 0.1f);
        }

        Instantiate(RevolverFiredEntity, revolverPos, Quaternion.identity);
    }

    public void DestroyComponents()
    {
        Destroy(_rigidbody2d);
        Destroy(_capsuleCollider2D);
        Destroy(ExitTriggerGO.gameObject);
        Destroy(_boxCollider2D, 2f);
        Destroy(_audioSource, 2f);
        GameObject breakableColliderGo = transform.GetChild(0).gameObject;
        GameObject doorColliderGo = transform.GetChild(1).gameObject;
        if (breakableColliderGo != null)
            Destroy(breakableColliderGo);
        if (doorColliderGo != null)
            Destroy(doorColliderGo);
        Destroy(this, 2f);
    }

    public void DestroyAnimator()
    {
        Destroy(_animator);
        Destroy(this, 1f);
    }

    #endregion

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!_enemy.AllowChangeDirection || !_enemy.AllowFlipping)
            return;

        if(collision.gameObject.tag.Equals("Enemy Exit Trigger") || collision.gameObject.tag.Equals("Door Collider"))
        {
            if (_enemy.IsShootingWhileWalkingTowards)
                StopShootingWhileWalking();

            _enemy.AllowChangeDirection = false;
            Invoke("SetAllowChangeDirection", 3f);
            if (IsFacingRight())
                TurnFaceToRight(false);
            else
                TurnFaceToRight(true);
        }
    }
}
