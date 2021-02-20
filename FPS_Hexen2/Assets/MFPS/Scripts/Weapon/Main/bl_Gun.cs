using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Serialization;
using MFPS.Core.Motion;

[RequireComponent(typeof(AudioSource))]
public class bl_Gun : bl_GunBase
{
    #region Public members
    public Component WeaponBinding;
    public float CrossHairScale = 8;
    public BulletInstanceMethod bulletInstanceMethod = BulletInstanceMethod.Pooled;
    public string BulletName = "bullet";
    public GameObject bulletPrefab;
    public GameObject grenade = null;       // the grenade style round... this can also be used for arrows or similar rounds
    public ParticleSystem muzzleFlash = null;     // the muzzle flash for this weapon
    public Transform muzzlePoint = null;    // the muzzle point of this weapon
    public ParticleSystem shell = null;          // the weapons empty shell particle
    public GameObject impactEffect = null;  // impact effect, used for raycast bullet types 
    public Vector3 AimPosition; //position of gun when is aimed
    private Vector3 DefaultPos;
    private Vector3 CurrentPos;
    public bool useSmooth = true;
    public float AimSmooth;
    public ShakerPresent shakerPresent;
    [Range(0, 179), FormerlySerializedAs("AimFog")]
    public float aimZoom = 50;
    public bool CanAuto = true;
    public bool CanSemi = true;
    public bool CanSingle = true;
    //Shotgun Specific Vars
    public int pelletsPerShot = 10;         // number of pellets per round fired for the shotgun
    public float delayForSecondFireSound = 0.45f;
    //Burst Specific Vars
    public int roundsPerBurst = 3;          // number of rounds per burst fire
    public float lagBetweenBurst = 0.5f;    // time between each shot in a burst
    private bool isBursting = false;
    //Launcher Specific Vars
    public List<GameObject> OnAmmoLauncher = new List<GameObject>();
    public bool ThrowByAnimation = false;
    public int impactForce = 50;            // how much force applied to a rigid body
    public float bulletSpeed = 200.0f;      // how fast are your bullets
    public bool AutoReload = true;
    public bool m_AllowQuickFire = true;
    public ReloadPer reloadPer = ReloadPer.Bullet;
    public int bulletsPerClip = 50;         // number of bullets in each clip
    public int numberOfClips = 5;           // number of clips you start with
    public int maxNumberOfClips = 10;       // maximum number of clips you can hold
    public float DelayFire = 0.85f;
    public Vector2 spreadMinMax = new Vector2(1, 3);
    public float spreadAimMultiplier = 0.5f;
    public float spreadPerSecond = 0.2f;    // if trigger held down, increase the spread of bullets
    public float spread = 0.0f;             // current spread of the gun
    public float decreaseSpreadPerSec = 0.5f;// amount of accuracy regained per frame when the gun isn't being fired 

    public float AimSwayAmount = 0.01f;
    [HideInInspector] public bool isReloading = false;       // am I in the process of reloading
    // Recoil
    public float RecoilAmount = 5.0f;
    public float RecoilSpeed = 2;
    public bool SoundReloadByAnim = false;
    public AudioClip TakeSound;
    public AudioClip FireSound;
    public AudioClip DryFireSound;
    public AudioClip ReloadSound;
    public AudioClip ReloadSound2 = null;
    public AudioClip ReloadSound3 = null;
    public AudioSource DelaySource = null;
    //cached player components
    public Renderer[] weaponRenders = null;
    public bl_PlayerSettings playerSettings;
    #endregion

    #region Public properties
    public IWeapon weaponLogic { get { if (WeaponBinding != null) { return WeaponBinding as IWeapon; } else { return null; } } }
    public float nextFireTime { get; set; }
    public Camera PlayerCamera { get; private set; }
    public bool BlockAimFoV { get; set; }
    public bool HaveInfinityAmmo { get; private set; }
    #endregion

    #region Private members
    private bool m_enable = true;
    private bl_WeaponBob GunBob;
    private bl_WeaponSway SwayGun = null;
    private bl_Recoil RecoilManager;
    private bl_UCrosshair Crosshair;
#if MFPSM
    private bl_TouchHelper TouchHelper;
    private bl_AutoWeaponFire AutoFire;
#endif
    public bool canBeTakenWhenIsEmpty = true;
    private bool alreadyKnife = false;
    private AudioSource Source;
    private Camera WeaponCamera;
    private Text FireTypeText;
    private bool inReloadMode = false;
    private AmmunitionType AmmoType = AmmunitionType.Bullets;
    private AudioSource FireSource = null;
    private bl_ObjectPooling Pooling;
    private bool isInitialized = false;
    private Transform m_Transform;
    public BulletData BulletSettings = new BulletData();
    public PlayerFPState FPState = PlayerFPState.Idle;
    Vector3 firePosition = Vector3.zero;
    Quaternion fireRotation = Quaternion.identity;
    private Vector2 defaultSpreadRange;
    public int extraDamage = 0;
    private Transform defaultMuzzlePoint = null;
    private bool lastAimState = false;
    private float currentZoom;
    private float m_defaultSwayAmount;
    private bool grenadeFired = false;
    GameObject instancedBullet = null;
    RaycastHit hit;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        Initialized();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Initialized()
    {
        if (isInitialized) return;

        m_Transform = transform;
        GunBob = PlayerReferences.weaponBob;
        SwayGun = PlayerReferences.weaponSway;
        RecoilManager = PlayerReferences.recoil;
        playerSettings = PlayerReferences.playerSettings;

        Pooling = bl_ObjectPooling.Instance;
        if (FireSource == null) { FireSource = gameObject.AddComponent<AudioSource>(); FireSource.playOnAwake = false; }
        Crosshair = bl_UCrosshair.Instance;
        Source = GetComponent<AudioSource>();
        if (bl_UIReferences.Instance != null)
        {
            FireTypeText = bl_UIReferences.Instance.PlayerUI.FireTypeText;
        }
        PlayerCamera = PlayerReferences.playerCamera;
#if MFPSM
        TouchHelper = bl_TouchHelper.Instance;
        AutoFire = FindObjectOfType<bl_AutoWeaponFire>();
#endif
        defaultSpreadRange = spreadMinMax;
        AmmoType = bl_GameData.Instance.AmmoType;
        bl_UCrosshair.Instance.Block = false;
        Setup();
        weaponLogic?.Initialitate(this);
        isInitialized = true;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.OnFireClick += OnFire;
            bl_TouchHelper.OnReload += OnReload;
        }
#endif

