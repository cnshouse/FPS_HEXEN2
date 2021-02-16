using UnityEngine;
using System.Collections.Generic;
using MFPS.Internal;

public class bl_PlayerAnimations : bl_MonoBehaviour
{
    [HideInInspector]
    public bool m_Update = true;
    [Header("Animations")]
    public Animator m_animator;
    public AnimationCurve dropTiltAngleCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [HideInInspector]
    public bool grounded = true;
    public Vector3 velocity { get; set; } = Vector3.zero;
    [HideInInspector]
    public Vector3 localVelocity = Vector3.zero;
    [HideInInspector]
    public float movementSpeed;
    [HideInInspector]
    public float lastYRotation;
    public PlayerFPState FPState { get; set; } = PlayerFPState.Idle;
    public PlayerState BodyState { get; set; } = PlayerState.Idle;
    private bool HitType = false;
    private GunType cacheWeaponType = GunType.Machinegun;
    private float vertical;
    private float horizontal;
    private Transform PlayerRoot;
    private float turnSpeed;
    private bool parent = false;
    private float TurnLerp = 0;
    [HideInInspector] public bl_NetworkGun EditorSelectedGun = null;

    public bool useFootSteps { get; set; } = false;
    public bool isWeaponsBlocked { get; set; }
    public bool stopHandsIK { get; set; }
    public float VelocityMagnitude { get; set; }
    private RaycastHit footRay;
    private float lerpValueSpeed = 12;
    private float reloadSpeed = 1;
    public bl_NetworkGun CurrentNetworkGun { get; set; }
    private PlayerState lastBodyState = PlayerState.Idle;
    private bl_Footstep footstep;
    private float deltaTime = 0.02f;
    private Transform m_Transform;
    private bl_PlayerReferences playerReferences;
    private Dictionary<string, int> animatorHashes;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();

        m_Transform = transform;
        useFootSteps = bl_GameData.Instance.CalculateNetworkFootSteps;
        playerReferences = transform.GetComponentInParent<bl_PlayerReferences>();
        PlayerRoot = playerReferences.transform;
        if (useFootSteps)
        {
            footstep = playerReferences.firstPersonController.footstep;
        }  
        if(animatorHashes == null)
        {
            FetchHashes();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_AnimatorReloadEvent.OnTPReload += OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AnimatorReloadEvent.OnTPReload -= OnTPReload;
    }

    /// <summary>
    /// 
    /// </summary>
    void FetchHashes()
    {
        //cache the hashes in a Array will be more appropriate but to be more readable for other users
        // I decide to cached them in a Dictionary with the key name indicating the parameter that contain
        animatorHashes = new Dictionary<string, int>();
        animatorHashes.Add("BodyState", Animator.StringToHash("BodyState"));
        animatorHashes.Add("Vertical", Animator.StringToHash("Vertical"));
        animatorHashes.Add("Horizontal", Animator.StringToHash("Horizontal"));
        animatorHashes.Add("Speed", Animator.StringToHash("Speed"));
        animatorHashes.Add("Turn", Animator.StringToHash("Turn"));
        animatorHashes.Add("isGround", Animator.StringToHash("isGround"));
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTPReload(bool enter, Animator theAnimator, AnimatorStateInfo stateInfo)
    {
        if (theAnimator != m_animator || CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null) return;

        reloadSpeed = enter ? (stateInfo.length / CurrentNetworkGun.Info.ReloadTime) - 0.1f : 1;
        m_animator.SetFloat("ReloadSpeed", reloadSpeed);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!m_Update)
            return;

        deltaTime = Time.deltaTime;
        ControllerInfo();
        Animate();
        UpperControll();
        UpdateFootstep();
        DropPlayerAngle();
    }

