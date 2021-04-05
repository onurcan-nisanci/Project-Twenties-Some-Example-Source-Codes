using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IBaseHuman
{
    #region Fields
    private Rigidbody2D _rigidbody2d;
    private Animator _animator;
    private CapsuleCollider2D _capsuleCollider;
    private AudioSource _audioSource;
    private AnimatorOverrideController _animatorOverrideController;

    private GameManager _gameManager;
    private Player _player;
    private float _playerHorizontalInput;
    private DoorBehaviour _currCollidedDoor;
    private GameObject _currCollidedCoverableObj;
    private PlayerFootstepBehaviour _footStepController;
    private PlayerWeaponController _playerWeaponController;
    private GameObject _breakablesCollider;
    [SerializeField] public bool IsPlayerWalking;
    #endregion

    #region SFX
    [SerializeField] AudioClip[] Grunts;
    [SerializeField] AudioClip[] FallingDowns;
    [SerializeField] AudioClip[] WhooshesAndSwooshes;
    [SerializeField] AudioClip[] UmbrellaSoundEffects;
    [SerializeField] AudioClip GroundedClip;
    [SerializeField] AudioClip FlippingClip;
    [SerializeField] AudioClip PutHatBackClip;
    #endregion

    #region VFX
    [SerializeField] ParticleSystem BloodEffect01;
    [SerializeField] GameObject[] BloodOnWalls;
    #endregion

    #region Cover Objects
    [SerializeField] GameObject RotableTable;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager.Instance;
        _player = _gameManager.Player;
        GameManager.Instance.PlayerGO = this;

        _player.SlipperyForce = 20f;
        _player.AllowHurting = true;
        _player.AllowBreakingArm = true;
        _player.AllowRolling = true;

        _rigidbody2d = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
        _animator.runtimeAnimatorController = _animatorOverrideController;
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _audioSource = GetComponent<AudioSource>();
        _footStepController = transform.GetChild(2).gameObject.GetComponent<PlayerFootstepBehaviour>();
        _playerWeaponController = GetComponent<PlayerWeaponController>();
        _playerWeaponController.Player = _player;
        _playerWeaponController.PlayerController = this;
        _breakablesCollider = transform.GetChild(3).gameObject;

        _animator.SetBool("IsWalking", IsPlayerWalking);
    }

    void FixedUpdate()
    {
        Movement();
    }

    // Update is called once per frame
    void Update()
    {
        if (_player.IsDead || _player.HasBeenKicked || _player.IsRolling)
            return;

        FlipPlayer();
        Roll();
        UmbrellaCover();
        LeaveCover();
        CanBreakArm();
    }

    #region Dust Effects

    void InstantiateRunningDustEffect()
    {
        float extraPosX = 0;
        if (IsFacingRight())
            extraPosX += 0.25f;
        else
            extraPosX -= 0.25f;

        _footStepController.InstantiateEffect(_footStepController.RunningDustEffect, extraPosX);
    }

    void InstantiateStopRunningDustEffect()
    {
        float extraPosX = 0;
        if (IsFacingRight())
            extraPosX += 0.55f;
        else
            extraPosX -= 0.55f;

        _footStepController.InstantiateEffect(_footStepController.StopRunningDustEffect, extraPosX);
    }

    void InstantiateRollingDustEffect()
    {
        float extraPosX = 0;
        if (IsFacingRight())
            extraPosX += 0.55f;
        else
            extraPosX -= 0.55f;

        _footStepController.InstantiateEffect(_footStepController.RollingDustEffect, extraPosX);
    }

    void InstantiateKickedOutDustEffect()
    {
        float extraPosX = 0;
        if (IsFacingRight())
            extraPosX -= 0.7f;
        else
            extraPosX += 0.7f;

        _footStepController.InstantiateEffect(_footStepController.StopRunningDustEffect, extraPosX);
    }

    void InstantiateTakingCoverDustEffect()
    {
        float extraPosX = 0;
        if (IsFacingRight())
            extraPosX += 0.25f;
        else
            extraPosX -= 0.25f;

        _footStepController.InstantiateEffect(_footStepController.StopRunningDustEffect, extraPosX);
    }
    #endregion

    #region Movement
    public void Movement()
    {
        if (_player.IsDead || _animator.GetCurrentAnimatorStateInfo(0).IsName("Grounded") || _player.IsFlipped || _player.IsCovering || _player.HasBeenKicked
            || _player.IsBreakingArm || _player.IsRolling)
            return;

        _playerHorizontalInput = Input.GetAxisRaw("Horizontal");

        if (((_playerHorizontalInput == 1f && IsFacingRight()) || (_playerHorizontalInput == -1f && !IsFacingRight()))
            && _player.AllowMovement)
        {
            _rigidbody2d.velocity = new Vector2(_playerHorizontalInput * _player.Speed, _rigidbody2d.velocity.y);
        }

        // Check if player is standing or umbrella cover is active
        if (_playerHorizontalInput == 0 || _player.IsUmbrellaOpened)
            _player.AllowMovement = false;

        _player.IsMoving = (Mathf.Abs(_rigidbody2d.velocity.x)) >= 1f;

        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Shooting_Running") 
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Reloading_Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Start_Weapon_Change_Running")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Finish_Weapon_Change_Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Shotgun_Pumping_Running"))
        {
            _player.IsItReachedPeakSpeed = true;
            // Slowly increase speed of player
            if (_player.Speed < 2f)
                _player.Speed += 0.5f * Time.fixedDeltaTime;
        }
        else
        {
            _player.IsItReachedPeakSpeed = false;

            if (IsPlayerWalking)
                _player.Speed = 0.6f;
            else
                _player.Speed = 1f;
        }

        _animator.SetBool("IsRunning", _player.IsMoving);
        _animator.SetBool("IsItReachedPeakSpeed", _player.IsItReachedPeakSpeed);

        _animator.SetFloat("Speed", Mathf.Abs(_playerHorizontalInput));
    }

    bool IsPlayerOnRunning()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Shooting_Running")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Reloading_Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Running_Hurt")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Start_Weapon_Change_Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Finish_Weapon_Change_Running")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Shotgun_Pumping_Running"))
            return true;
        else
            return false;
    }

    bool IsPlayerOnStartRunning()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Start_Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Shooting_Start_Running")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Reloading_StartRunning"))
            return true;
        else
            return false;
    }

    public PlayerMovementState GetPlayerMovementState()
    {
        if(_player.IsCovering || _player.IsRolling || _player.IsKicking || _player.IsBreakingArm || _player.IsUmbrellaOpened)
            return PlayerMovementState.NotRelated;

        if(IsPlayerOnRunning())
        {
            return PlayerMovementState.Running;
        } else if(IsPlayerOnStartRunning())
        {
            return PlayerMovementState.RunningStarted;
        } else if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Stop_Running"))
        {
            return PlayerMovementState.Stopping;
        }

        return PlayerMovementState.NotRelated;
    }

    void ResetMovementSpeed()
    {
        _player.Speed = 0;
        _playerHorizontalInput = 0;
        _animator.SetFloat("Speed", _player.Speed);
        _player.AllowMovement = false;
    }

    public bool IsPlayerRolling()
    {
        return _player.IsRolling;
    }

    void Roll()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (!_player.AllowRolling || _player.HasBeenKicked || _player.IsOnTheGround || 
                _animator.GetCurrentAnimatorStateInfo(0).IsName("Rolling") || _player.IsCovering || _player.IsUmbrellaOpened)
                return;

            _playerWeaponController.CancelShooting();
            SetUmbrellaCoverDeactive();
            _player.IsRolling = true;
            _animator.SetBool("IsRolling", _player.IsRolling);
        }
    }

    void AddRollingForce()
    {
        _player.Speed = 0;
        _playerHorizontalInput = 0;
        _animator.SetFloat("Speed", _player.Speed);
        _player.AllowMovement = false;
        float forceVal = 40;
        float rbVelocityXVal = 1.6f;

        if (!IsFacingRight())
        {
            forceVal = -forceVal;
            rbVelocityXVal = -rbVelocityXVal;
        }

        _rigidbody2d.velocity = new Vector2(rbVelocityXVal, 0);
        _rigidbody2d.AddForce(new Vector2(forceVal, 0));
    }

    void RollingFinished()
    {
        _player.IsRolling = false;
        _animator.SetBool("IsRolling", _player.IsRolling);
        _player.HasBulletTaken = false;
        _animator.SetBool("HasBulletTaken", _player.HasBulletTaken);
    }

    void FlipPlayer()
    {
        // If any of these animations are playing or a state is active then return.
        if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Running") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Shooting_Running")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Reloading_Running") || _player.IsCovering || _player.IsKicking || _player.HasBeenKicked ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Leave_Cover") || _animator.GetCurrentAnimatorStateInfo(0).IsName("StopShooting_OnCover")
            || _animator.GetCurrentAnimatorStateInfo(0).IsName("Getting_Up") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Rolling"))
        {
            return;
        }

        if (_playerHorizontalInput == 1f && !IsFacingRight())
        {
            if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Start_Shooting") || 
                _animator.GetCurrentAnimatorStateInfo(0).IsName("Continuous_Shooting") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Umbrella_Opened"))
            {
                _player.HasBulletTaken = false;
                _animator.SetBool("HasBulletTaken", _player.HasBulletTaken);

                CancelInvoke("SetFlippedToFalse");
                _player.IsFlipped = true;
                _animator.SetBool("IsFlipped", _player.IsFlipped);
                Invoke("SetFlippedToFalse", 1f);
                _player.Speed = 0;
                _playerHorizontalInput = 0;
                _animator.SetFloat("Speed", _player.Speed);
                return;
            }

            transform.localScale = new Vector2(1f, 1f); // Right direction
        }

        if (_playerHorizontalInput == -1f && IsFacingRight())
        {
            if(_animator.GetCurrentAnimatorStateInfo(0).IsName("Start_Shooting") 
                || _animator.GetCurrentAnimatorStateInfo(0).IsName("Continuous_Shooting") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Umbrella_Opened"))
            {
                _player.HasBulletTaken = false;
                _animator.SetBool("HasBulletTaken", _player.HasBulletTaken);

                CancelInvoke("SetFlippedToFalse");
                _player.IsFlipped = true;
                _animator.SetBool("IsFlipped", _player.IsFlipped);
                Invoke("SetFlippedToFalse", 1f);
                _player.Speed = 0;
                _playerHorizontalInput = 0;
                _animator.SetFloat("Speed", _player.Speed);
                return;
            }

            transform.localScale = new Vector2(-1f, 1f); // Left direction
        }
    }

    void SetIsStanding()
    {
        _player.IsOnTheGround = false;
        _animator.SetBool("IsOnTheGround", _player.IsOnTheGround);
    }

    public void AllowMovement()
    {
        _player.AllowMovement = true;
    }

    public bool IsFacingRight()
    {
        return transform.localScale.x > 0;
    }

    void FlipThePlayerBack()
    {
        if (!IsFacingRight())
            transform.localScale = new Vector2(1f, 1f); // Right direction
        else if (IsFacingRight())
            transform.localScale = new Vector2(-1f, 1f); // Left direction
    }

    void SetFlippedToFalse()
    {
        if (!_player.IsFlipped)
            return;

        _player.IsFlipped = false;
        _animator.SetBool("IsFlipped", _player.IsFlipped);
    }

    void AddSlipperyForce()
    {
        float forceValue = _player.SlipperyForce;

        if (_player.IsUmbrellaOpened)
            forceValue *= 1.5f;


        if (IsFacingRight())
            _rigidbody2d.AddForce(new Vector2(forceValue, 0));
        else
            _rigidbody2d.AddForce(new Vector2(-forceValue, 0));
    }

    #endregion

    #region SFX Methods

    void StopAudioSource()
    {
        _audioSource.Stop();
    }

    void PlayArmBreakingStartSfx()
    {
        _audioSource.clip = WhooshesAndSwooshes[1];
        _audioSource.Play();
    }

    void PlayArmBreakingWhooshSfx()
    {
        _audioSource.clip = WhooshesAndSwooshes[0];
        _audioSource.Play();
    }

    void PlayHasBeenKickedSfx()
    {
        _audioSource.clip = FallingDowns[1];
        _audioSource.Play();
    }

    void PlayStandingGrunt()
    {
        int randomHurtIndex = Random.Range(0, 2);
        _audioSource.PlayOneShot(Grunts[randomHurtIndex]);
    }

    void PlayRunningGrunt()
    {
        _audioSource.PlayOneShot(Grunts[2]);
    }

    void PlayOnAirAndCoverGrunt()
    {
        _audioSource.PlayOneShot(Grunts[3]);
    }

    void PlayDyingSfx()
    {
        if (_audioSource.isPlaying)
            return;

        _audioSource.clip = Grunts[7];
        _audioSource.Play();
    }

    void PlayDyingFallDown()
    {
        _audioSource.clip = FallingDowns[0];
        _audioSource.Play();
    }

    void PlayTakeCoverSfx()
    {
        _audioSource.clip = FallingDowns[4];
        _audioSource.Play();
    }

    void PlayRollingSfx()
    {
        _audioSource.clip = FallingDowns[2];
        _audioSource.Play();
    }

    void PlayRollingFinishedSfx()
    {
        _audioSource.clip = FallingDowns[3];
        _audioSource.Play();
    }

    void PlayUmbrellaOpening()
    {
        _audioSource.clip = UmbrellaSoundEffects[0];
        _audioSource.Play();
    }

    void PlayUmbrellaClosing()
    {
        _audioSource.clip = UmbrellaSoundEffects[1];
        _audioSource.Play();
    }

    void PlayKickSfx()
    {
        _audioSource.clip = WhooshesAndSwooshes[2];
        _audioSource.Play();
    }

    void PlayFlipSfx()
    {
        _audioSource.PlayOneShot(FlippingClip);
    }

    void PlayPutHatBackSfx()
    {
        _audioSource.PlayOneShot(PutHatBackClip);
    }

    void PlayGroundedSfx()
    {
        _audioSource.PlayOneShot(GroundedClip);
    }
    #endregion

    #region Actions

    bool CanBreakArm()
    {
        if (!_player.AllowBreakingArm || _player.IsKicking)
            return false;

        bool result = false;
        float castDist = 0.6f;

        if (!IsFacingRight())
            castDist = -castDist;

        Vector2 endPos = transform.position + Vector3.right * castDist;
        RaycastHit2D hit = Physics2D.Linecast(transform.position, endPos, 1 << LayerMask.NameToLayer("Enemies"));

        if (hit.collider != null)
        {
            if (hit.collider.gameObject.CompareTag("Enemy"))
                result = true;
            else
                result = false;

            Debug.DrawLine(transform.position, hit.point, Color.green);
            EnemyController collidedEnemy = hit.collider.gameObject.GetComponent<EnemyController>();

            if (result && collidedEnemy != null && Input.GetKey(KeyCode.E))
            {
                if (!collidedEnemy.HasEnemyInjured() || collidedEnemy.IsEnemyDead())
                    return false;

                _player.Speed = 0;
                _playerHorizontalInput = 0;
                _animator.SetFloat("Speed", _player.Speed);
                _player.AllowMovement = false;
                _player.AllowBreakingArm = false;
                Invoke("AllowBreakingArm", 2f);

                float enemyCurPosX = collidedEnemy.gameObject.transform.position.x;
                float additionalXPos = 0.6f;
                if (collidedEnemy.transform.localScale.x < 0)
                    additionalXPos = -additionalXPos;

                transform.position = new Vector3(enemyCurPosX + additionalXPos, transform.position.y, transform.position.z);

                _player.IsBreakingArm = true;
                _animator.SetBool("IsBreakingArm", _player.IsBreakingArm);

                BreakEnemyArm(collidedEnemy);
            }
        }
        else
        {
            Debug.DrawLine(transform.position, endPos, Color.red);
        }

        return result;
    }


    void AllowBreakingArm()
    {
        _player.AllowBreakingArm = true;
    }

    void BreakEnemyArm(EnemyController enemy)
    {
        enemy.ArmHasBroken();
    }

    public void BreakingArmFinished()
    {
        _player.IsBreakingArm = false;
        _animator.SetBool("IsBreakingArm", _player.IsBreakingArm);
    }

    public void Kicking(DoorBehaviour doorObject)
    {
        if (!_player.IsKicking && !_player.IsBreakingArm)
        {
            _currCollidedDoor = doorObject;
            _rigidbody2d.velocity = new Vector2(0, 0);
            _player.AllowMovement = false;
            _player.Speed = 0;
            _playerHorizontalInput = 0;
            _animator.SetFloat("Speed", _player.Speed);

            _player.IsKicking = true;
            _animator.SetBool("IsKicking", _player.IsKicking);
        }
    }

    void OpenTheDoor()
    {
        if (_currCollidedDoor != null)
            _currCollidedDoor.SetIsOpened();
    }

    public bool IsOnKickingState()
    {
        return _player.IsKicking;
    }

    public void KickingFinished()
    {
         _player.IsKicking = false;
        _animator.SetBool("IsKicking", _player.IsKicking);
    }
    #endregion

    #region Cover

    public bool CheckIfPlayerIsOnAction()
    {
        if (_player.IsCovering || _player.IsKicking || _player.IsBreakingArm || _player.HasBeenKicked || _player.IsOnTheGround || _player.IsRolling || _player.IsShooting
            || _player.IsUmbrellaOpened)
            return true;

        return false;
    }

    void UmbrellaCover()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            if (_player.IsKicking || _player.HasBeenKicked || _player.IsOnTheGround || _player.IsRolling)
                return;

            _player.IsUmbrellaOpened = true;
            _animator.SetBool("IsUmbrellaOpened", _player.IsUmbrellaOpened);
        }
        else
        {
            _player.IsUmbrellaOpened = false;
            _animator.SetBool("IsUmbrellaOpened", _player.IsUmbrellaOpened);
        }
    }

    public bool IsPlayerCoveredByUmbrella()
    {
        return _player.IsUmbrellaCoverActive;
    }

    void SetUmbrellaCoverActive()
    {
        _player.IsUmbrellaCoverActive = true;
        _animator.SetBool("IsUmbrellaCoverActive", _player.IsUmbrellaCoverActive);

    }

    void SetUmbrellaCoverDeactive()
    {
        _player.IsUmbrellaCoverActive = false;
        _animator.SetBool("IsUmbrellaCoverActive", _player.IsUmbrellaCoverActive);
    }

    bool CheckIfUmbrellaCoverStartedOrActive()
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Umbrella_Opening") || _animator.GetCurrentAnimatorStateInfo(0).IsName("Umbrella_Opening_While_Running") ||
            _animator.GetCurrentAnimatorStateInfo(0).IsName("Umbrella_Closing") || _player.IsUmbrellaOpened || _player.IsUmbrellaCoverActive)
            return true;
        else
            return false;
    }

    public bool TakeCoverToObject(float objXPos, float objScaleX, bool initializeRotableObj, GameObject currCollidedCoverableObj = null)
    {
        if (_player.IsCovering || _player.IsBreakingArm || _player.HasBeenKicked || _player.IsOnTheGround || _player.IsRolling ||
            _player.IsFlipped || CheckIfUmbrellaCoverStartedOrActive())
            return false;

        _rigidbody2d.velocity = new Vector2(0, 0);
        _player.AllowMovement = false;
        _player.Speed = 0;
        _playerHorizontalInput = 0;
        _animator.SetFloat("Speed", _player.Speed);

        float additionalXPos = 0;

        if (objScaleX > 0)
        {
            additionalXPos = 0.098f;
            if (!IsFacingRight())
                additionalXPos = 0.8f;

        } else
        {
            additionalXPos = -0.8f;
            if (!IsFacingRight())
                additionalXPos = -0.084f;
        }

        // If object hasn't flipped yet, continue here...
        if (initializeRotableObj)
        {
            if (IsFacingRight())
                additionalXPos = -0.1f;
            else
                additionalXPos = 0.1f;

            Vector2 rotableTablePos = new Vector2(transform.position.x + additionalXPos, transform.position.y + 0.04f);
            _currCollidedCoverableObj = Instantiate(RotableTable, rotableTablePos, Quaternion.identity);

            if (!IsFacingRight())
                _currCollidedCoverableObj.transform.localScale = new Vector2(-1f, 1f);

            _currCollidedCoverableObj.GetComponent<CoverPerformableObjectBehaviour>().SetPlayerTrigger(false);
        }

        if (!initializeRotableObj)
        {
            transform.position = new Vector3(objXPos + additionalXPos, transform.position.y, transform.position.z);
            _currCollidedCoverableObj = currCollidedCoverableObj;
            _currCollidedCoverableObj.GetComponent<CoverPerformableObjectBehaviour>().SetPlayerTrigger(false);
        }

        _player.IsCovering = true;
        _player.AllowMovement = false;
        _animator.SetBool("IsCovering", _player.IsCovering);

        return true;
    }

    public void LeaveCover(bool isObjectDestroyed = false)
    {
        if (Input.GetKeyDown(KeyCode.F) || isObjectDestroyed)
        {
            if ((_player.IsShooting || _player.IsReloading || !_player.IsCovering) && !isObjectDestroyed)
                return;

            bool canLeaveCover = _animator.GetCurrentAnimatorStateInfo(0).IsName("StandBy_Covering") || _animator.GetCurrentAnimatorStateInfo(0).IsName("StopShooting_OnCover");

            if (_player.IsCovering && (canLeaveCover || isObjectDestroyed))
            {
                _playerWeaponController.SetLastSelectedWeaponDefaultAnimator();
                _player.IsAimingToLeftOnCover = false;
                transform.parent = null;
                _player.IsCovering = false;
                _animator.SetBool("IsCovering", false);
                _currCollidedCoverableObj.GetComponent<CoverPerformableObjectBehaviour>().SetPlayerTrigger(true);
            }
        }
    }

    public bool IsAimingToLeftOnCover()
    {
        return _player.IsAimingToLeftOnCover;
    }
    #endregion

    #region Hurt-Die Behaviours

    private bool _isPushDirLeft;

    public bool IsPlayerKickedByEnemy()
    {
        return _player.HasBeenKicked;
    }

    public void PlayerHasBeenKickedByEnemy(bool isDirLeft)
    {
        if (_player.IsOnTheGround || _player.IsBreakingArm || _player.IsCovering)
            return;

        _player.IsUmbrellaCoverActive = false;
        _player.IsRolling = false;
        _animator.SetBool("IsRolling", _player.IsRolling);
        _player.HasBeenKicked = true;
        _animator.SetBool("HasBeenKicked", _player.HasBeenKicked);
        _breakablesCollider.SetActive(true);
        _player.IsOnTheGround = true;
        _animator.SetBool("IsOnTheGround", _player.IsOnTheGround);
        _player.AllowMovement = false;

        if (isDirLeft)
            transform.localScale = new Vector3(1f, 1f);
        else
            transform.localScale = new Vector3(-1f, 1f);

        _isPushDirLeft = isDirLeft;
    }

    public void AddPushForce()
    {
        _player.Speed = 0;
        _playerHorizontalInput = 0;
        _animator.SetFloat("Speed", _player.Speed);
        float forceValue = 50f;

        if (_isPushDirLeft)
            forceValue = -forceValue;

        _rigidbody2d.AddForce(new Vector2(forceValue, 0));

        if (_audioSource.isPlaying)
            return;

        _audioSource.clip = Grunts[4];
        _audioSource.Play();
    }

    public void GetUp()
    {
        _breakablesCollider.SetActive(false);
        _player.HasBeenKicked = false;
        _animator.SetBool("HasBeenKicked", _player.HasBeenKicked);
    }

    object[] GetBloodEffect(WeaponType weaponType)
    {
        ParticleSystem selectedBloodEffect = new ParticleSystem();
        Vector2 bloodPos = new Vector2();

        switch (weaponType)
        {
            case WeaponType.Revolver:
            case WeaponType.Pistol1911:
                selectedBloodEffect = BloodEffect01;
                bloodPos = new Vector2(0.4f, 0.025f);
                break;
            case WeaponType.Shotgun:
                break;
            case WeaponType.Submachine:
                break;
            default:
                break;
        }

        return new object[] { selectedBloodEffect, bloodPos };
    }

    public void InstantiateBloodOnWall(WeaponType weapon)
    {
        Vector2 bloodOnWallPos;
        float randXPos = Random.Range(-0.1f, 0.150f);
        float randYPos = Random.Range(-0.250f, 0.250f);
        float randRotationZVal = Random.Range(0, -40f);

        bloodOnWallPos = new Vector2(transform.position.x + randXPos, transform.position.y + randYPos);

        int bloodOnWallIndex = Random.Range(0, BloodOnWalls.Length);
        Instantiate(BloodOnWalls[bloodOnWallIndex], bloodOnWallPos, Quaternion.Euler(0, 0, randRotationZVal));
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

    public void GetDamage(float bulletScaleX, WeaponType weaponType, short amountOfDamageToReduce, float pushForce = 0, bool canInstantiateBloodEffect = true)
    {
        short curHealth = _gameManager.Health;

        if (!_player.AllowHurting || curHealth <= 0 || _player.HasBeenKicked || IsPlayerWalking)
            return;

        _player.AllowHurting = false;
        _player.HasBulletTaken = true;
        _animator.SetBool("HasBulletTaken", _player.HasBulletTaken);
        Invoke("SetAllowHurting", 1f);

        if(canInstantiateBloodEffect)
        {
            object[] bloodEffectVals = GetBloodEffect(weaponType);
            InstantiateBloodEffect(bloodEffectVals);
            InstantiateBloodOnWall(weaponType);
        }

        // Decrease health
        short totalDamage = 0;
        switch (weaponType)
        {
            case WeaponType.Revolver:
                totalDamage = (short)(_playerWeaponController.RevolverDamage - amountOfDamageToReduce);
                break;
            case WeaponType.Pistol1911:
                totalDamage = (short)(_playerWeaponController.Pistol1911Damage - amountOfDamageToReduce);
                break;
            case WeaponType.Shotgun:
                totalDamage = (short)(_playerWeaponController.ShotgunDamage - amountOfDamageToReduce);
                break;
            case WeaponType.Submachine:
                totalDamage = (short)(_playerWeaponController.SubmachineDamage - amountOfDamageToReduce);
                break;
            default:
                break;
        }

        curHealth = _gameManager.DecreaseHealth(totalDamage);

        if (curHealth <= 0)
            SetIsDead();
    }

    private void SetAllowHurting()
    {
        _player.AllowHurting = true;
    }

    public void RecoverPlayer()
    {
        _player.HasBulletTaken = false;
        _animator.SetBool("HasBulletTaken", _player.HasBulletTaken);
    }

    public void SetIsDead()
    {
        _player.IsDead = true;
        _animator.SetBool("IsDead", _player.IsDead);
    }

    public void DestroyComponents()
    {
        Destroy(_capsuleCollider);
        Destroy(_audioSource, 3f);

        GameObject cigarGo = transform.GetChild(0).gameObject;
        if (cigarGo != null)
            Destroy(cigarGo);

        GameObject umbrellaElectricityGo = transform.GetChild(1).gameObject;
        if (umbrellaElectricityGo != null)
            Destroy(umbrellaElectricityGo);

        _footStepController.DestroyGameobject();
    }

    public void DestroyAnimator()
    {
        Destroy(_animator);
    }
    #endregion
}