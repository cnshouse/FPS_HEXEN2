using UnityEngine;
using System.Collections;
using MFPS.PlayerController;
using UnityEngine.Serialization;
using MFPS.Runtime.Level;

[RequireComponent(typeof(CharacterController))]
public class bl_FirstPersonController : bl_MonoBehaviour
{
    #region Public members
    [Header("Settings")]
    public PlayerState State;
    public float WalkSpeed = 4.5f;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float runSpeed = 8;
    [FormerlySerializedAs("m_CrouchSpeed")]
    public float crouchSpeed = 2;
    public float slideSpeed = 10;
    [FormerlySerializedAs("m_ClimbSpeed")]
    public float climbSpeed = 1f;
    [FormerlySerializedAs("m_JumpSpeed")]
    public float jumpSpeed;
    public float slopeFriction = 3f;
    public float crouchTransitionSpeed = 0.25f;
    [Range(0.2f, 1.5f)] public float slideTime = 0.75f;
    [Range(1, 12)] public float slideFriction = 10;
    [Range(0, 2)] public float JumpMinRate = 0.82f;
    [Range(0, 2)] public float AirControlMultiplier = 0.8f;
    public float m_StickToGroundForce;
    public float m_GravityMultiplier;
    [LovattoToogle] public bool RunFovEffect = true;
    public float runFOVAmount = 8;
    [LovattoToogle] public bool KeepToCrouch = true;
    [Header("Falling")]
    [LovattoToogle] public bool FallDamage = true;
    [Range(0.1f, 5f)]
    public float SafeFallDistance = 3;
    [Range(3, 25)]
    public float DeathFallDistance = 15;

    [Header("Dropping")]
    public float dropControlSpeed = 25;
    public Vector2 dropTiltSpeedRange = new Vector2(20, 60);
    [Header("Mouse Look"), FormerlySerializedAs("m_MouseLook")]
    public MouseLook mouseLook;
    [FormerlySerializedAs("HeatRoot")]
    public Transform headRoot;
    public Transform CameraRoot;
    [Header("HeadBob")]
    [Range(0, 1.2f)] public float headBobMagnitude = 0.9f;
    public LerpControlledBob m_JumpBob = new LerpControlledBob();
    [Header("FootSteps")]
    public bl_Footstep footstep;
    public AudioClip jumpSound;           // the sound played when character leaves the ground.
    public AudioClip landSound;           // the sound played when character touches back on ground.
    public AudioClip slideSound;

    [Header("UI")]
    public Sprite StandIcon;
    public Sprite CrouchIcon;
    #endregion

    #region Public properties
    public float RunFov { get; set; }
    public CollisionFlags m_CollisionFlags { get; set; }
    public Vector3 Velocity { get; set; }
    public float VelocityMagnitude { get; set; }
    public bool isControlable { get; set; } = true;
    #endregion

    #region Private members
    private bool hasPlatformJump = false;
    private float PlatformJumpForce = 0;
    private bool m_Jump;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private bool m_PreviouslyGrounded;
    private bool m_Jumping;
    private bool Crounching = false;
    private AudioSource m_AudioSource;
    private bool Finish = false;
    private Vector3 defaultCameraRPosition;
    private bool isClimbing = false;
    private bl_Ladder m_Ladder;
    private bool MoveToStarted = false;
#if MFPSM
    private bl_Joystick Joystick;
#endif
    private float PostGroundVerticalPos = 0;
    private bool isFalling = false;
    private int JumpDirection = 0;
    private float HigherPointOnJump;
    private CharacterController m_CharacterController;
    private float lastJumpTime = 0;
    private float WeaponWeight = 1;
    private bool hasTouchGround = false;
    private bool JumpInmune = false;
    private Transform m_Transform;
    private RaycastHit[] SurfaceRay = new RaycastHit[1];
    private Vector3 desiredMove = Vector3.zero;
    private float VerticalInput, HorizontalInput;
    private bool lastCrouchState = false;
    private float fallingTime = 0;
    private bool haslanding = false;
    private float capsuleRadious;
    private readonly Vector3 feetPositionOffset = new Vector3(0, 0.8f, 0);
    private float slideForce = 0;
    private float lastSlideTime = 0;
    private bl_PlayerReferences playerReferences;
    private Vector3 vectorForward = Vector3.forward;
    private PlayerState lastState = PlayerState.Idle;
    private bool forcedCrouch = false;
    private Vector3 surfaceNormal = Vector3.zero;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!photonView.IsMine)
            return;

        base.Awake();
        m_Transform = transform;
        playerReferences = GetComponent<bl_PlayerReferences>();
        m_CharacterController = playerReferences.characterController;