    /// <summary>
    /// 
    /// </summary>
    void ControllerInfo()
    {
        localVelocity = PlayerRoot.InverseTransformDirection(velocity);
        localVelocity.y = 0;

        float lerp = deltaTime * lerpValueSpeed;
        vertical = Mathf.Lerp(vertical, localVelocity.z, lerp);
        horizontal = Mathf.Lerp(horizontal, localVelocity.x, lerp);

        VelocityMagnitude = velocity.magnitude;
        turnSpeed = Mathf.DeltaAngle(lastYRotation, PlayerRoot.rotation.eulerAngles.y);
        TurnLerp = Mathf.Lerp(TurnLerp, turnSpeed, lerp);
        movementSpeed = Mathf.Lerp(movementSpeed, VelocityMagnitude, lerp);

        parent = !parent;
        if (parent)
        {
            lastYRotation = PlayerRoot.rotation.eulerAngles.y;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Animate()
    {
        if (m_animator == null)
            return;

        CheckPlayerStates();

        m_animator.SetInteger(animatorHashes["BodyState"], (int)BodyState);
        m_animator.SetFloat(animatorHashes["Vertical"], vertical);
        m_animator.SetFloat(animatorHashes["Horizontal"], horizontal);
        m_animator.SetFloat(animatorHashes["Speed"], movementSpeed);
        m_animator.SetFloat(animatorHashes["Turn"], TurnLerp);
        m_animator.SetBool(animatorHashes["isGround"], grounded);
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPlayerStates()
    {
        if (BodyState != lastBodyState)
        {
            if (lastBodyState == PlayerState.Sliding && BodyState != PlayerState.Sliding)
            {
                m_animator.CrossFade("Move", 0.2f, 0);
            }
            if (BodyState == PlayerState.Sliding)
            {
                m_animator.Play("Slide", 0, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Dropping))
            {
                m_animator.Play("EmptyUpper", 1, 0);
            }
            else if (OnEnterPlayerState(PlayerState.Gliding))
            {
                m_animator.Play("EmptyUpper", 1, 0);
                m_animator.CrossFade("gliding-1", 0.33f, 0);
            }

            if (OnExitPlayerState(PlayerState.Dropping))
            {
                m_Transform.localRotation = Quaternion.identity;
            }

            lastBodyState = BodyState;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnEnterPlayerState(PlayerState playerState)
    {
        if(BodyState == playerState && lastBodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool OnExitPlayerState(PlayerState playerState)
    {
        if (lastBodyState == playerState && BodyState != playerState)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    void UpperControll()
    {
        int _fpState = (int)FPState;
        if(_fpState == 9) { _fpState = 1; }
        m_animator.SetInteger("UpperState", _fpState);
    }

    /// <summary>
    /// 
    /// </summary>
    void DropPlayerAngle()
    {
        if (BodyState != PlayerState.Dropping) return;

        Vector3 pangle = m_Transform.localEulerAngles;
        float tilt = dropTiltAngleCurve.Evaluate(Mathf.Clamp01(VelocityMagnitude / (playerReferences.firstPersonController.dropTiltSpeedRange.y - 10)));
        pangle.x = Mathf.Lerp(0, 70, tilt);
        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(pangle), deltaTime * 4);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnWeaponBlock(int blockState)
    {
        isWeaponsBlocked = blockState == 1;
        if (blockState != 2)
        {
            int id = isWeaponsBlocked ? -1 : (int)cacheWeaponType;
            m_animator.SetInteger("GunType", id);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGetHit()
    {
        int r = Random.Range(0, 2);
        string hit = (r == 1) ? "Right Hit" : "Left Hit";
        m_animator.Play(hit, 2, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateFootstep()
    {
        if (!useFootSteps) return;
        if (VelocityMagnitude < 0.3f) return;
        bool isClimbing = (BodyState == PlayerState.Climbing);
        if ((!grounded && !isClimbing) || BodyState == PlayerState.Sliding)
            return;

        footstep?.UpdateStep(movementSpeed);
    }

    public void PlayFireAnimation(GunType typ)
    {
        switch (typ)
        {
            case GunType.Knife:
                m_animator.Play("FireKnife", 1, 0);
                break;
            case GunType.Machinegun:
                m_animator.Play("RifleFire", 1, 0);
                break;
            case GunType.Pistol:
                m_animator.Play("PistolFire", 1, 0);
                break;
            case GunType.Launcher:
                m_animator.Play("LauncherFire", 1, 0);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HitPlayer()
    {
        if (m_animator != null)
        {
            HitType = !HitType;
            int ht = (HitType) ? 1 : 0;
            m_animator.SetInteger("HitType", ht);
            m_animator.SetTrigger("Hit");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateStates()
    {
        ControllerInfo();
        Animate();
        UpperControll();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetNetworkWeapon(GunType weaponType, bl_NetworkGun networkGun)
    {
        cacheWeaponType = weaponType;
        CurrentNetworkGun = networkGun;
        m_animator?.SetInteger("GunType", (int)weaponType);
        m_animator.Play("Equip", 1, 0);
        isWeaponsBlocked = false;
        if (CurrentNetworkGun == null || CurrentNetworkGun.LocalGun == null)
        {
            reloadSpeed = 1;
        }
        stopHandsIK = true;
        CancelInvoke();
        Invoke(nameof(ResetHandsIK), 0.3f);
    }

    void ResetHandsIK() { stopHandsIK = false; }

    public GunType GetCurretWeaponType() { return cacheWeaponType; }
}