        if (!isInitialized) return;

        base.OnEnable();
        Source.clip = TakeSound;
        Source.Play();
        CanFire = false;
        UpdateUI();
        if (WeaponAnimation)
        {
            float t = WeaponAnimation.DrawWeapon();
            Invoke(nameof(DrawComplete), t);
        }
        else
        {
            DrawComplete();
        }
        bl_EventHandler.onAmmoPickUp += this.OnPickUpAmmo;
        bl_EventHandler.onRoundEnd += this.OnRoundEnd;
        Crosshair.ActiveCrosshairForWeapon(this);
        SetFireTypeName();
        playerSettings?.DoSpawnWeaponRenderEffect(weaponRenders);
        OnAmmoLauncher.ForEach(x => { x?.SetActive(bulletsLeft > 0); });
#if MFPSTPV
        SetWeaponRendersActive(!bl_CameraViewSettings.IsThirdPerson());
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onAmmoPickUp -= this.OnPickUpAmmo;
        bl_EventHandler.onRoundEnd -= this.OnRoundEnd;
#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            bl_TouchHelper.OnFireClick -= OnFire;
            bl_TouchHelper.OnReload -= OnReload;
        }
#endif
        alreadyKnife = false;
        if (PlayerCamera == null) { PlayerCamera = transform.root.GetComponent<bl_PlayerSettings>().PlayerCamera; }
        StopAllCoroutines();
        if (isReloading) { inReloadMode = true; isReloading = false; }
        isAiming = false;
        isFiring = false;
        lastAimState = false;
        ResetDefaultMuzzlePoint();
    }

    /// <summary>
    /// Called by the weapon manager on all the equipped weapons
    /// even when they have not been enabled
    /// </summary>
    public void Setup(bool initial = false)
    {
        bulletsLeft = bulletsPerClip; // load gun on startup
        DefaultPos = transform.localPosition;
        if (PlayerCamera == null) { PlayerCamera = PlayerReferences.playerCamera; }
        WeaponCamera = PlayerReferences.weaponCamera;
        WeaponCamera.fieldOfView = bl_MFPS.Settings != null ? (float)bl_MFPS.Settings.GetSettingOf("Weapon FOV") : 55;

        if (!initial)
        {
            m_defaultSwayAmount = SwayGun.Smoothness;
            if (AmmoType == AmmunitionType.Bullets)
            {
                numberOfClips = bulletsPerClip * numberOfClips;
            }
        }
        CanAiming = true;
        Info = bl_GameData.Instance.GetWeapon(GunID);
        if (WeaponType != GunType.Shotgun && WeaponType != GunType.Sniper) { reloadPer = ReloadPer.Magazine; }
    }

    /// <summary>
    /// check what the player is doing every frame
    /// </summary>
    /// <returns></returns>
    public override void OnUpdate()
    {
        if (!m_enable)
            return;

        InputUpdate();
        Aim();
        DetermineUpperState();

        if (isFiring) // if the gun is firing
        {
            spread += (Crosshair.isCrouch) ? spreadPerSecond * 0.5f : spreadPerSecond; // gun is less accurate with the trigger held down
        }
        else
        {
            spread -= decreaseSpreadPerSec; // gun regains accuracy when trigger is released
        }
        spread = Mathf.Clamp(spread, BaseSpread, spreadMinMax.y);
    }

    /// <summary>
    /// All Input events 
    /// </summary>
    void InputUpdate()
    {
        if (bl_GameData.Instance.isChating || !gunManager.isGameStarted || !bl_RoomMenu.Instance.isCursorLocked) return;

        // Did the user press fire.... and what kind of weapon are they using ?  ===============
        if (bl_UtilityHelper.isMobile)
        {
#if MFPSM
            if (bl_GameData.Instance.AutoWeaponFire && AutoFire != null)
                HandleAutoFire();
            else
            {
                if (WeaponType == GunType.Machinegun && TouchHelper != null)
                {
                    if (TouchHelper.FireDown && CanFire)
                        LoopFire();
                }
            }
#endif
        }
        else
        {
#if MFPSM
            if (bl_GameData.Instance.AutoWeaponFire && AutoFire != null)
            {
                HandleAutoFire();
            }
#endif
            if (CanFire)
            {
                if (FireButtonDown)//is was pressed
                {
                    SingleFire();
                }
                if (FireButton)//if keep pressed
                {
                    LoopFire();
                }
            }
            else
            {
                if (FireButtonDown && bulletsLeft <= 0 && !isReloading)//if try fire and don't have more bullets
                {
                    PlayEmptyFireSound();
                }
            }
        }

        if (FireButtonDown && isReloading)//if try fire while reloading 
        {
            if (WeaponType == GunType.Sniper || WeaponType == GunType.Shotgun)
            {
                if (bulletsLeft > 0)//and has at least one bullet
                {
                    CancelReloading();
                }
            }
        }

        if (bl_UtilityHelper.isMobile)
        {
#if MFPSM
            isAiming = TouchHelper.isAim && CanAiming;
#endif
        }
        else
        {
            isAiming = AimButton && CanAiming;
        }

        if (bl_RoomMenu.Instance.isCursorLocked)
        {
            Crosshair.OnAim(isAiming);
        }

        if (bl_GameInput.Reload() && CanReload)
        {
            Reload();
        }

        if (WeaponType == GunType.Machinegun || WeaponType == GunType.Burst || WeaponType == GunType.Pistol)
        {
            ChangeTypeFire();
        }

        //used to decrease weapon accuracy as long as the trigger remains down =====================
        if (WeaponType != GunType.Grenade && WeaponType != GunType.Knife)
        {
            if (bl_UtilityHelper.isMobile)
            {
#if MFPSM
                if (!bl_GameData.Instance.AutoWeaponFire)
                {
                    isFiring = (TouchHelper.FireDown && CanFire);
                }
#endif
            }
            else
            {
                if (WeaponType == GunType.Machinegun)
                {
                    isFiring = (FireButton && CanFire); // fire is down, gun is firing
                }
                else
                {
                    if (FireButtonDown && CanFire)
                    {
                        isFiring = true;
                        CancelInvoke("CancelFiring");
                        Invoke("CancelFiring", 0.12f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// change the type of gun gust
    /// </summary>
    void ChangeTypeFire()
    {
        bool inp = Input.GetKeyDown(KeyCode.B);
#if INPUT_MANAGER
        inp = ((bl_Input.isButtonDown("FireType")));
#endif 
        if (inp)
        {
            switch (WeaponType)
            {
                case GunType.Machinegun:
                    if (CanSemi) WeaponType = GunType.Burst;
                    else if (CanSingle) WeaponType = GunType.Pistol;
                    break;
                case GunType.Burst:
                    if (CanSingle) WeaponType = GunType.Pistol;
                    else if (CanAuto) WeaponType = GunType.Machinegun;
                    break;
                case GunType.Pistol:
                    if (CanAuto) WeaponType = GunType.Machinegun;
                    else if (CanSemi) WeaponType = GunType.Burst;
                    break;
            }
            SetFireTypeName();
            gunManager.PlaySound(0);
        }
    }

    /// <summary>
    /// Called by mobile button event
    /// </summary>
    void OnFire()
    {
        if (!gunManager.isGameStarted) return;

        if (bulletsLeft <= 0 && !isReloading)
        {
            PlayEmptyFireSound();
        }

        if (isReloading)
        {
            if (WeaponType == GunType.Sniper || WeaponType == GunType.Shotgun)
            {
                if (bulletsLeft > 0)
                {
                    CancelReloading();
                }
            }
        }

        if (!CanFire)
            return;

        SingleFire();
    }

    /// <summary>
    /// 
    /// </summary>
    void HandleAutoFire()
    {
#if MFPSM
        bool fireDown = AutoFire.Fire();
        isFiring = fireDown;
        if (fireDown)
        {
            if (WeaponType == GunType.Machinegun)
                LoopFire();
            else
                SingleFire();
        }
#endif
    }

    /// <summary>
    /// Fire one time
    /// </summary>
    void SingleFire()
    {
        if (weaponLogic != null) { weaponLogic.OnFireDown(); }
        else
        {
            switch (WeaponType)
            {
                case GunType.Shotgun:
                    ShotgunFire();
                    break;
                case GunType.Burst:
                    if (!isBursting) { StartCoroutine(BurstFire()); }
                    break;
                case GunType.Grenade:
                    if (!grenadeFired) { GrenadeFire(); }
                    break;
                case GunType.Pistol:
                    MachineGunFire();
                    break;
                case GunType.Sniper:
                    SniperFire();
                    break;
                case GunType.Knife:
                    if (!alreadyKnife) { KnifeFire(); }
                    break;
            }
        }
    }

    /// <summary>
    /// Fire continuously
    /// </summary>
    void LoopFire()
    {
        if (WeaponType != GunType.Machinegun) return;

        if (weaponLogic != null) { weaponLogic.OnFire(); }
        else
            MachineGunFire();
    }

    #region Fire Handlers
    /// <summary>
    /// fire the machine gun
    /// </summary>
    void MachineGunFire()
    {
        // If there is more than one bullet between the last and this frame
        float time = Time.time;
        if (time - Info.FireRate > nextFireTime)
            nextFireTime = time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < time)
        {
            StartCoroutine(FireOneShot());
            if (WeaponAnimation != null)
            {
                if (isAiming)
                {
                    WeaponAnimation.AimFire();
                }
                else
                {
                    WeaponAnimation.Fire();
                }
            }
            OnFireCommons();
            if (!isAiming)
            {
                if (muzzleFlash) { muzzleFlash.Play(); }
            }
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                Reload();
            }
        }
    }

    /// <summary>
    /// fire the sniper gun
    /// </summary>
    void SniperFire()
    {
        float time = Time.time;
        if (time - Info.FireRate > nextFireTime)
            nextFireTime = time - Time.deltaTime;

        // Keep firing until we used up the fire time
        while (nextFireTime < time)
        {
            StartCoroutine(FireOneShot());
            if (WeaponAnimation != null)
            {
                WeaponAnimation.Fire();
            }
            StartCoroutine(DelayFireSound());
            OnFireCommons();
            gunManager.HeadAnimator.Play("Sniper", 0, 0);
            if (!isAiming)
            {
                if (muzzleFlash) { muzzleFlash.Play(); }
            }
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                Reload(delayForSecondFireSound + 0.2f);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private float KnifeFire(bool quickFire = false)
    {
        // If there is more than one shot  between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        float time = 0;
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            isFiring = true; // fire is down, gun is firing
            alreadyKnife = true;
            StartCoroutine(KnifeSendFire());

            Vector3 position = PlayerCamera.transform.position;
            Vector3 direction = PlayerCamera.transform.TransformDirection(Vector3.forward);

            RaycastHit hit;
            float range = playerReferences.cameraRay == null ? Info.Range : Info.Range + playerReferences.cameraRay.ExtraRayDistance;
            if (Physics.Raycast(position, direction, out hit, range))
            {
                if (hit.transform.CompareTag(bl_MFPS.HITBOX_TAG))
                {
                    var bp = hit.transform.GetComponent<bl_BodyPart>();
                    bp?.GetDamage(Info.Damage, PhotonNetwork.NickName, DamageCause.Player, transform.position, GunID);
                }
                else if (hit.transform.CompareTag(bl_MFPS.AI_TAG))
                {
                    if (hit.transform.GetComponent<bl_AIShooterHealth>() != null)
                    {
                        hit.transform.GetComponent<bl_AIShooterHealth>().DoDamage(Info.Damage, Info.Name, transform.position, bl_GameManager.LocalPlayerViewID, false, PhotonNetwork.LocalPlayer.GetPlayerTeam(), false, 0);
                        bl_ObjectPooling.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                    }
                    else if (hit.transform.GetComponent<bl_AIHitBox>() != null)
                    {
                        hit.transform.GetComponent<bl_AIHitBox>().DoDamage(Info.Damage, Info.Name, transform.position, bl_GameManager.LocalPlayerViewID, false, PhotonNetwork.LocalPlayer.GetPlayerTeam());
                        bl_ObjectPooling.Instance.Instantiate("blood", hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal));
                    }
                }
                else
                {
                    var damageable = hit.transform.GetComponent<IMFPSDamageable>();
                    if (damageable != null)
                    {
                        DamageData damageData = new DamageData()
                        {
                            Damage = (int)Info.Damage,
                            Direction = m_Transform.position,
                            MFPSActor = bl_GameManager.Instance.LocalActor,
                            ActorViewID = bl_GameManager.LocalPlayerViewID,
                            GunID = GunID,
                            From = LocalPlayer.NickName,
                        };
                        damageData.Cause = DamageCause.Player;
                        damageable.ReceiveDamage(damageData);
                    }
                }
            }

            if (WeaponAnimation != null)
            {
                time = WeaponAnimation.KnifeFire(quickFire);
            }
            PlayerNetwork.IsFire(GunType.Knife, Vector3.zero);
            OnFireCommons();
            Crosshair.OnFire();
            isFiring = false;
            bl_EventHandler.DispatchLocalPlayerFire(GunID);
        }
        return time;
    }

    /// <summary>
    /// burst shooting
    /// </summary>
    /// <returns></returns>
    IEnumerator BurstFire()
    {
        int shotCounter = 0;
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - lagBetweenBurst > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        int shots = Mathf.Min(roundsPerBurst, bulletsLeft);
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            while (shotCounter < shots)
            {
                isBursting = true;
                StartCoroutine(FireOneShot());
                shotCounter++;
                OnFireCommons();
                if (muzzleFlash) { muzzleFlash.Play(); }
                WeaponAnimation?.Fire();
                yield return new WaitForSeconds(Info.FireRate);
                if (bulletsLeft <= 0) { break; }
            }

            nextFireTime += lagBetweenBurst;
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                Reload();
            }
        }
        isBursting = false;
    }

    /// <summary>
    /// fire the shotgun
    /// </summary>
    void ShotgunFire()
    {
        // If there is more than one bullet between the last and this frame
        // Reset the nextFireTime
        if (Time.time - Info.FireRate > nextFireTime)
            nextFireTime = Time.time - Time.deltaTime;

        int pelletCounter = 0;  // counter used for pellets per round
        // Keep firing until we used up the fire time
        while (nextFireTime < Time.time)
        {
            do
            {
                StartCoroutine(FireOneShot());
                pelletCounter++; // add another pellet         
            } while (pelletCounter < pelletsPerShot); // if number of pellets fired is less then pellets per round... fire more pellets

            if (!SoundReloadByAnim)
                StartCoroutine(DelayFireSound());
            else
                PlayFireAudio();

            WeaponAnimation?.Fire();
            OnFireCommons();
            if (!isAiming)
            {
                if (muzzleFlash) { muzzleFlash.Play(); }
            }
            //is Auto reload
            if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
            {
                Reload(delayForSecondFireSound + 0.3f);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GrenadeFire(bool fastFire = false)
    {
        if (grenadeFired || (Time.time - nextFireTime) <= Info.FireRate)
            return;

        if (!fastFire && bulletsLeft == 0 && numberOfClips > 0)
        {
            Reload(1); // if out of ammo, reload
            return;
        }
        isFiring = true;
        grenadeFired = true;
        if (ThrowByAnimation)
        {
            nextFireTime = Time.time + Info.FireRate;
            WeaponAnimation.FireGrenade(fastFire);
        }
        else { StartCoroutine(ThrowGrenade(fastFire)); }
    }

    /// <summary>
    /// fire your launcher
    /// </summary>
    public IEnumerator ThrowGrenade(bool fastFire = false, bool useDelay = true)
    {
        float t = 0;
        if (useDelay)
        {
            nextFireTime = Time.time + Info.FireRate;
            t = WeaponAnimation.FireGrenade(fastFire);
            float d = (fastFire) ? DelayFire + WeaponAnimation.GetDrawLenght : DelayFire;
            yield return new WaitForSeconds(d);
        }
        Vector3 angular = (Random.onUnitSphere * 10f);
        FireOneProjectile(angular); // fire 1 round            
        bulletsLeft--; // subtract a bullet
        UpdateUI();
        Kick();

        PlayerNetwork.IsFireGrenade(spread, muzzlePoint.position, transform.parent.rotation, angular);
        PlayFireAudio();
        isFiring = false;

        //hide the grenade mesh when doesn't have ammo
        if (bulletsLeft <= 0)
        {
            OnAmmoLauncher.ForEach(x =>
           {
               x?.SetActive(false);
           });
        }

        //is Auto reload
        if (!fastFire && AutoReload)
        {
            if (useDelay)
            {
                t = t - DelayFire;
                yield return new WaitForSeconds(t);
            }
            if (bulletsLeft <= 0 && numberOfClips > 0)
            {
                Reload(0);
            }
            else if (bulletsLeft <= 0)
            {
                gunManager?.OnReturnWeapon();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator FastGrenadeFire(System.Action callBack)
    {
        float tt = WeaponAnimation.GetFirePlusDrawLenght;
        GrenadeFire(true);
        yield return new WaitForSeconds(tt);
        if (numberOfClips > 0)
        {
            bulletsLeft++;
            if (!HaveInfinityAmmo) numberOfClips--;
            UpdateUI();
        }
        callBack();
        StopAllCoroutines();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void QuickMelee(System.Action callBack)
    {
        StartCoroutine(QuickMeleeSequence(callBack));
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator QuickMeleeSequence(System.Action callBack)
    {
        float tt = KnifeFire(true);
        yield return new WaitForSeconds(tt);
        callBack();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Create and fire a bullet
    /// </summary>
    /// <returns></returns>
    IEnumerator FireOneShot()
    {
        // set the gun's info into an array to send to the bullet
        BuildBulletData();
        Vector3 hitPoint = GetBulletPosition(out firePosition, out fireRotation);
        //bullet info is set up in start function
        instancedBullet = Pooling.Instantiate(BulletName, firePosition, fireRotation); // create a bullet
        instancedBullet.GetComponent<bl_Bullet>().SetUp(BulletSettings);// send the gun's info to the bullet
        Crosshair.OnFire();
        PlayerNetwork.IsFire(WeaponType, hitPoint);
        if (WeaponType != GunType.Grenade)
        {
            Source.clip = FireSound;
            Source.spread = Random.Range(1.0f, 1.5f);
            Source.Play();
        }
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
        if ((bulletsLeft == 0))
        {
            Reload();  // if out of bullets.... reload
            yield break;
        }
    }

    /// <summary>
    /// Create and Fire 1 launcher projectile
    /// </summary>
    /// <returns></returns>
    void FireOneProjectile(Vector3 angular)
    {
        Vector3 position = muzzlePoint.position; // position to spawn rocket / grenade is at the muzzle point of the gun
        BuildBulletData();

        //Instantiate grenade
        GameObject newNoobTube = Instantiate(grenade, position, transform.parent.rotation) as GameObject;
        if (newNoobTube.GetComponent<Rigidbody>() != null)//if grenade have a rigidbody,then apply velocity
        {
            newNoobTube.GetComponent<Rigidbody>().angularVelocity = angular;
        }
        newNoobTube.GetComponent<bl_Projectile>().SetUp(BulletSettings);// send the gun's info to the grenade    
        grenadeFired = false;
        bl_EventHandler.DispatchLocalPlayerFire(GunID);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnFireCommons()
    {
        PlayFireAudio();
        bulletsLeft--;
        UpdateUI();
        nextFireTime += Info.FireRate;
        EjectShell();
        Kick();
        Shake();
    }

    /// <summary>
    /// Get the bullet spawn position
    /// </summary>
    public Vector3 GetBulletPosition(out Vector3 position, out Quaternion rotation)
    {
        Vector3 HitPoint = PlayerCamera.transform.forward * 100;
        rotation = m_Transform.parent.rotation;
        position = PlayerCamera.transform.position;
        if (Physics.Raycast(position, PlayerCamera.transform.forward, out hit))
        {
            if (bl_UtilityHelper.Distance(muzzlePoint.position, hit.point) > 5)
            {
                HitPoint = hit.point;
                rotation = Quaternion.LookRotation(HitPoint - muzzlePoint.position);
                position = muzzlePoint.position;
            }
        }
        return HitPoint;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public void BuildBulletData()
    {
        BulletSettings.Damage = Damage;
        BulletSettings.ImpactForce = impactForce;
        if (WeaponType == GunType.Sniper && !isAiming)
        {
            BulletSettings.Spread = spread * 3;
            BulletSettings.MaxSpread = spreadMinMax.y * 3;
        }
        else
        {
            BulletSettings.MaxSpread = spreadMinMax.y;
            BulletSettings.Spread = spread;
        }
        BulletSettings.Speed = bulletSpeed;
        BulletSettings.WeaponName = Info.Name;
        BulletSettings.Position = this.transform.root.position;
        BulletSettings.WeaponID = GunID;
        BulletSettings.isNetwork = false;
        BulletSettings.Range = Info.Range;
        BulletSettings.ActorViewID = bl_GameManager.LocalPlayerViewID;
        BulletSettings.MFPSActor = bl_GameManager.Instance.LocalActor;
    }

    /// <summary>
    /// Aiming control
    /// </summary>
    void Aim()
    {
        if (isAiming && !isReloading)
        {
            CurrentPos = AimPosition; //Place in the center ADS
            currentZoom = aimZoom; //create a zoom camera
            GunBob.Intensitity = 0.01f;
            SwayGun.Smoothness = m_defaultSwayAmount * 2.5f;
            SwayGun.Amount = AimSwayAmount;
            spreadMinMax = defaultSpreadRange * spreadAimMultiplier;
        }
        else // if not aimed
        {
            CurrentPos = DefaultPos; //return to default gun position       
            currentZoom = playerReferences.DefaultCameraFOV; //return to default fog
            GunBob.Intensitity = 1;
            SwayGun.Smoothness = m_defaultSwayAmount;
            SwayGun.ResetSettings();
            spreadMinMax = defaultSpreadRange;
        }
        float delta = Time.deltaTime;
        //apply position
        m_Transform.localPosition = useSmooth ? Vector3.Lerp(m_Transform.localPosition, CurrentPos, delta * AimSmooth) : //with smooth effect
        Vector3.MoveTowards(m_Transform.localPosition, CurrentPos, delta * AimSmooth); // with snap effect
        if (PlayerCamera != null && !BlockAimFoV)
        {
            PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, currentZoom + controller.RunFov, delta * AimSmooth);
        }
        GunBob.isAim = isAiming;
        if (lastAimState != isAiming)
        {
            bl_EventHandler.DispatchLocalAimEvent(isAiming);
            lastAimState = isAiming;
        }
    }

    /// <summary>
    /// send kick back to mouse look
    /// when is fire
    /// </summary>
    public void Kick() => RecoilManager.SetRecoil(RecoilAmount, RecoilSpeed);

    /// <summary>
    /// 
    /// </summary>
    void Shake()
    {
        float influence = isAiming ? 0.5f : 1;
        bl_EventHandler.DoPlayerCameraShake(shakerPresent, "fpweapon", influence);
    }

    /// <summary>
    /// 
    /// </summary>
    void EjectShell()
    {
        if (shell != null)
            shell?.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckBullets(float delay = 0)
    {
        if (bulletsLeft <= 0 && numberOfClips > 0 && AutoReload)
        {
            Reload(delay);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnReload()
    {
        if (!CanReload) return;

        Reload();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reload(float delay = 0.2f)
    {
        if (isReloading)
            return;

        StartCoroutine(DoReload(delay));
    }

    /// <summary>
    /// start reload weapon
    /// deduct the remaining bullets in the cartridge of a new clip
    /// as this happens, we disable the options: fire, aim and run
    /// </summary>
    /// <returns></returns>
    IEnumerator DoReload(float waitTime = 0.2f)
    {
        isAiming = false;
        CanFire = false;

        if (isReloading)
            yield break; // if already reloading... exit and wait till reload is finished


        yield return new WaitForSeconds(waitTime);

        if (numberOfClips > 0 || inReloadMode)//if have at least one cartridge
        {
            isReloading = true; // we are now reloading
            if (WeaponAnimation != null)
            {
                if (reloadPer == ReloadPer.Bullet)//insert one bullet at a time
                {
                    int t_repeat = bulletsPerClip - bulletsLeft; //get the number of spent bullets
                    int add = (numberOfClips >= t_repeat) ? t_repeat : numberOfClips;
                    WeaponAnimation.SplitReload(Info.ReloadTime, add);
                    yield break;
                }
                else
                {
                    WeaponAnimation.Reload(Info.ReloadTime);
                }
            }
            if (!SoundReloadByAnim)
            {
                StartCoroutine(ReloadSoundIE());
            }
            if (!inReloadMode)// take away a clip
            {
                if (AmmoType == AmmunitionType.Clips)
                {
                   if(!HaveInfinityAmmo) numberOfClips--;
                }
            }
            if (WeaponType == GunType.Grenade) { OnAmmoLauncher.ForEach(x => { x?.SetActive(true); }); }
            yield return new WaitForSeconds(Info.ReloadTime); // wait for set reload time
            if (AmmoType == AmmunitionType.Clips)
            {
                bulletsLeft = bulletsPerClip; // fill up the gun
            }
            else
            {
                int need = bulletsPerClip - bulletsLeft;
                int add = (numberOfClips >= need) ? need : numberOfClips;
                bulletsLeft += add;
                if (!HaveInfinityAmmo) numberOfClips -= add;
            }
        }
        UpdateUI();
        isReloading = false; // done reloading
        CanAiming = true;
        CanFire = true;
        inReloadMode = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bullet"></param>
    public void AddBullet(int bullet)
    {
        if (AmmoType == AmmunitionType.Bullets)
        {
            numberOfClips -= bullet;
        }
        bulletsLeft += bullet;
        UpdateUI();
    }

    /// <summary>
    /// Set unlimited ammo to this weapon
    /// </summary>
    /// <param name="infinity"></param>
    public void SetInifinityAmmo(bool infinity)
    {
        HaveInfinityAmmo = infinity;
        bulletsLeft = bulletsPerClip;
        numberOfClips = AmmoType == AmmunitionType.Bullets ? bulletsPerClip * numberOfClips : numberOfClips;
        if (gameObject.activeInHierarchy)
        UpdateUI();
    }

    /// <summary>
    /// Sync Weapon state for Upper animations
    /// </summary>
    void DetermineUpperState()
    {
        if (PlayerNetwork == null)
            return;

        if (isFiring && !isReloading)
        {
            FPState = (isAiming) ? PlayerFPState.FireAiming : PlayerFPState.Firing;
        }
        else if (isAiming && !isFiring && !isReloading)
        {
            FPState = PlayerFPState.Aiming;
        }
        else if (isReloading)
        {
            FPState = PlayerFPState.Reloading;
        }
        else if (controller.State == PlayerState.Running && !isReloading && !isFiring && !isAiming)
        {
            FPState = PlayerFPState.Running;
        }
        else
        {
            FPState = PlayerFPState.Idle;
        }
        PlayerNetwork.FPState = FPState;
    }

    /// <summary>
    /// Set the weapon directly to the aim position.
    /// </summary>
    public void SetToAim() => m_Transform.localPosition = AimPosition;

    #region Audio
    /// <summary>
    /// 
    /// </summary>
    public void PlayFireAudio()
    {
        FireSource.clip = FireSound;
        FireSource.spread = Random.Range(1.0f, 1.5f);
        FireSource.pitch = Random.Range(1.0f, 1.075f);
        FireSource.Play();
    }


    /// <summary>
    /// most shotguns have the sound of shooting and then reloading
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayFireSound()
    {
        PlayFireAudio();
        yield return new WaitForSeconds(delayForSecondFireSound);
        if (DelaySource != null)
        {
            DelaySource.clip = ReloadSound3;
            DelaySource.Play();
        }
        else
        {
            Source.clip = ReloadSound3;
            Source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FinishReload()
    {
        if (AmmoType == AmmunitionType.Clips)
        {
            if (!HaveInfinityAmmo) numberOfClips--;
        }
        isReloading = false; // done reloading
        CanAiming = true;
        CanFire = true;
        inReloadMode = false;
    }

    public void PlayReloadAudio(int part)
    {
        if (SoundReloadByAnim) return;

        if (part == 0)
        {
            Source.clip = ReloadSound;
            Source.Play();
        }
        else if (part == 1)
        {
            Source.clip = ReloadSound2;
            Source.Play();
        }
        else if (part == 2)
        {
            Source.clip = ReloadSound3;
            Source.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayEmptyFireSound()
    {
        if (WeaponType != GunType.Knife && DryFireSound != null)
        {
            Source.clip = DryFireSound;
            Source.Play();
        }
    }

    /// <summary>
    /// use this method to various sounds reload.
    /// if you have only 1 sound, them put only one in inspector
    /// and leave empty other box
    /// </summary>
    /// <returns></returns>
    IEnumerator ReloadSoundIE()
    {
        float t_time = Info.ReloadTime / 3;
        if (ReloadSound != null)
        {
            Source.clip = ReloadSound;
            Source.Play();
            gunManager?.HeadAnimation(1, t_time);
        }
        if (ReloadSound2 != null)
        {
            if (WeaponType == GunType.Shotgun)
            {
                int t_repeat = bulletsPerClip - bulletsLeft;
                for (int i = 0; i < t_repeat; i++)
                {
                    yield return new WaitForSeconds(t_time / t_repeat + 0.025f);
                    Source.clip = ReloadSound2;
                    Source.Play();
                }
            }
            else
            {
                yield return new WaitForSeconds(t_time);
                Source.clip = ReloadSound2;
                Source.Play();
            }
        }
        if (ReloadSound3 != null)
        {
            yield return new WaitForSeconds(t_time);
            Source.clip = ReloadSound3;
            Source.Play();
            gunManager?.HeadAnimation(2, t_time);
        }
        yield return new WaitForSeconds(0.65f);
        gunManager?.HeadAnimation(0, t_time);
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator KnifeSendFire()
    {
        yield return new WaitForSeconds(0.5f);
        isFiring = false;
        alreadyKnife = false;
    }

    /// <summary>
    /// When we disable the gun ship called the animation
    /// and disable the basic functions
    /// </summary>
    public float DisableWeapon(bool isFastKill = false)
    {
        CanAiming = false;
        if (isReloading) { inReloadMode = true; isReloading = false; }
        CanFire = false;
        gunManager?.HeadAnimation(0, 1);
        if (PlayerCamera == null) { PlayerCamera = playerReferences.playerCamera; }
        if (!isFastKill) { StopAllCoroutines(); }
        return WeaponAnimation.HideWeapon();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetFireTypeName()
    {
        string n = string.Empty;
        switch (WeaponType)
        {
            case GunType.Machinegun:
                n = bl_GameTexts.FireTypeAuto;
                break;
            case GunType.Burst:
                n = bl_GameTexts.FireTypeSemi;
                break;
            case GunType.Pistol:
            case GunType.Shotgun:
            case GunType.Sniper:
                n = bl_GameTexts.FireTypeSingle;
                break;
            default:
                n = "--";
                break;
        }
        if (FireTypeText != null)
        {
            FireTypeText.text = n;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawComplete()
    {
        CanFire = true;
        CanAiming = true;
        if (inReloadMode) { Reload(0.2f); }
    }

    /// <summary>
    /// When round is end we can't fire
    /// </summary>
    void OnRoundEnd() => m_enable = false;

    /// <summary>
    /// 
    /// </summary>
    public void OnPickUpAmmo(int bullets, int projectiles, int gunID)
    {
        //if this is not a global ammo but for a specific weapon
        if (gunID != -1)
        {
            //and this is not the weapon that is for
            if (gunID != GunID) return;
        }

        if (WeaponType == GunType.Knife) return;
        if (AmmoType == AmmunitionType.Clips)
        {
            //max number of clips
            if (numberOfClips >= maxNumberOfClips) return;

            numberOfClips += Mathf.FloorToInt(bullets / bulletsPerClip);
            if (numberOfClips > maxNumberOfClips)
            {
                numberOfClips = maxNumberOfClips;
            }
        }
        else
        {
            if (WeaponType == GunType.Grenade)
            {
                numberOfClips += projectiles;
                bl_UIReferences.Instance.AddLeftNotifier(string.Format("+{0} {1}", projectiles.ToString(), Info.Name));
            }
            else
            {
                int oldCount = numberOfClips;
                numberOfClips += bullets;
                numberOfClips = Mathf.Clamp(numberOfClips, 0, bulletsPerClip * maxNumberOfClips);
                bl_UIReferences.Instance.AddLeftNotifier(string.Format("+{0} {1} Bullets", numberOfClips - oldCount, Info.Name));
            }
        }
        UpdateUI();
    }

    public void OverrideMuzzlePoint(Transform newPoint)
    {
        defaultMuzzlePoint = muzzlePoint;
        muzzlePoint = newPoint;
    }

    /// <summary>
    /// Enable/Disable the weapon renders/meshes
    /// </summary>
    public void SetWeaponRendersActive(bool active)
    {
        if (weaponRenders == null)
        {
            weaponRenders = m_Transform.GetComponentsInChildren<Renderer>();
        }

        foreach (var item in weaponRenders)
        {
            if (item == null) continue;
            item.gameObject.SetActive(active);
        }

#if CUSTOMIZER
        var customizerWeapon = GetComponent<bl_CustomizerWeapon>();
        if(active && customizerWeapon != null && customizerWeapon.ApplyOnStart)
        {
            customizerWeapon.LoadAttachments();
            customizerWeapon.ApplyAttachments();
        }
#endif
    }

    void CancelFiring() { isFiring = false; }
    void CancelReloading() { WeaponAnimation.CancelReload(); }
    public void ResetDefaultMuzzlePoint() { if (defaultMuzzlePoint != null) muzzlePoint = defaultMuzzlePoint; }
    public void UpdateUI() => bl_UIReferences.Instance.PlayerUI.UpdateWeaponState(this);
    public void SetDefaultWeaponCameraFOV(float fov) => WeaponCamera.fieldOfView = fov;

    #region Getters
    public GunType WeaponType { get => Info.Type; set => Info.Type = value; }
    private float BaseSpread
    {
        get { return Crosshair.isCrouch ? spreadMinMax.x * 0.5f : spreadMinMax.x; }
    }

    public bool FireButtonDown
    {
        get
        {
#if !INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return bl_Input.isButtonDown("SingleFire");
#endif
        }
    }

    public bool FireButton
    {
        get
        {
#if !INPUT_MANAGER
            return Input.GetMouseButton(0);
#else
            return bl_Input.isButton("Fire");
#endif
        }
    }

    public bool AimButton
    {
        get
        {
#if !INPUT_MANAGER
            return (Input.GetMouseButton(1));
#else
            return bl_Input.isButton("Aim");
#endif       
        }
    }

    public int Damage
    {
        get { return Info.Damage + extraDamage; }
        set { extraDamage = value; }//don't modify the base damage, only the extra value
    }

    public int GetCompactClips { get { return (numberOfClips / bulletsPerClip); } }

    /// <summary>
    /// Determine if the player can shoot this weapon.
    /// </summary>
    private bool m_canFire = false;
    public bool CanFire
    {
        get
        {
            return (bulletsLeft > 0 && m_canFire && !isReloading && FireWhileRun);
        }
        set => m_canFire = value;
    }

    public bool FireRatePassed { get { return (Time.time - nextFireTime) > Info.FireRate; } }
    public bool AllowQuickFire() => m_AllowQuickFire && bulletsLeft > 0 && FireRatePassed;

    /// <summary>
    ///  Determine if the player can aiming with this weapon in the current state
    /// </summary>
    private bool m_canAim;
    public bool CanAiming
    {
        get
        {
            if (WeaponType == GunType.Grenade || WeaponType == GunType.Knife) return false;
            return (m_canAim && controller.State != PlayerState.Running);
        }
        set => m_canAim = value;
    }

    /// <summary>
    /// Determine if the player can reload this weapon
    /// </summary>
    bool CanReload
    {
        get
        {
            bool can = false;
            if (bulletsLeft < bulletsPerClip && numberOfClips > 0 && controller.State != PlayerState.Running && !isReloading)
            {
                can = true;
            }
            if (WeaponType == GunType.Knife && nextFireTime < Time.time)
            {
                can = false;
            }
            return can;
        }
    }

    bool FireWhileRun
    {
        get
        {
            if (bl_GameData.Instance.CanFireWhileRunning)
            {
                return true;
            }
            if (controller.State != PlayerState.Running)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool CanBeTaken() { if (canBeTakenWhenIsEmpty) return true; else return (bulletsLeft > 0 || numberOfClips > 0); }

    public bl_FirstPersonController controller => PlayerReferences.firstPersonController;
    public bl_PlayerNetwork PlayerNetwork => PlayerReferences.playerNetwork;
    private bl_GunManager gunManager => PlayerReferences.gunManager;

    private bl_PlayerReferences playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (playerReferences == null) playerReferences = transform.root.GetComponent<bl_PlayerReferences>();
            return playerReferences;
        }
    }

    private bl_WeaponAnimation _anim;
    public bl_WeaponAnimation WeaponAnimation
    {
        get
        {
            if (_anim == null) { _anim = GetComponentInChildren<bl_WeaponAnimation>(); }
            return _anim;
        }
    }
    #endregion

    [System.Serializable]
    public enum BulletInstanceMethod
    {
        Pooled,
        Instanced,
    }

    [System.Serializable]
    public enum ReloadPer
    {
        Bullet,
        Magazine,
    }


#if UNITY_EDITOR
    public bool _aimRecord = false;
    public Vector3 _defaultPosition = new Vector3(-100, 0, 0);

    private void OnDrawGizmos()
    {
        if (muzzlePoint != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.4f);
            Gizmos.DrawSphere(muzzlePoint.position, 0.022f);
            Gizmos.color = Color.white;
        }
    }
#endif
}