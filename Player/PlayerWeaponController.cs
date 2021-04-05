using Assets.Code.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour, IBaseWeaponController
{
    #region SFX
    private AudioClip SelectedShotClip;
    private AudioClip SelectedEmptyClip;

    #region Settings
    [SerializeField] public short RevolverDamage = 5;
    [SerializeField] public short Pistol1911Damage = 8;
    [SerializeField] public short ShotgunDamage = 15;
    [SerializeField] public short SubmachineDamage = 4;
    #endregion

    #region General
    [SerializeField] AudioClip WeaponSwitchClip;
    #endregion

    #region Revolver
    [SerializeField] AudioClip RevolverShotClip;
    [SerializeField] AudioClip RevolverEmptyClip;
    [SerializeField] AudioClip RevolverChamberRemoveClip;
    [SerializeField] AudioClip RevolverChamberLoadClip;
    #endregion

    private AudioClip SelectedMagazineReleaseClip;
    private AudioClip SelectedMagazineLoadClip;
    private AudioClip SelectedCockingClip;

    #region Pistol 1911
    [SerializeField] AudioClip Pistol1911ShotClip;
    [SerializeField] AudioClip Pistol1911EmptyClip;
    [SerializeField] AudioClip Pistol1911MagazineReleaseClip;
    [SerializeField] AudioClip Pistol1911MagazineLoadClip;
    [SerializeField] AudioClip Pistol1911CockingClip;
    #endregion

    #region Shotgun
    [SerializeField] AudioClip ShotgunShotClip;
    [SerializeField] AudioClip ShotgunEmptyClip;
    [SerializeField] AudioClip ShotgunAmmoLoadClip;
    [SerializeField] AudioClip ShotgunPumpingClip;
    #endregion


    #region Submachine
    [SerializeField] AudioClip SubmachineShotClip;
    [SerializeField] AudioClip SubmachineMagazineReleaseClip;
    [SerializeField] AudioClip SubmachineMagazineLoadClip;
    [SerializeField] AudioClip SubmachineCockingClip;
    #endregion

    #endregion // SFX END

    #region VFX
    private GameObject SelectedBullet;
    private GameObject SelectedWeaponEntity;
    private ParticleSystem SelectedWeaponSmokeEffect;

    #region Revolver
    [SerializeField] GameObject RevolverBullet;
    [SerializeField] GameObject RevolverSixShells;
    [SerializeField] GameObject RevolverEntity;
    [SerializeField] ParticleSystem RevolverSmokeEffect;
    #endregion

    private GameObject SelectedWeaponMagazine;
    private ShellController SelectedShell;

    #region Pistol 1911
    [SerializeField] GameObject Pistol1911Bullet;
    [SerializeField] ShellController Pistol1911Shell;
    [SerializeField] GameObject Pistol1911Entity;
    [SerializeField] GameObject Pistol1911Magazine;
    [SerializeField] ParticleSystem Pistol1911SmokeEffect;
    #endregion

    #region Shotgun
    [SerializeField] GameObject ShotgunBullet;
    [SerializeField] ShellController ShotgunShell;
    [SerializeField] GameObject ShotgunEntity;
    [SerializeField] ParticleSystem ShotgunSmokeEffect;
    [SerializeField] ParticleSystem ShotgunSmokeEffectRight;
    [SerializeField] GameObject ShotgunDarkSmokeEffect;
    #endregion

    #region Submachine
    [SerializeField] GameObject SubmachineBullet;
    [SerializeField] GameObject SubmachineEntity;
    [SerializeField] GameObject SubmachineMagazine;
    [SerializeField] ParticleSystem SubmachineSmokeEffect;
    [SerializeField] ParticleSystem SubmachineSmokeEffectRight;
    #endregion
    #endregion VFX END

    #region VFX Positions
    private Vector2 _additionalBulletPos;
    private Vector2 _additionalBulletPosOnCover;
    private Vector2 _additionalBulletPosOnCoverToLeft;
    private Vector2 _additionalSmokePos;
    private Vector2 _additionalSmokePosOnCover;
    private Vector2 _additionalSmokePosOnCoverToLeft;
    private Vector2 _additionalShellPos;
    private Vector2 _additionalShellPosOnCover;
    private Vector2 _additionalShellPosOnCoverToLeft;
    private Vector2 _additionalMagazinePos;
    private Vector2 _additionalMagazinePosOnCover;
    #endregion

    #region Fields
    private GameManager _gameManager;
    [HideInInspector] public Player Player;
    [HideInInspector] public PlayerController PlayerController;
    [HideInInspector] public AudioSource _audioSource;
    [HideInInspector] public Animator _animator;
    private Color32 UISelectedWeaponColor = new Color32(169, 153, 91, 255);
    private bool _allowShooting;
    private bool _hasWeaponSwitched;
    private bool _allowChangingShootingDir;
    private bool _allowShotgunPumping;
    private bool _doesShotgunNeedsPumping;
    private KeyCode _lastPressedWeaponKeyCode;
    #endregion

    #region Properties
    private WeaponType _selectedWeapon;
    public WeaponType SelectedWeapon
    {
        get
        {
            return _selectedWeapon;
        }
        set
        {
            _selectedWeapon = value;
            SetSelectedWeaponValues(value);
        }
    }

    #region Ammunition
    private short _loadedRevolverAmmo;
    public short LoadedRevolverAmmo
    {
        get
        {
            return _loadedRevolverAmmo;
        }
        set
        {
            _loadedRevolverAmmo = value;

            if (_loadedRevolverAmmo == 0)
                _gameManager.RevolverAmmoText.color = Color.red;

           _gameManager.RevolverAmmoText.text = $"{_loadedRevolverAmmo}/∞";
        }
    }
    private short _loadedPistol1911Ammo;
    public short LoadedPistol1911Ammo
    {
        get
        {
            return _loadedPistol1911Ammo;
        }
        set
        {
            _loadedPistol1911Ammo = value;

            if (_loadedPistol1911Ammo == 0)
                _gameManager.Pistol1911AmmoText.color = Color.red;

            _gameManager.Pistol1911AmmoText.text = $"{_loadedPistol1911Ammo}/{_gameManager.TotalPistol1911Ammo}";
        }
    }
    private short _loadedShotgunAmmo;
    public short LoadedShotgunAmmo
    {
        get
        {
            return _loadedShotgunAmmo;
        }
        set
        {
            _loadedShotgunAmmo = value;

            if (_loadedShotgunAmmo == 0)
                _gameManager.ShotgunAmmoText.color = Color.red;

            _gameManager.ShotgunAmmoText.text = $"{_loadedShotgunAmmo}/{_gameManager.TotalShotgunAmmo}";
        }
    }

    private short _loadedSubmachineAmmo;
    public short LoadedSubmachineAmmo
    {
        get
        {
            return _loadedSubmachineAmmo;
        }
        set
        {
            _loadedSubmachineAmmo = value;

            if (_loadedSubmachineAmmo == 0)
                _gameManager.SubmachineAmmoText.color = Color.red;

            _gameManager.SubmachineAmmoText.text = $"{_loadedSubmachineAmmo}/{_gameManager.TotalSubmachineAmmo}";
        }
    }
    #endregion
    #endregion

    #region Animators
    [SerializeField] AnimatorOverrideController RevolverDefaultAnimator;
    [SerializeField] AnimatorOverrideController RevolverOnCoverAimingToLeftAnimator;
    [SerializeField] AnimatorOverrideController Pistol1911DefaultAnimator;
    [SerializeField] AnimatorOverrideController Pistol1911OnCoverAimingToLeftAnimator;
    [SerializeField] AnimatorOverrideController ShotgunDefaultAnimator;
    [SerializeField] AnimatorOverrideController ShotgunOnCoverAimingToLeftAnimator;
    [SerializeField] AnimatorOverrideController SubmachineDefaultAnimator;
    [SerializeField] AnimatorOverrideController SubmachineOnCoverAimingToLeftAnimator;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager.Instance;
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();

        LoadedRevolverAmmo = 6;
        LoadedPistol1911Ammo = 7;
        LoadedShotgunAmmo = 6;
        LoadedSubmachineAmmo = 50;
        SelectedWeapon = WeaponType.Revolver;
        _gameManager.RevolverAmmoTitle.color = UISelectedWeaponColor;
        _allowShooting = true;
        _allowChangingShootingDir = true;
        _allowShotgunPumping = true;
    }

    void Update()
    {
        if (Player.IsDead)
            return;

        Shooting();
        Reloading();
        ChangeWeaponController();
        ChangeCoverShootingDir();
    }

    #region Animation Controls

    bool CanChangeWeapon()
    {
        if (Player.IsBreakingArm || Player.IsKicking || Player.HasBeenKicked || Player.IsOnTheGround)
            return false;

        return true;
    }

    bool CanPlayChangeWeaponAnimation()
    {
        if (!Player.IsCovering && !Player.IsUmbrellaOpened && !Player.IsRolling)
            return true;

        return false;
    }

    void ChangeWeaponController()
    {
        if (!CanChangeWeapon())
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // Do not change to already selected weapon
            if (_lastPressedWeaponKeyCode == KeyCode.Alpha1)
                return;

            SwitchToDesiredWeapon(KeyCode.Alpha1);

            _gameManager.Pistol1911AmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.ShotgunAmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.RevolverAmmoTitle.color = UISelectedWeaponColor;
            _gameManager.SubmachineAmmoTitle.color = new Color32(217, 217, 217, 255);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Do not change to already selected weapon
            if (_lastPressedWeaponKeyCode == KeyCode.Alpha2)
                return;

            SwitchToDesiredWeapon(KeyCode.Alpha2);

            _gameManager.Pistol1911AmmoTitle.color = UISelectedWeaponColor;
            _gameManager.RevolverAmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.ShotgunAmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.SubmachineAmmoTitle.color = new Color32(217, 217, 217, 255);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // Do not change to already selected weapon
            if (_lastPressedWeaponKeyCode == KeyCode.Alpha3)
                return;

            SwitchToDesiredWeapon(KeyCode.Alpha3);

            _gameManager.ShotgunAmmoTitle.color = UISelectedWeaponColor;
            _gameManager.RevolverAmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.Pistol1911AmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.SubmachineAmmoTitle.color = new Color32(217, 217, 217, 255);

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // Do not change to already selected weapon
            if (_lastPressedWeaponKeyCode == KeyCode.Alpha4)
                return;

            SwitchToDesiredWeapon(KeyCode.Alpha4);

            _gameManager.SubmachineAmmoTitle.color = UISelectedWeaponColor;
            _gameManager.RevolverAmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.Pistol1911AmmoTitle.color = new Color32(217, 217, 217, 255);
            _gameManager.ShotgunAmmoTitle.color = new Color32(217, 217, 217, 255);
        }
    }

    void SwitchToDesiredWeapon(KeyCode keyCode)
    {
        _lastPressedWeaponKeyCode = keyCode;

        _audioSource.PlayOneShot(WeaponSwitchClip);
        ReloadingFinished(true);

        // Do not pump the shotgun while weapon switching
        _doesShotgunNeedsPumping = false;
        _animator.SetBool("DoesShotgunNeedsPumping", _doesShotgunNeedsPumping);

        if (CanPlayChangeWeaponAnimation())
        {
            _hasWeaponSwitched = true;
            _animator.SetBool("HasWeaponSwitched", _hasWeaponSwitched);
        }
        else
            WeaponSwitchFinished();
    }

    void WeaponSwitchFinished()
    {
        _hasWeaponSwitched = false;
        _animator.SetBool("HasWeaponSwitched", _hasWeaponSwitched);

        switch (_lastPressedWeaponKeyCode)
        {
            case KeyCode.Alpha1:
                SelectedWeapon = WeaponType.Revolver;
                _animator.runtimeAnimatorController = RevolverDefaultAnimator;
                break;
            case KeyCode.Alpha2:
                SelectedWeapon = WeaponType.Pistol1911;
                _animator.runtimeAnimatorController = Pistol1911DefaultAnimator;
                break;
            case KeyCode.Alpha3:
                SelectedWeapon = WeaponType.Shotgun;
                _animator.runtimeAnimatorController = ShotgunDefaultAnimator;
                break;
            case KeyCode.Alpha4:
                SelectedWeapon = WeaponType.Submachine;
                _animator.runtimeAnimatorController = SubmachineDefaultAnimator;
                break;
        }
    }

    public void SetLastSelectedWeaponDefaultAnimator()
    {
        switch (SelectedWeapon)
        {
            case WeaponType.Revolver:
                _animator.runtimeAnimatorController = RevolverDefaultAnimator;
                break;
            case WeaponType.Pistol1911:
                _animator.runtimeAnimatorController = Pistol1911DefaultAnimator;
                break;
            case WeaponType.Shotgun:
                _animator.runtimeAnimatorController = ShotgunDefaultAnimator;
                break;
            case WeaponType.Submachine:
                _animator.runtimeAnimatorController = SubmachineDefaultAnimator;
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Set selected weapon's visual and sound effects, bullet-shell x-y positions etc..
    /// </summary>
    /// <param name="selectedWeapon"></param>
    void SetSelectedWeaponValues(WeaponType selectedWeapon)
    {
        switch (selectedWeapon)
        {
            case WeaponType.Revolver:
                SelectedBullet = RevolverBullet;
                SelectedShotClip = RevolverShotClip;
                SelectedEmptyClip = RevolverEmptyClip;
                SelectedWeaponEntity = RevolverEntity;
                SelectedWeaponSmokeEffect = RevolverSmokeEffect;

                // Positions
                _additionalBulletPos = new Vector2(0.404f, 0.14f);
                _additionalBulletPosOnCover = new Vector2(0.425f, -0.125f);
                _additionalBulletPosOnCoverToLeft = new Vector2(-0.12f, -0.125f);

                _additionalSmokePos = new Vector2(0.394f, 0.112f);
                _additionalSmokePosOnCover = new Vector2(0.408f, -0.119f);
                _additionalSmokePosOnCoverToLeft = new Vector2(-0.10f, -0.119f);

                break;
            case WeaponType.Pistol1911:
                SelectedBullet = Pistol1911Bullet;
                SelectedShell = Pistol1911Shell;
                SelectedShotClip = Pistol1911ShotClip;
                SelectedEmptyClip = Pistol1911EmptyClip;
                SelectedWeaponEntity = Pistol1911Entity;
                SelectedWeaponSmokeEffect = Pistol1911SmokeEffect;
                SelectedWeaponMagazine = Pistol1911Magazine;
                SelectedMagazineReleaseClip = Pistol1911MagazineReleaseClip;
                SelectedMagazineLoadClip = Pistol1911MagazineLoadClip;
                SelectedCockingClip = Pistol1911CockingClip;

                // Positions
                _additionalBulletPos = new Vector2(0.404f, 0.14f);
                _additionalBulletPosOnCover = new Vector2(0.425f, -0.125f);
                _additionalBulletPosOnCoverToLeft = new Vector2(-0.12f, -0.125f);

                _additionalSmokePos = new Vector2(0.394f, 0.112f);
                _additionalSmokePosOnCover = new Vector2(0.408f, -0.119f);
                _additionalSmokePosOnCoverToLeft = new Vector2(-0.10f, -0.119f);

                _additionalShellPos = new Vector2(0.28f, 0.14f);
                _additionalShellPosOnCover = new Vector2(0.4f, -0.09f);
                _additionalShellPosOnCoverToLeft = new Vector2(0.0047f, -0.0869f);

                _additionalMagazinePos = new Vector2(0.27f, 0.115f);
                _additionalMagazinePosOnCover = new Vector2(0.0733f, -0.0871f);

                break;
            case WeaponType.Shotgun:
                SelectedBullet = ShotgunBullet;
                SelectedShell = ShotgunShell;
                SelectedShotClip = ShotgunShotClip;
                SelectedEmptyClip = ShotgunEmptyClip;
                SelectedWeaponEntity = ShotgunEntity;

                // Positions
                _additionalBulletPos = new Vector2(0.569f, 0.105f);
                _additionalBulletPosOnCover = new Vector2(0.425f, -0.125f);
                _additionalBulletPosOnCoverToLeft = new Vector2(-0.12f, -0.125f);

                _additionalSmokePos = new Vector2(0.52f, 0.066f);
                _additionalSmokePosOnCover = new Vector2(0.408f, -0.119f);
                _additionalSmokePosOnCoverToLeft = new Vector2(-0.10f, -0.119f);

                _additionalShellPos = new Vector2(0.202f, 0.109f);
                _additionalShellPosOnCover = new Vector2(0.4f, -0.09f);
                _additionalShellPosOnCoverToLeft = new Vector2(0.0047f, -0.0869f);
                break;
            case WeaponType.Submachine:
                SelectedBullet = SubmachineBullet;
                SelectedShell = Pistol1911Shell;
                SelectedShotClip = SubmachineShotClip;
                SelectedEmptyClip = Pistol1911EmptyClip;
                SelectedWeaponEntity = SubmachineEntity;
                SelectedWeaponMagazine = SubmachineMagazine;
                SelectedMagazineReleaseClip = SubmachineMagazineReleaseClip;
                SelectedMagazineLoadClip = SubmachineMagazineLoadClip;
                SelectedCockingClip = SubmachineCockingClip;

                // Positions
                _additionalBulletPos = new Vector2(0.456f, 0.075f);
                _additionalBulletPosOnCover = new Vector2(0.425f, -0.125f);
                _additionalBulletPosOnCoverToLeft = new Vector2(-0.12f, -0.125f);

                _additionalShellPos = new Vector2(0.279f, 0.1125f);
                _additionalShellPosOnCover = new Vector2(0.4f, -0.09f);
                _additionalShellPosOnCoverToLeft = new Vector2(0.0047f, -0.0869f);

                _additionalMagazinePos = new Vector2(0.189f, 0.0089f);
                _additionalMagazinePosOnCover = new Vector2(0.0733f, -0.0871f);

                break;
            default:
                break;
        }
    }

    #endregion

    #region SFX Methods
    void PlayRevolverChamberRemove()
    {
        if (Player.IsChamberRemoveClipPlayed)
            return;

        Player.IsChamberRemoveClipPlayed = true;
        _audioSource.clip = RevolverChamberRemoveClip;
        _audioSource.Play();
    }

    void PlayRevolverChamberLoad()
    {
        if (Player.IsChamberLoadClipPlayed)
            return;

        Player.IsChamberLoadClipPlayed = true;
        _audioSource.clip = RevolverChamberLoadClip;
        _audioSource.Play();
        ReloadingFinished();
    }

    void PlayMagazineReleaseSfx()
    {
        if (Player.IsChamberRemoveClipPlayed)
            return;

        Player.IsChamberRemoveClipPlayed = true;
        _audioSource.clip = SelectedMagazineReleaseClip;
        _audioSource.Play();
        InstatiateWeaponMagazine();
    }

    void PlayMagazineLoadSfx()
    {
        if (Player.IsChamberLoadClipPlayed)
            return;

        Player.IsChamberLoadClipPlayed = true;
        _audioSource.clip = SelectedMagazineLoadClip;
        _audioSource.Play();
    }

    void PlayCockingSfx()
    {
        _audioSource.clip = SelectedCockingClip;
        _audioSource.Play();
        ReloadingFinished();
    }

    void PlayShotgunPumpingSfx()
    {
        if (!_allowShotgunPumping)
            return;

        _allowShotgunPumping = false;
        Invoke("AllowShotgunPumping", 1f);
        CancelInvoke("SetShotgunPumping");
        _audioSource.PlayOneShot(ShotgunPumpingClip);
        InstantiateShell();
        _doesShotgunNeedsPumping = false;
        _animator.SetBool("DoesShotgunNeedsPumping", _doesShotgunNeedsPumping);
    }

    void PlayShotgunLoadPumpingSfx()
    {
        _audioSource.PlayOneShot(ShotgunPumpingClip);
        InstantiateShell();
        ReloadingFinished();
    }

    void PlayShotgunLoadSfx()
    {
        _audioSource.PlayOneShot(ShotgunAmmoLoadClip);
    }
    #endregion

    #region Shooting

    void AllowShooting()
    {
        _allowShooting = true;
    }

    void AllowShotgunPumping()
    {
        _allowShotgunPumping = true;
    }

    bool CheckIfThereIsAmmo()
    {
        bool result = true;
        switch (SelectedWeapon)
        {
            case WeaponType.Revolver:
                if (LoadedRevolverAmmo == 0)
                    result = false;
                break;
            case WeaponType.Pistol1911:
                if (LoadedPistol1911Ammo == 0)
                    result = false;
                break;
            case WeaponType.Shotgun:
                if (LoadedShotgunAmmo == 0)
                    result = false;
                break;
            case WeaponType.Submachine:
                if (LoadedSubmachineAmmo == 0)
                    result = false;
                break;
            default:
                break;
        }

        if(!result)
        {
            CancelShooting();
            if(!_audioSource.isPlaying)
            {
                _audioSource.clip = SelectedEmptyClip;
                _audioSource.Play();
            }
        }

        return result;
    }

    public void Shooting()
    {
        if (Input.GetKey(KeyCode.X))
        {
            if (!_allowShooting || Player.IsRolling || Player.IsKicking || Player.IsUmbrellaCoverActive || !CheckIfThereIsAmmo())
                return;

            if(_doesShotgunNeedsPumping)
            {
                Invoke("CancelShooting", 0);
                _animator.SetBool("DoesShotgunNeedsPumping", _doesShotgunNeedsPumping);
                return;
            }

            CancelInvoke("CancelShooting");
            SetIsShooting();
        }
        else if (Input.GetKeyUp(KeyCode.X))
        {
            Invoke("CancelShooting", 0.2f);
        }
    }

    void SetIsShooting()
    {
        Player.IsShooting = true;
        _animator.SetBool("IsShooting", Player.IsShooting);
    }

    public void CancelShooting()
    {
        Player.IsShooting = false;
        _animator.SetBool("IsShooting", Player.IsShooting);
    }

    #endregion

    #region Reloading

    bool CheckIfCanBeLoaded()
    {
        switch (SelectedWeapon)
        {
            case WeaponType.Revolver:
                if(LoadedRevolverAmmo != 6)
                    return true;
                break;
            case WeaponType.Pistol1911:
                if (_gameManager.TotalPistol1911Ammo >= 7 && LoadedPistol1911Ammo != 7)
                    return true;
                break;
            case WeaponType.Shotgun:
                if (_gameManager.TotalShotgunAmmo >= 1 && LoadedShotgunAmmo != 6)
                    return true;
                break;
            case WeaponType.Submachine:
                if (_gameManager.TotalSubmachineAmmo >= 50 && LoadedSubmachineAmmo != 50)
                    return true;
                break;
            default:
                break;
        }

        return false;
    }

    void ReloadSelectedWeapon()
    {
        switch (SelectedWeapon)
        {
            case WeaponType.Revolver:
                LoadedRevolverAmmo = 6;
                _gameManager.RevolverAmmoText.color = Color.white;
                break;
            case WeaponType.Pistol1911:
                if (!CheckIfCanBeLoaded())
                    return;

                _gameManager.TotalPistol1911Ammo -= 7;
                LoadedPistol1911Ammo = 7;
                _gameManager.Pistol1911AmmoText.color = Color.white;
                break;
            case WeaponType.Shotgun:
                if (!CheckIfCanBeLoaded())
                    return;

                short neededAmmo = (short) (6 - LoadedShotgunAmmo);

                if (_gameManager.TotalShotgunAmmo < neededAmmo)
                    neededAmmo = _gameManager.TotalShotgunAmmo;

                _gameManager.TotalShotgunAmmo -= neededAmmo;
                LoadedShotgunAmmo += neededAmmo;
                _gameManager.ShotgunAmmoText.color = Color.white;
                break;
            case WeaponType.Submachine:
                if (!CheckIfCanBeLoaded())
                    return;

                _gameManager.TotalSubmachineAmmo -= 50;
                LoadedSubmachineAmmo = 50;
                _gameManager.SubmachineAmmoText.color = Color.white;
                break;
            default:
                break;
        }
    }

    public void Reloading()
    {
        if (Input.GetKey(KeyCode.R) && !Player.IsReloading)
        {
            if (!CheckIfCanBeLoaded())
                return;

            Player.IsReloading = true;
            _animator.SetBool("IsReloading", Player.IsReloading);
        }
    }

    public void ReloadingFinished(bool isCancelled = false)
    {
        Player.IsReloading = false;
        Player.IsShellsDropped = false;
        Player.IsChamberRemoveClipPlayed = false;
        Player.IsChamberLoadClipPlayed = false;
        _animator.SetBool("IsReloading", Player.IsReloading);

        if(!isCancelled)
            ReloadSelectedWeapon();
    }
    #endregion

    #region Bullets-Shells-Magazines

    void SetShotgunPumping()
    {
        _doesShotgunNeedsPumping = true;
        _animator.SetBool("DoesShotgunNeedsPumping", _doesShotgunNeedsPumping);
    }

    void SetShotgunPumpingToFalse()
    {
        _doesShotgunNeedsPumping = false;
        _animator.SetBool("DoesShotgunNeedsPumping", _doesShotgunNeedsPumping);
    }

    void InstantiateBullet()
    {
        if (!CheckIfThereIsAmmo())
            return;

        _allowShooting = false;
        Invoke("AllowShooting", 0.25f);

        // Decrease ammo
        switch (SelectedWeapon)
        {
            case WeaponType.Revolver:
                LoadedRevolverAmmo--;
                break;
            case WeaponType.Pistol1911:
                LoadedPistol1911Ammo--;
                break;
            case WeaponType.Shotgun:
                LoadedShotgunAmmo--;

                if (!Player.IsCovering)
                    Invoke("SetShotgunPumping", 0.4f);
                break;
            case WeaponType.Submachine:
                LoadedSubmachineAmmo--;
                break;
            default:
                break;
        }

        // Play shot sfx
        _audioSource.PlayOneShot(SelectedShotClip);

        Vector2 additionalBulletPos = new Vector2();
        Vector2 additionalSmokePos = new Vector2();

        if (!Player.IsCovering)
        {
            additionalBulletPos = _additionalBulletPos;
            additionalSmokePos = _additionalSmokePos;
        }
        else
        {
            if (Player.IsAimingToLeftOnCover)
            {
                additionalBulletPos = _additionalBulletPosOnCoverToLeft;
                additionalSmokePos = _additionalSmokePosOnCoverToLeft;
            }
            else
            {
                additionalBulletPos = _additionalBulletPosOnCover;
                additionalSmokePos = _additionalSmokePosOnCover;
            }
        }

        //Instantiate smoke effect
        Vector2 bulletSmokeEffectPos;
        if (PlayerController.IsFacingRight())
        {
            bulletSmokeEffectPos = new Vector2(transform.position.x + additionalSmokePos.x, transform.position.y + additionalSmokePos.y);
            if (SelectedWeapon == WeaponType.Shotgun)
                SelectedWeaponSmokeEffect = ShotgunSmokeEffectRight;

            if(SelectedWeapon == WeaponType.Submachine)
            {
                SelectedWeaponSmokeEffect = SubmachineSmokeEffectRight;
                float extraRandXPos = Random.Range(-0.04f, 0.35f);
                float extraRandYPos = Random.Range(-0.025f, -0.1f);
                bulletSmokeEffectPos = new Vector2(bulletSmokeEffectPos.x + extraRandXPos, bulletSmokeEffectPos.y + extraRandYPos);
            }
        }
        else
        {
            bulletSmokeEffectPos = new Vector2(transform.position.x - additionalSmokePos.x, transform.position.y + additionalSmokePos.y);
            if (SelectedWeapon == WeaponType.Shotgun)
                SelectedWeaponSmokeEffect = ShotgunSmokeEffect;

            if (SelectedWeapon == WeaponType.Submachine)
            {
                SelectedWeaponSmokeEffect = SubmachineSmokeEffect;
                float extraRandXPos = Random.Range(-0.04f, 0.35f);
                float extraRandYPos = Random.Range(-0.025f, -0.1f);
                bulletSmokeEffectPos = new Vector2(bulletSmokeEffectPos.x - extraRandXPos, bulletSmokeEffectPos.y + extraRandYPos);
            }
        }

        Instantiate(SelectedWeaponSmokeEffect, bulletSmokeEffectPos, Quaternion.Euler(-90, 0, 0));

        Vector2 bulletPos;
        if (PlayerController.IsFacingRight())
            bulletPos = new Vector2(transform.position.x + additionalBulletPos.x, transform.position.y + additionalBulletPos.y);
        else
            bulletPos = new Vector2(transform.position.x - additionalBulletPos.x, transform.position.y + additionalBulletPos.y);

        Instantiate(SelectedBullet, bulletPos, Quaternion.identity);

        if (SelectedWeapon == WeaponType.Shotgun)
            Invoke("InstantiateShotgunDarkSmokeEffect", 0.2f);

        if (SelectedWeapon == WeaponType.Revolver || SelectedWeapon == WeaponType.Shotgun)
            return;

        InstantiateShell();
    }

    void InstantiateShell()
    {
        Vector2 additionalShellPos = new Vector2();

        if (!Player.IsCovering)
        {
            additionalShellPos = _additionalShellPos;
        }
        else
        {
            if (Player.IsAimingToLeftOnCover)
                additionalShellPos = _additionalShellPosOnCoverToLeft;
            else
                additionalShellPos = _additionalShellPosOnCover;
        }

        Vector2 shellPos;
        if (PlayerController.IsFacingRight())
            shellPos = new Vector2(transform.position.x + additionalShellPos.x, transform.position.y + additionalShellPos.y);
        else
            shellPos = new Vector2(transform.position.x - additionalShellPos.x, transform.position.y + additionalShellPos.y);

        Instantiate(SelectedShell, shellPos, Quaternion.Euler(0, 0, 25));
    }

    void InstatiateRevolverShells()
    {
        if (Player.IsShellsDropped)
            return;

        float additionalXPos = 0;
        float additionalYPos = 0;

        if (!Player.IsCovering)
        {
            additionalXPos = 0.27f;
            additionalYPos = 0.115f;
        }
        else
        {
            additionalXPos = 0.071f;
            additionalYPos = -0.038f;
        }

        Vector2 shellPos;
        if (PlayerController.IsFacingRight())
            shellPos = new Vector2(transform.position.x + additionalXPos, transform.position.y + additionalYPos);
        else
            shellPos = new Vector2(transform.position.x - additionalXPos, transform.position.y + additionalYPos);

        Instantiate(RevolverSixShells, shellPos, Quaternion.identity);
        Player.IsShellsDropped = true;
    }

    /// <summary>
    /// For Pistol 1911 and Thompson magazines
    /// </summary>
    void InstatiateWeaponMagazine()
    {
        if (Player.IsShellsDropped)
            return;

        Vector2 additionalMagazinePos = _additionalMagazinePos;

        if (Player.IsCovering)
            additionalMagazinePos = _additionalMagazinePosOnCover;

        Vector2 magazinePos;
        if (PlayerController.IsFacingRight())
            magazinePos = new Vector2(transform.position.x + additionalMagazinePos.x, transform.position.y + additionalMagazinePos.y);
        else
            magazinePos = new Vector2(transform.position.x - additionalMagazinePos.x, transform.position.y + additionalMagazinePos.y);

        Instantiate(SelectedWeaponMagazine, magazinePos, Quaternion.Euler(0, 0, -20));
        Player.IsShellsDropped = true;
    }

    void InstantiateShotgunDarkSmokeEffect()
    {
        Vector2 darkSmokePos;
        if (PlayerController.IsFacingRight())
            darkSmokePos = new Vector2(transform.position.x + 0.665f, transform.position.y + 0.01f);
        else
            darkSmokePos = new Vector2(transform.position.x - 0.665f, transform.position.y + 0.01f);

        Instantiate(ShotgunDarkSmokeEffect, darkSmokePos, Quaternion.identity);
    }

    #endregion

    #region Shooting On Cover
    void ChangeCoverShootingDir()
    {
        if (!Player.IsCovering)
            return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (PlayerController.IsFacingRight())
                ChangeShootingDirection(true);
            else
                ChangeShootingDirection(false);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (PlayerController.IsFacingRight())
                ChangeShootingDirection(false);
            else
                ChangeShootingDirection(true);
        }
    }

    void SetAllowChangingShootingDirection()
    {
        _allowChangingShootingDir = true;
    }

    void ChangeShootingDirection(bool setToLeft)
    {
        if (!_allowChangingShootingDir)
            return;

        _allowChangingShootingDir = false;

        switch (SelectedWeapon)
        {
            case WeaponType.Revolver:
                    if(setToLeft)
                        _animator.runtimeAnimatorController = RevolverOnCoverAimingToLeftAnimator;
                    else
                        _animator.runtimeAnimatorController = RevolverDefaultAnimator;
                break;
            case WeaponType.Pistol1911:
                if (setToLeft)
                    _animator.runtimeAnimatorController = Pistol1911OnCoverAimingToLeftAnimator;
                else
                    _animator.runtimeAnimatorController = Pistol1911DefaultAnimator;
                break;
            case WeaponType.Shotgun:
                if (setToLeft)
                    _animator.runtimeAnimatorController = ShotgunOnCoverAimingToLeftAnimator;
                else
                    _animator.runtimeAnimatorController = ShotgunDefaultAnimator;
                break;
            case WeaponType.Submachine:
                if (setToLeft)
                    _animator.runtimeAnimatorController = SubmachineOnCoverAimingToLeftAnimator;
                else
                    _animator.runtimeAnimatorController = SubmachineDefaultAnimator;
                break;
            default:
                break;
        }

        if (setToLeft)
            Player.IsAimingToLeftOnCover = true;
        else
            Player.IsAimingToLeftOnCover = false;

        Invoke("SetAllowChangingShootingDirection", 0.5f);
    }
    #endregion

    public void DropWeapon()
    {
        Vector2 weaponPos;
        if (PlayerController.IsFacingRight())
            weaponPos = new Vector2(transform.position.x + 0.2f, transform.position.y + -0.067f);
        else
            weaponPos = new Vector2(transform.position.x - 0.2f, transform.position.y + -0.067f);

        Instantiate(SelectedWeaponEntity, weaponPos, Quaternion.identity);
    }
}