#if MFPSM
        Joystick = FindObjectOfType<bl_Joystick>();
#endif
        defaultCameraRPosition = CameraRoot.localPosition;
        m_Jumping = false;
        m_AudioSource = gameObject.AddComponent<AudioSource>();
        mouseLook.Init(m_Transform, headRoot, playerReferences.gunManager);
        lastJumpTime = Time.time;
        RunFov = 0;
        capsuleRadious = m_CharacterController.radius * 0.1f;
        isControlable = bl_MatchTimeManager.Instance.TimeState == RoomTimeState.Started;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        bl_EventHandler.onRoundEnd += OnRoundEnd;
        bl_EventHandler.onChangeWeapon += OnChangeWeapon;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
        bl_EventHandler.onLocalAimChanged += OnAimChange;
#if MFPSM
        bl_TouchHelper.OnCrouch += OnCrouchClicked;
        bl_TouchHelper.OnJump += OnJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        bl_EventHandler.onRoundEnd -= OnRoundEnd;
        bl_EventHandler.onChangeWeapon -= OnChangeWeapon;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
        bl_EventHandler.onLocalAimChanged -= OnAimChange;
#if MFPSM
        bl_TouchHelper.OnCrouch -= OnCrouchClicked;
        bl_TouchHelper.OnJump -= OnJump;
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        Velocity = m_CharacterController.velocity;
        VelocityMagnitude = Velocity.magnitude;
        RotateView();

        if (Finish)
            return;

        MovementInput();
        GroundDetection();
        CheckStates();
    }

    /// <summary>
    /// Triggered when the state of this player controller has changed.
    /// </summary>
    private void OnStateChanged(PlayerState from, PlayerState to)
    {
        if (from == PlayerState.Crouching || to == PlayerState.Crouching)
        {
            DoCrouchTransition();
        }
        else if (from == PlayerState.Sliding || to == PlayerState.Sliding)
        {
            DoCrouchTransition();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void MovementInput()
    {
        if (State == PlayerState.Sliding)
        {
            slideForce -= Time.deltaTime * slideFriction;
            speed = slideForce;
            if (bl_GameInput.Jump())
            {
                State = PlayerState.Jumping;
                m_Jump = true;
            }
            else return;
        }

        if (bl_UtilityHelper.isMobile) return;

        if (!m_Jump && State != PlayerState.Crouching && (Time.time - lastJumpTime) > JumpMinRate)
        {
            m_Jump = bl_GameInput.Jump();
        }

        if (State != PlayerState.Jumping && State != PlayerState.Climbing)
        {
            if (forcedCrouch) return;
            if (KeepToCrouch)
            {
                Crounching = bl_GameInput.Crouch();
                if (Crounching != lastCrouchState)
                {
                    OnCrouchChanged();
                    lastCrouchState = Crounching;
                }
            }
            else
            {
                if (bl_GameInput.Crouch(GameInputType.Down))
                {
                    Crounching = !Crounching;
                    OnCrouchChanged();
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GroundDetection()
    {
        //if the player has touch the ground after falling
        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            OnLand();
        }
        else if (m_PreviouslyGrounded && !m_CharacterController.isGrounded)//when the player start jumping
        {
            if (!isFalling)
            {
                PostGroundVerticalPos = m_Transform.position.y;
                isFalling = true;
                fallingTime = Time.time;
            }
        }

        if (isFalling)
        {
            FallingCalculation();
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        if (forcedCrouch)
        {
            if ((Time.frameCount % 10) == 0)
            {
                if (!IsHeadHampered())
                {
                    forcedCrouch = false;
                    State = PlayerState.Idle;
                }
            }
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnFixedUpdate()
    {
        if (Finish)
            return;
        if (m_CharacterController == null || !m_CharacterController.enabled || MoveToStarted)
            return;

        //if player focus is in game
        if (bl_RoomMenu.Instance.isCursorLocked && !bl_GameData.Instance.isChating)
        {
            //determine the player speed
            float s = 0;
            GetInput(out s);
            speed = s;
        }
        else if (State != PlayerState.Sliding)//if player is not focus in game
        {
            m_Input = Vector2.zero;
        }

        if (isClimbing && m_Ladder != null)
        {
            //climbing control
            OnClimbing();
        }
        else
        {
            //player movement
            Move();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Move()
    {
        //if the player is touching the surface
        if (m_CharacterController.isGrounded)
        {
            OnSurface();
            //vertical resistance
            m_MoveDir.y = -m_StickToGroundForce;
            hasTouchGround = true;
            //has a pending jump
            if (m_Jump || hasPlatformJump)
            {
                DoJump();
            }
        }
        else//if the player is not touching the ground
        {
            //if the player is dropping
            if (State == PlayerState.Dropping)
            {
                //handle the air movement in different process
                OnDropping();
                return;
            }
            else if (State == PlayerState.Gliding)
            {
                //handle the gliding movement in different process
                OnGliding();
                return;
            }

            OnAir();
        }
        //apply the movement direction in the character controller
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Control the player when is in a surface
    /// </summary>
    void OnSurface()
    {
        // always move along the camera forward as it is the direction that it being aimed at
        desiredMove = (m_Transform.forward * m_Input.y) + (m_Transform.right * m_Input.x);

        // get a normal for the surface that is being touched to move along it
        Physics.SphereCastNonAlloc(m_Transform.position, capsuleRadious, Vector3.down, SurfaceRay, m_CharacterController.height * 0.5f, Physics.AllLayers, QueryTriggerInteraction.Ignore);

        //determine the movement angle based in the normal of the current player surface
        desiredMove = Vector3.ProjectOnPlane(desiredMove, SurfaceRay[0].normal);
        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;

        SlopeControl();
    }

    /// <summary>
    /// Control the player when is in air (not dropping nor gliding)
    /// </summary>
    void OnAir()
    {
        //how much can the player control the player when is in air.
        float airControlMult = AirControlMultiplier;
        //fall gravity amount
        float gravity = m_GravityMultiplier;

        m_MoveDir += Physics.gravity * gravity * Time.fixedDeltaTime;
        m_MoveDir.x = (desiredMove.x * speed) * airControlMult;
        m_MoveDir.z = (desiredMove.z * speed) * airControlMult;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLand()
    {
        //land camera effect
        StartCoroutine(m_JumpBob.DoBobCycle());

        isFalling = false;
        if (FallDamage && hasTouchGround && haslanding)
        {
            CalculateFall();
        }
        else { PlayLandingSound(1); }
        haslanding = true;
        JumpDirection = 0;
        m_MoveDir.y = 0f;
        m_Jumping = false;
        if (State != PlayerState.Crouching)
            State = PlayerState.Idle;

        bl_EventHandler.DispatchPlayerLandEvent();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCrouchChanged()
    {
        if (Crounching)
        {
            State = PlayerState.Crouching;
            bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = CrouchIcon;

            //Slide implementation
            if (VelocityMagnitude > WalkSpeed)
            {
                DoSlide();
            }
        }
        else
        {
            if (!IsHeadHampered())
            {
                State = PlayerState.Idle;
                bl_UIReferences.Instance.PlayerUI.PlayerStateIcon.sprite = StandIcon;
            }
            else forcedCrouch = true;
        }
        bl_UCrosshair.Instance.OnCrouch(Crounching);
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoCrouchTransition()
    {
        StopCoroutine(nameof(CrouchTransition));
        StartCoroutine(nameof(CrouchTransition));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator CrouchTransition()
    {
        bool isCrouch = Crounching || State == PlayerState.Sliding;
        float height = isCrouch ? 1.4f : 2f;
        Vector3 center = isCrouch ? new Vector3(0, -0.3f, 0) : Vector3.zero;
        Vector3 cameraPosition = CameraRoot.localPosition;
        Vector3 verticalCameraPos = isCrouch ? new Vector3(cameraPosition.x, 0.2f, cameraPosition.z) : defaultCameraRPosition;

        float originHeight = m_CharacterController.height;
        Vector3 originCenter = m_CharacterController.center;
        Vector3 originCameraPosition = cameraPosition;

        float d = 0;
        while (d < 1)
        {
            d += Time.deltaTime / crouchTransitionSpeed;
            m_CharacterController.height = Mathf.Lerp(originHeight, height, d);
            m_CharacterController.center = Vector3.Lerp(originCenter, center, d);
            CameraRoot.localPosition = Vector3.Lerp(originCameraPosition, verticalCameraPos, d);
            yield return null;
        }
    }

    /// <summary>
    /// Make the player jump
    /// </summary>
    void DoJump()
    {
        m_MoveDir.y = (hasPlatformJump) ? PlatformJumpForce : jumpSpeed;
        PlayJumpSound();
        m_Jump = false;
        m_Jumping = true;
        hasPlatformJump = false;
        State = PlayerState.Jumping;
        lastJumpTime = Time.time;
    }

    /// <summary>
    /// Make the player slide
    /// </summary>
    void DoSlide()
    {
        if ((Time.time - lastSlideTime) < slideTime * 1.2f) return;//wait the equivalent of one extra slide before be able to slide again
        if (m_Jumping) return;
        Vector3 startPosition = (m_Transform.position - feetPositionOffset) + (m_Transform.forward * m_CharacterController.radius);
        if (Physics.Linecast(startPosition, startPosition + m_Transform.forward)) return;//there is something in front of the feet's

        State = PlayerState.Sliding;
        slideForce = slideSpeed;//slide force will be continually decreasing
        speed = slideSpeed;
        playerReferences.gunManager.HeadAnimator.Play("slide-start", 0, 0);
        if (slideSound != null)
        {
            m_AudioSource.clip = slideSound;
            m_AudioSource.volume = 0.7f;
            m_AudioSource.Play();
        }
        mouseLook.UseOnlyCameraRotation();
        this.InvokeAfter(slideTime, () =>
        {
            if (Crounching)
                State = PlayerState.Crouching;
            else if (State != PlayerState.Jumping)
                State = PlayerState.Idle;

            Crounching = false;
            lastSlideTime = Time.time;
            mouseLook.PortBodyOrientationToCamera();
        });
    }

    /// <summary>
    /// Detect slope limit and apply slide physics.
    /// </summary>
    void SlopeControl()
    {
        float angle = Vector3.Angle(Vector3.up, surfaceNormal);

        if (angle <= m_CharacterController.slopeLimit || angle >= 75) return;

        m_MoveDir.x += ((1f - surfaceNormal.y) * surfaceNormal.x) * (-Physics.gravity.y - slopeFriction);
        m_MoveDir.z += ((1f - surfaceNormal.y) * surfaceNormal.z) * (-Physics.gravity.y - slopeFriction);
    }

    /// <summary>
    /// Make the player dropping
    /// </summary>
    public void DoDrop()
    {
        if (isGrounded)
        {
            Debug.Log("Can't drop when player is in a surface");
            return;
        }
        State = PlayerState.Dropping;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDropping()
    {
        //get the camera upside down angle
        float tilt = Mathf.InverseLerp(0, 90, mouseLook.VerticalAngle);
        //normalize it
        tilt = Mathf.Clamp01(tilt);
        if (mouseLook.VerticalAngle <= 0 || mouseLook.VerticalAngle >= 180) tilt = 0;
        //get the forward direction of the player camera
        desiredMove = headRoot.forward * Mathf.Clamp01(m_Input.y);
        if (desiredMove.y > 0) desiredMove.y = 0;

        //calculate the drop speed based in the upside down camera angle
        float dropSpeed = Mathf.Lerp(m_GravityMultiplier * dropTiltSpeedRange.x, m_GravityMultiplier * dropTiltSpeedRange.y, tilt);
        m_MoveDir = Physics.gravity * dropSpeed * Time.fixedDeltaTime;
        //if the player press the vertical input -> add velocity in the direction where the camera is looking at
        m_MoveDir += desiredMove * dropControlSpeed;

        //apply the movement direction in the character controller
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Make the player glide
    /// </summary>
    public void DoGliding()
    {
        if (isGrounded)
        {
            Debug.Log("Can't gliding when player is in a surface");
            return;
        }
        State = PlayerState.Gliding;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnGliding()
    {
        desiredMove = (m_Transform.forward * m_Input.y) + (m_Transform.right * m_Input.x);
        //how much can the player control the player when is in air.
        float airControlMult = AirControlMultiplier * 5;
        //fall gravity amount
        float gravity = m_GravityMultiplier * 15;

        m_MoveDir = Physics.gravity * gravity * Time.fixedDeltaTime;
        m_MoveDir.x = (desiredMove.x * speed) * airControlMult;
        m_MoveDir.z = (desiredMove.z * speed) * airControlMult;

        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnClimbing()
    {
        if (m_Ladder.HasPending)
        {
            if (!MoveToStarted)
            {
                StartCoroutine(MoveTo(m_Ladder.GetCurrent, false));
            }
        }
        else
        {
            desiredMove = m_Ladder.transform.rotation * vectorForward * m_Input.y;
            m_MoveDir.y = desiredMove.y * climbSpeed;
            m_MoveDir.x = desiredMove.x * climbSpeed;
            m_MoveDir.z = desiredMove.z * climbSpeed;
            if (bl_GameInput.Jump())
            {
                ToggleClimbing();
                m_Ladder.JumpOut();
                m_MoveDir.y = jumpSpeed;
                m_MoveDir.z = 30;
                lastJumpTime = Time.time;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CalculateFall()
    {
        if (JumpInmune) { JumpInmune = false; return; }
        if ((Time.time - fallingTime) <= 0.4f) return;

        float ver = HigherPointOnJump - m_Transform.position.y;
        if (JumpDirection == -1)
        {
            // float normalized = m_Transform.position.y + Mathf.Abs(PostGroundVerticalPos);
            float normalized = PostGroundVerticalPos - m_Transform.position.y;
            ver = Mathf.Abs(normalized);
        }
        if (ver > SafeFallDistance)
        {
            int damage = Mathf.FloorToInt((ver / DeathFallDistance) * 100);
            playerReferences.playerHealthManager.GetFallDamage(damage);
        }
        PlayLandingSound((ver / DeathFallDistance));
        fallingTime = Time.time;
    }

    /// <summary>
    /// 
    /// </summary>
    void FallingCalculation()
    {
        if (m_Transform.position.y == PostGroundVerticalPos) return;

        //if the direction has not been decided yet
        if (JumpDirection == 0)
        {
            //is the player below or above from the surface he was?
            // 1 = above (jump), -1 = below (falling)
            JumpDirection = (m_Transform.position.y > PostGroundVerticalPos) ? 1 : -1;
        }
        else if (JumpDirection == 1)//if the player jump
        {
            //but not start falling
            if (m_Transform.position.y < PostGroundVerticalPos)
            {
                //get the higher point he reached jumping
                HigherPointOnJump = PostGroundVerticalPos;
            }
            else//if still going up
            {
                PostGroundVerticalPos = m_Transform.position.y;
            }
        }
        else//if the player was falling without jumping
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void GetInput(out float speed)
    {
        if (!isControlable) { speed = 0; return; }

        // Read input
        HorizontalInput = bl_GameInput.Horizontal;
        VerticalInput = bl_GameInput.Vertical;

#if MFPSM
        if (bl_UtilityHelper.isMobile)
        {
            HorizontalInput = Joystick.Horizontal;
            VerticalInput = Joystick.Vertical;
            VerticalInput = VerticalInput * 1.25f;
        }
#endif
        if (State == PlayerState.Sliding)
        {
            VerticalInput = 1;
            HorizontalInput = 0;
        }

        m_Input = new Vector2(HorizontalInput, VerticalInput);
        //if the player is dropping, the speed is calculated in the dropping function
        if (State == PlayerState.Dropping || State == PlayerState.Gliding) { speed = 0; return; }

        if (State != PlayerState.Climbing && State != PlayerState.Sliding)
        {
            if (m_Input.sqrMagnitude > 0)
            {
                if (!bl_UtilityHelper.isMobile)
                {
                    // On standalone builds, walk/run speed is modified by a key press.
                    // keep track of whether or not the character is walking or running
                    if (bl_GameInput.Run() && State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Running;
                    }
                    else if (bl_GameInput.Run(GameInputType.Up) && State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && VerticalInput > 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }

                }
                else
                {
                    if (VerticalInput > 1 && VerticalInput > 0.05f && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Running;
                    }
                    else if (VerticalInput <= 1 && VerticalInput != 0 && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Crouching && VerticalInput != 0)
                    {
                        State = PlayerState.Walking;
                    }
                    else if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                    {
                        State = PlayerState.Idle;
                    }
                }
            }
            else if (m_CharacterController.isGrounded)
            {
                if (State != PlayerState.Jumping && State != PlayerState.Crouching)
                {
                    State = PlayerState.Idle;
                }
            }
        }

        if (Crounching)
        {
            speed = (State == PlayerState.Crouching) ? crouchSpeed : runSpeed;
        }
        else
        {
            // set the desired speed to be walking or running
            speed = (State == PlayerState.Running) ? runSpeed : WalkSpeed;
        }
        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }
        if (RunFovEffect)
        {
            float rf = State == PlayerState.Running ? runFOVAmount : 0;
            RunFov = Mathf.Lerp(RunFov, rf, Time.deltaTime * 6);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayFootStepAudio(bool b)
    {
        if (State == PlayerState.Sliding) return;
        if (!m_CharacterController.isGrounded && !isClimbing)
            return;

        if (!isClimbing)
        {
            footstep?.DetectAndPlaySurface();
        }
        else
        {
            footstep?.PlayStepForTag("Generic");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlatformJump(float force)
    {
        hasPlatformJump = true;
        PlatformJumpForce = force;
        JumpInmune = true;
    }

#if MFPSM
    /// <summary>
    /// 
    /// </summary>
    void OnCrouchClicked()
    {
        Crounching = !Crounching;
        OnCrouchChanged();
    }

    void OnJump()
    {
        if (!m_Jump && State != PlayerState.Crouching)
        {
            m_Jump = true;
        }
    }
#endif

    public void OnTeleport(Vector3 TelePosition, Quaternion TeleRotation)
    {
        Debug.Log("Teleport the player to " + TelePosition + " with a rotation of " + TeleRotation);
        TelePosition.y = TelePosition.y + 1;
        transform.position = TelePosition;
        transform.rotation = TeleRotation;
    }

    /// <summary>
    /// 
    /// </summary>
    public void RotateView()
    {
        if (!isClimbing)
        {
            mouseLook.LookRotation(m_Transform, headRoot);
        }
        else
        {
            mouseLook.LookRotation(m_Transform, headRoot, m_Ladder.InsertionPoint);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void CheckStates()
    {
        if (lastState == State) return;
        OnStateChanged(lastState, State);
        lastState = State;
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayLandingSound(float vol = 1)
    {
        vol = Mathf.Clamp(vol, 0.05f, 1);
        m_AudioSource.clip = landSound;
        m_AudioSource.volume = vol;
        m_AudioSource.Play();
    }

    /// <summary>
    /// 
    /// </summary>
    private void PlayJumpSound()
    {
        m_AudioSource.volume = 1;
        m_AudioSource.clip = jumpSound;
        m_AudioSource.Play();
    }

    void OnChangeWeapon(int id)
    {
        WeaponWeight = bl_GameData.Instance.GetWeapon(id).Weight;
    }
    void OnMatchStart() { isControlable = true; }
    void OnGameSettingsChanged() => mouseLook.FetchSettings();
    void OnAimChange(bool aim) => mouseLook.OnAimChange(aim);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsHeadHampered()
    {
        Vector3 origin = m_Transform.localPosition + m_CharacterController.center + Vector3.up * m_CharacterController.height * 0.5F;
        float dist = 2.05f - m_CharacterController.height;
        return Physics.Raycast(origin, Vector3.up, dist);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        Finish = true;
    }

    private void ToggleClimbing()
    {
        isClimbing = !isClimbing;
        State = (isClimbing) ? PlayerState.Climbing : PlayerState.Idle;
        bl_UIReferences.Instance.JumpLadder.SetActive(isClimbing);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator MoveTo(Vector3 pos, bool down)
    {
        MoveToStarted = true;
        float t = 0;
        Vector3 from = m_Transform.position;
        while (t < 1)
        {
            t += Time.deltaTime / 0.4f;
            m_Transform.position = Vector3.Lerp(from, pos, t);
            yield return null;
        }
        if (down) { bl_EventHandler.onPlayerLand(); }
        if (m_Ladder != null)
        {
            m_Ladder.HasPending = false;
        }
        MoveToStarted = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent == null)
            return;

        bl_Ladder l = other.transform.parent.GetComponent<bl_Ladder>();
        if (l != null)
        {
            if (!l.CanUse)
                return;

            m_Ladder = l;
            if (other.transform.name == bl_Ladder.BottomColName)
            {
                m_Ladder.InsertionPoint = other.transform;
                if (!isClimbing)
                {
                    JumpInmune = true;
                    m_Ladder.ToBottom();
                    ToggleClimbing();
                }
                else
                {
                    ToggleClimbing();
                    m_Ladder.HasPending = false;
                }
            }
            else if (other.transform.name == bl_Ladder.TopColName)
            {
                m_Ladder.InsertionPoint = other.transform;
                if (isClimbing)
                {
                    m_Ladder.SetToTop();
                    if (!MoveToStarted)
                    {
                        StartCoroutine(MoveTo(m_Ladder.GetCurrent, true));
                    }
                }
                else
                {
                    m_Ladder.ToMiddle();
                }
                ToggleClimbing();
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        surfaceNormal = hit.normal;
        /// Enable this if you want player controller apply force on contact to rigidbodys
        /// is commented by default for performance matters.
        /* Rigidbody body = hit.collider.attachedRigidbody;
         //dont move the rigidbody if the character is on top of it
         if (m_CollisionFlags == CollisionFlags.Below)
         {
             return;
         }

         if (body == null || body.isKinematic)
         {
             return;
         }
         body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);*/
    }

    internal float _speed = 0;
    public float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = value - WeaponWeight;
            _speed = Mathf.Clamp(_speed, 2, 12);
        }
    }

    public Vector3 MovementDirection => m_MoveDir;
    public bool isGrounded { get { return m_CharacterController.isGrounded; } }

#if UNITY_EDITOR
    [HideInInspector] public bool _movementExpand = false;
    [HideInInspector] public bool _jumpExpand = false;
    [HideInInspector] public bool _fallExpand = false;
    [HideInInspector] public bool _mouseExpand = false;
    [HideInInspector] public bool _bobExpand = false;
    [HideInInspector] public bool _soundExpand = false;
    [HideInInspector] public bool _miscExpand = false;
#endif
}