using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;
using NetHashTable = ExitGames.Client.Photon.Hashtable;
using MFPS.Runtime.AI;
using MFPSEditor;

[RequireComponent(typeof(NavMeshAgent))]
public class bl_AIShooterAgent : bl_AIShooter
{
    #region Public Members
    [Space(5)]
    [ScriptableDrawer] public bl_AIBehaviorSettings behaviorSettings;
    [ScriptableDrawer] public bl_AISoldierSettings soldierSettings;


    [Header("AutoTargets")]
    public float updatePlayersEach = 5f;

    [Header("Others")]
    public LayerMask ObstaclesLayer;
    public bool DebugStates = false;

    [Header("References")]
    public Transform aimTarget;
    #endregion

    #region Public Properties
    public override Transform AimTarget
    {
        get => aimTarget;
    }
    public bl_Footstep footstep;
    public NavMeshAgent Agent { get; set; }
    public bool personal { get; set; }
    public bool playerInFront { get; set; }
    public bool ObstacleBetweenTarget { get; set; }
    public bl_AIShooterWeapon AIWeapon { get; set; }
    public bl_AIShooterHealth AIHealth { get; set; }
    public float CachedTargetDistance { get; set; } = 0;
    public string DebugLine { get; set; }//last ID 34
    public List<Transform> PlayersInRoom { get; set; } = new List<Transform>();//All Players in room
    public AILookAt LookingAt { get; private set; } = AILookAt.Path;
    public bool IsCrouch { get; set; } = false;
    #endregion

    #region Private members
    private Animator Anim;
    private Vector3 finalPosition;
    private float lastPathTime = 0;
    private bl_AIAnimation AIAnim;
    private bl_AICovertPointManager CoverManager;
    private bl_AIMananger AIManager;
    private bl_AICoverPoint CoverPoint = null;
    private bool ForceCoverFire = false;
    private float CoverTime = 0;
    private Vector3 LastHitDirection;
    private int SwitchCoverTimes = 0;
    private float lookTime = 0;
    private bool randomOnStartTake = false;
    private bool AllOrNothing = false;
    private bl_MatchTimeManager TimeManager;
    private bl_NamePlateDrawer DrawName;
    private GameMode m_GameMode;
    private float time = 0;
    private float delta = 0;
    private Transform m_Transform;
    private float nextEnemysCheck = 0;
    private bool isGameStarted = false;
    private Vector3 targetDirection = Vector3.zero;
    RaycastHit obsRay;
    private float lastDestinationTime = 0;
    private float velocityMagnitud = 0;
    private bool forceUpdateRotation = false;
    private Vector3 localRotation = Vector3.zero;
    private int[] animationHash = new int[] { 0 };
    private bool wasTargetInSight = false;
    public const string RPC_NAME = "RPCShooterBot";
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        Agent = References.Agent;
        AIAnim = References.aiAnimation;
        AIHealth = References.shooterHealth;
        AIWeapon = References.shooterWeapon;
        Anim = GetComponentInChildren<Animator>();
        ObstacleBetweenTarget = false;
        AIManager = bl_AIMananger.Instance;
        CoverManager = AIManager.GetComponent<bl_AICovertPointManager>();
        TimeManager = bl_MatchTimeManager.Instance;
        DrawName = References.namePlateDrawer;
        m_GameMode = GetGameMode;
        Agent.updateRotation = false;
        animationHash[0] = Animator.StringToHash("Crouch");
    }

    /// <summary>
    /// 
    /// </summary>
    public override void Init()
    {
        isGameStarted = TimeManager.TimeState == RoomTimeState.Started;
        InvokeRepeating(nameof(UpdateList), 0, updatePlayersEach);
        References.shooterNetwork.CheckNamePlate();
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        time = Time.time;
        delta = Time.deltaTime;
        if (isDeath) return;
        isNewDebug = true;

        if (!isGameStarted) return;
        if (Target != null)
        {
            if (AgentState != AIAgentState.Covering)
            {
                TargetControll();
            }
            else
            {
                OnCovering();
            }
        }
        LookAtControl();
    }

    /// <summary>
    /// this is called one time each second instead of each frame
    /// </summary>
    public override void OnSlowUpdate()
    {
        if (isDeath) return;
        if (TimeManager.isFinish || !isGameStarted)
        {
            return;
        }

        velocityMagnitud = Agent.velocity.magnitude;

        if (!HasATarget)
        {
            SetDebugState(-1, true);
            //Get the player nearest player
            SearchPlayers();
            //if target null yet, then patrol         
            RandomPatrol(!isOneTeamMode);
        }
        else
        {
            CheckEnemysDistances();
            CheckVision();
        }
        FootStep();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnAgentStateChanged(AIAgentState from, AIAgentState to)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    private void OnTargetChanged(Transform from, Transform to)
    {

    }

    /// <summary>
    /// Called when the bot not direct vision to the target -> have direct vision to the target and vice versa
    /// </summary>
    /// <param name="from">seeing?</param>
    /// <param name="to">seeing?</param>
    private void OnTargetLineOfSightChanged(bool from, bool to)
    {
        if(from == true)//the player lost the line of vision with the target
        {
            if(HasATarget && TargetDistance > 5)//he lost the target but not cuz it is death.
            Invoke(nameof(CorrectLookAt), 3);//if after 3 second of loss the target, still now found it -> don't look at it (trough walls for example)
        }
        else//player now has direct vision to the target
        {
            if(AgentState == AIAgentState.Following)
            CancelInvoke(nameof(CorrectLookAt));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CorrectLookAt()
    {
        SetLookAtState(AILookAt.PathToTarget);
    }

    /// <summary>
    /// 
    /// </summary>
    void TargetControll()
    {
        CachedTargetDistance = bl_UtilityHelper.Distance(Target.position, m_Transform.localPosition);
        if (CachedTargetDistance >= soldierSettings.limitRange)
        {
            WhenTargetOutOfRange();
        }
        else if (CachedTargetDistance > soldierSettings.closeRange && CachedTargetDistance < soldierSettings.mediumRange)
        {
            WhenTargetOnMediumRange();
        }
        else if (CachedTargetDistance <= soldierSettings.closeRange)
        {
            WhenTargetOnCloseRange();
        }
        else if (CachedTargetDistance < soldierSettings.limitRange)
        {
            WhenTargetOnLimitRange();
        }
        else
        {
            Debug.Log("Unknown state: " + CachedTargetDistance);
            SetDebugState(101, true);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOutOfRange()
    {
        if (behaviorSettings.targetOutRangeBehave == AITargetOutRangeBehave.SearchNewNearestTarget)
        {
            var newTarget = GetNearestPlayer;
            if (newTarget != Target)
            {
                SetTarget(newTarget);
                return;
            }
        }

        if (AgentState == AIAgentState.Following || personal)
        {
            if (!isOneTeamMode)
            {
                //update the target position each 300 frames
                if (!Agent.hasPath || (Time.frameCount % 300) == 0)
                    if (bl_UtilityHelper.Distance(TargetPosition, Agent.destination) > 1)//update the path only if the target has moved substantially
                        SetDestination(TargetPosition, 3);
            }
            else
            {
                //in one team mode, when the target is in the limit range
                //the bot will start to random patrol instead of following the player.
                RandomPatrol(true);
            }
            SetDebugState(0, true);
        }
        else if (AgentState == AIAgentState.Searching)
        {
            SetDebugState(1, true);
            RandomPatrol(true);
        }
        else
        {
            SetDebugState(2, true);
            SetTarget(null);
            RandomPatrol(false);
            SetState(AIAgentState.Patroling);
        }
        Speed = soldierSettings.walkSpeed;
        if (!AIWeapon.isFiring)
        {
            Anim.SetInteger("UpperState", 4);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnMediumRange()
    {
        SetDebugState(3, true);
        OnTargeContest(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnCloseRange()
    {
        SetDebugState(4, true);
        Follow();
    }

    /// <summary>
    /// 
    /// </summary>
    void WhenTargetOnLimitRange()
    {
        SetDebugState(5, true);
        OnTargeContest(true);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnCovering()
    {
        if (Target != null)
        {
            CachedTargetDistance = TargetDistance;
            if (CachedTargetDistance <= soldierSettings.mediumRange && playerInFront)//if in look range and in front, start follow him and shot
            {
                if (behaviorSettings.agentBehave == AIAgentBehave.Agressive)
                {
                    SetDebugState(6, true);
                    if (!Agent.hasPath)
                    {
                        SetState(AIAgentState.Following);
                        SetDestination(TargetPosition, 3);
                    }
                }
                else //to covert point and looking to it
                {
                    SetDebugState(7, true);
                    SetState(AIAgentState.Covering);
                    if (!Agent.hasPath)
                    {
                        Cover(false);
                    }
                }
                TriggerFire();
            }
            else if (CachedTargetDistance > soldierSettings.limitRange && CanCover(7))// if out of line of sight, start searching him
            {
                SetDebugState(8, true);
                SetState(AIAgentState.Searching);
                SetCrouch(false);
                TriggerFire( bl_AIShooterWeapon.FireReason.OnMove);
            }
            else if (ForceCoverFire && !ObstacleBetweenTarget)//if bot is cover and still get damage, start shoot at the target (panic)
            {
                SetDebugState(9, true);
                TriggerFire(bl_AIShooterWeapon.FireReason.Forced);
                if (CanCover(behaviorSettings.maxCoverTime)) { SwitchCover(); }
            }
            else if (CanCover(behaviorSettings.maxCoverTime) && CachedTargetDistance >= 7)//if has been a time since cover and nothing happen, try a new spot.
            {
                SetDebugState(10, true);
                SwitchCover();
                TriggerFire(bl_AIShooterWeapon.FireReason.OnMove);
            }
            else
            {
                if (playerInFront)
                {
                    Speed = soldierSettings.walkSpeed;
                    TriggerFire(bl_AIShooterWeapon.FireReason.Forced);
                    SetDebugState(11, true);
                }
                else
                {
                    SetDebugState(12, true);
                    Speed = soldierSettings.runSpeed;
                    CheckConfrontation();
                    TriggerFire(bl_AIShooterWeapon.FireReason.OnMove);
                    SetCrouch(false);
                }
            }
        }
        if (Agent.pathStatus == NavMeshPathStatus.PathComplete)//once the bot reach the target cover point
        {
            if (CoverPoint != null && CoverPoint.Crouch) { SetCrouch(true); }//and the point required crouch -> do crouch
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool Cover(bool overridePoint, AIAgentCoverArea coverArea = AIAgentCoverArea.ToPoint)
    {
        //if the target if far, there's not point in cover right now
        if (behaviorSettings.agentBehave == AIAgentBehave.Agressive && CachedTargetDistance > 20)
        {
            SetState(AIAgentState.Following);
            return false;
        }
        Transform t = transform;
        switch (coverArea)
        {
            case AIAgentCoverArea.ToTarget:
                t = Target;//find a point near the target
                break;
        }
        if (overridePoint)//override the current cover point
        {
            //if the agent has complete his current destination
            if (Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                //get a random point in 30 metters
                if (coverArea == AIAgentCoverArea.ToRandomPoint)
                {
                    //look for another random cover point 
                    CoverPoint = CoverManager.GetCoverOnRadius(t, 30);
                }
                else
                {
                    //Get the nearest cover point
                    CoverPoint = CoverManager.GetCloseCover(t, CoverPoint);
                }
            }
            SetDebugState(13);
        }
        else
        {
            SetDebugState(14);
            //look for a near cover point
            CoverPoint = CoverManager.GetCloseCover(t);
        }
        if (CoverPoint != null)//if a point was found
        {
            SetDebugState(15);
            Speed = playerInFront ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
            SetDestination(CoverPoint.transform.position, 0.1f);
            SetState(AIAgentState.Covering);
            CoverTime = time;
            TriggerFire(behaviorSettings.agentBehave == AIAgentBehave.Agressive ? bl_AIShooterWeapon.FireReason.Normal : bl_AIShooterWeapon.FireReason.OnMove);
            //LookAtTarget();
            return true;
        }
        else
        {
            //if there are not nears cover points
            if (Target != null)//and have a target
            {
                SetDebugState(16);
                //follow the target
                SetDestination(TargetPosition, 3);
                DetermineSpeedBaseOnRange();
                personal = true;//and follow not matter the distance
                SetState(AIAgentState.Searching);
            }
            else//if don't have a target
            {
                SetDebugState(17);
                //Force to get a covert point
                CoverPoint = CoverManager.GetCloseCoverForced(m_Transform);
                SetDestination(CoverPoint.transform.position, 0.1f);
                Speed = Probability(0.5f) ? soldierSettings.walkSpeed : soldierSettings.runSpeed;
            }
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGetHit(Vector3 pos)
    {
        LastHitDirection = pos;
        //if the AI is not covering, will look for a cover point
        if (AgentState != AIAgentState.Covering)
        {
            //if the AI is following and attacking the target he will not look for cover point
            if (AgentState == AIAgentState.Following && TargetDistance <= soldierSettings.mediumRange && !ObstacleBetweenTarget)
            {
                SetLookAtState(AILookAt.Target);
                return;
            }
            Cover(false);
        }
        else
        {
            //if already in a cover and still get shoots from far away will force the AI to fire.
            if (!playerInFront)
            {
                SetLookAtState(AILookAt.Target);
                Cover(true);
            }
            else
            {
                ForceCoverFire = true;
                SetLookAtState(AILookAt.HitDirection);
            }
            //if the AI is cover but still get hit, he will search other cover point 
            if (AIHealth.Health <= 50 && Agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                Cover(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SwitchCover()
    {
        if (Agent.pathStatus != NavMeshPathStatus.PathComplete)
            return;

        if (SwitchCoverTimes <= 3)
        {
            Cover(true, AIAgentCoverArea.ToTarget);
            SwitchCoverTimes++;
        }
        else
        {
            SetState(AIAgentState.Following);
            SetDestination(TargetPosition, 3);
            SwitchCoverTimes = 0;
            AllOrNothing = true;//go straight to the target to confront him
        }
    }

    /// <summary>
    /// When the target is at look range
    /// </summary>
    void OnTargeContest(bool overrideCover)
    {
        if (AgentState == AIAgentState.Following || ForceFollowAtHalfHealth)
        {
            if (!Cover(overrideCover) || CanCover(behaviorSettings.maxCoverTime) || AllOrNothing)
            {
                if (CachedTargetDistance <= 3)
                {
                    SetDebugState(35);
                    Cover(true, AIAgentCoverArea.ToRandomPoint);
                }
                else
                {
                    SetDebugState(18);
                    Follow();
                }
            }
            else
            {
                if (Target != null)
                {
                    bl_AIShooterWeapon.FireReason fr = TargetDistance < 12 ? bl_AIShooterWeapon.FireReason.Forced : bl_AIShooterWeapon.FireReason.OnMove;
                    TriggerFire(fr);
                }
                SetDebugState(19);
                SetCrouch(true);
            }
        }
        else if (AgentState == AIAgentState.Covering)
        {
            if (CanCover(5) && TargetDistance >= 7)
            {
                SetDebugState(21);
                Cover(true);
            }
            else
            {
                SetDebugState(22);
            }
        }
        else
        {
            SetDebugState(23);
            CheckConfrontation();
            SetCrouch(false);
            DetermineSpeedBaseOnRange();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SearchPlayers()
    {
        SetDebugState(-2);
        for (int i = 0; i < PlayersInRoom.Count; i++)
        {
            Transform enemy = PlayersInRoom[i];
            if (enemy != null)
            {
                float Distance = bl_UtilityHelper.Distance(enemy.position, m_Transform.localPosition);//if a player in range, get this
                bl_AIShooter aisa = enemy.root.GetComponent<bl_AIShooter>();
                if (aisa == null || enemy.name.Contains("(die)") || aisa.isDeath) continue;

                if (Distance < soldierSettings.mediumRange)//if in range
                {
                    if (!isOneTeamMode && aisa.AITeam == AITeam) continue;

                    //get this player
                    if (GetTarget(PlayersInRoom[i])) break;
                }
            }
        }

        if (PhotonNetwork.IsMasterClient && !randomOnStartTake && PlayersInRoom.Count > 0)
        {
            if (behaviorSettings.GetRandomTargetOnStart)
            {
                SetTarget(PlayersInRoom[Random.Range(0, PlayersInRoom.Count)]);
                randomOnStartTake = true;
            }
        }
        if (!HasATarget)
        {
            if (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking) { SetState(AIAgentState.Searching); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckVision()
    {
        if (!HasATarget || !PhotonNetwork.IsMasterClient)
        {
            ObstacleBetweenTarget = false;
            return;
        }

        Vector3 relative = m_Transform.InverseTransformPoint(TargetPosition);
        playerInFront = (relative.x < 2f && relative.x > -2f) || (relative.x > -2f && relative.x < 2f);

        if (Physics.Linecast(AIWeapon.FirePoint.position, TargetPosition, out obsRay, ObstaclesLayer, QueryTriggerInteraction.Ignore))
        {
            ObstacleBetweenTarget = obsRay.transform.root.CompareTag(bl_PlayerSettings.LocalTag) == false;
        }
        else { ObstacleBetweenTarget = false; }

        if(wasTargetInSight != ObstacleBetweenTarget)
        {
            OnTargetLineOfSightChanged(wasTargetInSight, ObstacleBetweenTarget);
            wasTargetInSight = ObstacleBetweenTarget;
        }
    }

    /// <summary>
    /// If a enemy is not in range, then make the AI randomly patrol in the map
    /// </summary>
    /// <param name="precision">Patrol closed to the enemy's area</param>
    void RandomPatrol(bool precision)
    {
        if (isDeath)
            return;

        float precisionArea = soldierSettings.farRange;
        if (precision)
        {
            if (TargetDistance < soldierSettings.mediumRange)
            {
                SetDebugState(24);
                if (!HasATarget)
                {
                    SetTarget(GetNearestPlayer);
                }
                SetState(behaviorSettings.agentBehave == AIAgentBehave.Protective ? AIAgentState.Covering : AIAgentState.Looking);
                precisionArea = 5;
            }
            else
            {
                SetDebugState(25);
                SetState(behaviorSettings.agentBehave == AIAgentBehave.Agressive ? AIAgentState.Following : AIAgentState.Searching);
                precisionArea = 8;
            }
        }
        else
        {
            SetDebugState(26);
            SetState(AIAgentState.Patroling);
            if (behaviorSettings.agentBehave == AIAgentBehave.Agressive && !HasATarget)
            {
                SetTarget(GetNearestPlayer);
                SetCrouch(false);
            }
            ForceCoverFire = false;
        }

        SetLookAtState(AILookAt.Path);
        AIWeapon.isFiring = false;

        if (!Agent.hasPath || (time - lastPathTime) > 5)
        {
            SetDebugState(27);
            bool toAnCover = (Random.value <= behaviorSettings.randomCoverProbability);//probability of get a cover point as random destination
            Vector3 randomDirection = TargetPosition + (Random.insideUnitSphere * precisionArea);
            if (toAnCover) { randomDirection = CoverManager.GetCoverOnRadius(transform, 20).transform.position; }
            if (!HasATarget && isOneTeamMode)
            {
                randomDirection += m_Transform.localPosition;
            }
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, precisionArea, 1);
            finalPosition = hit.position;
            lastPathTime = time + Random.Range(0, 5);
            DetermineSpeedBaseOnRange();
            SetCrouch(false);
        }
        else
        {
            if (Agent.hasPath)
            {
                SetDebugState(28);
            }
            else
            {
                SetDebugState(32);
            }
        }
        SetDestination(finalPosition, 1, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void KillTheTarget()
    {
        if (!HasATarget) return;

        SetTarget(null);
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("type", AIRemoteCallType.SyncTarget);
        data.Add("viewID", -1);

        photonView.RPC(RPC_NAME, RpcTarget.Others, data);
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckConfrontation()
    {
        if (AgentState != AIAgentState.Covering)
        {
            if (lookTime >= 5)
            {
                SetState(AIAgentState.Following);
                lookTime = 0;
                return;
            }
            lookTime += delta;
            SetState(AIAgentState.Looking);
        }

        TriggerFire();
        SetCrouch(playerInFront && !ObstacleBetweenTarget);
    }

    /// <summary>
    /// 
    /// </summary>
    private void LookAtControl()
    {
        if ((Time.frameCount % bl_AIMananger.Instance.updateBotsLookAtEach) == 0 || forceUpdateRotation)
        {
            if (LookingAt != AILookAt.Path && !HasATarget)
            {
                LookingAt = AILookAt.Path;
            }

            switch (LookingAt)
            {
                case AILookAt.Path:
                case AILookAt.PathToTarget:
                    int cID = Agent.path.corners.Length > 1 ? 1 : 0;
                    var v = Agent.path.corners[cID];
                    v.y = m_Transform.localPosition.y + 0.2f;
                    LookAtPosition = v;

                    v = LookAtPosition - m_Transform.localPosition;
                    LookAtDirection = v;
                    break;
                case AILookAt.Target:
                    v = TargetPosition;
                    v.y += 0.22f;
                    LookAtPosition = v;
                    LookAtDirection = LookAtPosition - m_Transform.localPosition;
                    break;
                case AILookAt.HitDirection:
                    LookAtPosition = LastHitDirection;
                    LookAtDirection = LookAtPosition - m_Transform.localPosition;
                    if (LookAtDirection == Vector3.zero) { LookAtDirection = m_Transform.localPosition + (m_Transform.forward * 10); }
                    break;
            }

            if (bl_UtilityHelper.Distance(m_Transform.localPosition, LookAtPosition) <= 0.55f)
            {
                LookAtPosition = m_Transform.localPosition + (m_Transform.forward * 10);
                LookAtDirection = LookAtPosition - m_Transform.localPosition;
            }

            localRotation = m_Transform.localEulerAngles;
            localRotation.y = Quaternion.LookRotation(LookAtDirection).eulerAngles.y;

            forceUpdateRotation = false;
        }

        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, Quaternion.Euler(localRotation), delta * soldierSettings.rotationSmoothing);
    }

    /// <summary>
    /// 
    /// </summary>
    void Follow()
    {
        if (AgentState == AIAgentState.Covering && Random.value > 0.5f) return;

        SetLookAtState(AILookAt.Target);

        SetCrouch(false);
        SetDestination(TargetPosition, 3);
        if (CachedTargetDistance <= 3)
        {
            Speed = soldierSettings.walkSpeed;
            if (Cover(true, AIAgentCoverArea.ToTarget))
            {
                SetDebugState(29);
            }
            else if (Cover(true, AIAgentCoverArea.ToRandomPoint))
            {
                SetDebugState(30);
            }
            else
            {
                SetDebugState(34);
                SetDestination(m_Transform.position - (m_Transform.forward * 3), 0.1f);
            }
            SetState(AIAgentState.Covering);
            CheckConfrontation();
            SetCrouch(false);
            TriggerFire(bl_AIShooterWeapon.FireReason.Forced);
        }
        else
        {
            DetermineSpeedBaseOnRange();
            SetDebugState(33);
            TriggerFire();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void TriggerFire(bl_AIShooterWeapon.FireReason reason = bl_AIShooterWeapon.FireReason.Normal)
    {
        if (LookingAt == AILookAt.Path) SetLookAtState(AILookAt.Target);

        AIWeapon.Fire(reason);
    }

    /// <summary>
    /// 
    /// </summary>
    private void DetermineSpeedBaseOnRange()
    {
        Speed = (Target != null && CachedTargetDistance > soldierSettings.mediumRange) ? soldierSettings.runSpeed : soldierSettings.walkSpeed;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetState(AIAgentState newState)
    {
        if (newState == AgentState) return;

        OnAgentStateChanged(AgentState, newState);
        AgentState = newState;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        if (Target == newTarget) return;

        OnTargetChanged(Target, newTarget);
        Target = newTarget;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLookAtState(AILookAt newLookAt)
    {
        if (LookingAt == newLookAt) return;
        if(LookingAt == AILookAt.PathToTarget && newLookAt == AILookAt.Target)
        {
            if (ObstacleBetweenTarget) return;
        }

        LookingAt = newLookAt;
        forceUpdateRotation = true;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetDestination(Vector3 position, float stopedDistance, bool checkRate = false)
    {
        if (checkRate && (time - lastDestinationTime) < 2) return;

        Agent.stoppingDistance = stopedDistance;
        Agent.SetDestination(position);
        lastDestinationTime = time;
    }

    /// <summary>
    /// 
    /// </summary>
    void SetCrouch(bool crouch)
    {
        if (crouch && (AgentState == AIAgentState.Following || AgentState == AIAgentState.Looking))
        {
            crouch = false;
        }
        Anim.SetBool(animationHash[0], crouch);
        Speed = crouch ? soldierSettings.crounchSpeed : soldierSettings.walkSpeed;
        if(IsCrouch != crouch)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("type", AIRemoteCallType.CrouchState);
            data.Add("state", crouch);

            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            IsCrouch = crouch;
        }
    }

    /// <summary>
    /// This is called when the bot have a Target
    /// this check if other enemy is nearest and change of target if it's require
    /// </summary>
    void CheckEnemysDistances()
    {
        if (!behaviorSettings.checkEnemysWhenHaveATarget || PlayersInRoom.Count <= 0) return;
        if (time < nextEnemysCheck) return;

        CachedTargetDistance = bl_UtilityHelper.Distance(m_Transform.localPosition, TargetPosition);
        for (int i = 0; i < PlayersInRoom.Count; i++)
        {
            //if the enemy transform is not null or the same target that have currently have or death.
            if (PlayersInRoom[i] == null || PlayersInRoom[i] == Target || PlayersInRoom[i].name.Contains("(die)")) continue;
            //calculate the distance from this other enemy
            float otherDistance = bl_UtilityHelper.Distance(m_Transform.localPosition, PlayersInRoom[i].position);
            if (otherDistance > soldierSettings.limitRange) continue;//if this enemy is too far away...
            //and check if it's nearest than the current target (5 meters close at least)
            if (otherDistance < CachedTargetDistance && (CachedTargetDistance - otherDistance) > 5)
            {
                //calculate the angle between this bot and the other enemy to check if it's in a "View Angle"
                Vector3 targetDir = PlayersInRoom[i].position - m_Transform.localPosition;
                float Angle = Vector3.Angle(targetDir, m_Transform.forward);
                if (Angle > -55 && Angle < 55)
                {
                    //so then get it as new dangerous target
                    SetTarget(PlayersInRoom[i]);
                    //prevent to change target in at least the next 3 seconds
                    nextEnemysCheck = time + 3;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool GetTarget(Transform t)
    {
        if (t == null)
            return false;

        SetTarget(t);
        PhotonView view = GetPhotonView(Target.root.gameObject);
        if (view != null)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("type", AIRemoteCallType.SyncTarget);
            data.Add("viewID", view.ViewID);
            photonView.RPC(RPC_NAME, RpcTarget.Others, data);
            return true;
        }
        else
        {
            Debug.Log("This Target " + Target.name + "no have photonview");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateList()
    {
        PlayersInRoom = AllPlayers;
        AimTarget.name = AIName;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void CheckTargets()
    {
        if (Target != null && Target.name.Contains("(die)"))
        {
            SetTarget(null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void FootStep()
    {
        if(velocityMagnitud > 0.2f)
        footstep?.UpdateStep(Agent.speed);
    }

    /// <summary>
    /// 
    /// </summary>
    protected List<Transform> AllPlayers
    {
        get
        {
            List<Transform> list = new List<Transform>();
            Player[] players = PhotonNetwork.PlayerList;
            for (int i = 0; i < players.Length; i++)
            {
                Player p = players[i];
                if (!isOneTeamMode)
                {
                    Team pt = p.GetPlayerTeam();
                    if (pt != AITeam && pt != Team.None)
                    {
                        MFPSPlayer g = bl_GameManager.Instance.FindActor(p.NickName);
                        if (g != null)
                        {
                            list.Add(g.Actor);
                        }
                    }
                }
                else
                {
                    MFPSPlayer g = bl_GameManager.Instance.FindActor(p.NickName);
                    if (g != null)
                    {
                        list.Add(g.Actor);
                    }
                }
            }
            list.AddRange(AIManager.GetOtherBots(AimTarget, AITeam));
            return list;
        }
    }

    private float Speed
    {
        get => Agent.speed;
        set
        {
            bool cr = Anim.GetBool(animationHash[0]);
            if (cr)
            {
                Agent.speed = 2;
            }
            else
            {
                Agent.speed = value;
            }
        }
    }

    [PunRPC]
    public void RPCShooterBot(NetHashTable data, PhotonMessageInfo info)
    {
        var callType = (AIRemoteCallType)data["type"];
        switch(callType)
        {
            case AIRemoteCallType.DestroyBot:
                DestroyBot(data, info);
                break;
            case AIRemoteCallType.SyncTarget:
                SyncTargetAI(data);
                break;
            case AIRemoteCallType.CrouchState:
                Anim.SetBool(animationHash[0], (bool)data["state"]);
                break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void DestroyBot(NetHashTable data, PhotonMessageInfo info)
    {
        if (data.ContainsKey("instant"))
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Destroy(gameObject);
            }
            return;
        }
        Vector3 position = (Vector3)data["direction"];
        StartCoroutine(DestroyNetwork(position, data.ContainsKey("explosion"), info));
    }

    /// <summary>
    /// 
    /// </summary>
    void SyncTargetAI(NetHashTable data)
    {
        var view = (int)data["viewID"];
        if(view == -1)
        {
            SetTarget(null);
            return;
        }

        GameObject pr = FindPlayerRoot(view);
        if (pr == null) return;

        Transform t = pr.transform;
        if (t != null)
        {
            SetTarget(t);
        }
    }

    IEnumerator DestroyNetwork(Vector3 position, bool isExplosion, PhotonMessageInfo info)
    {
        if ((PhotonNetwork.Time - info.SentServerTime) > 5f)
        {
            Destroy(gameObject);
            yield break;
        }
        AIAnim.Ragdolled(position, isExplosion);
        yield return new WaitForSeconds(5);
        if (!PhotonNetwork.IsMasterClient)
        {
            Destroy(this.gameObject);
            yield return 0; // if you allow 1 frame to pass, the object's OnDestroy() method gets called and cleans up References.
        }
    }

    void OnLocalSpawn()
    {
        if (!isOneTeamMode && bl_GameManager.Instance.LocalPlayerTeam == AITeam)
        {
            DrawName.enabled = true;
        }
    }

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (PhotonNetwork.IsMasterClient && newPlayer.ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            photonView.RPC("RpcSync", newPlayer, AIHealth.Health);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        bl_EventHandler.onMatchStart += OnMatchStart;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
        bl_EventHandler.onMatchStart -= OnMatchStart;
    }

    void OnMatchStart() { isGameStarted = true; }
    public override void OnDeath() { CancelInvoke(); }

    public override Vector3 TargetPosition
    {
        get
        {
            if (Target != null) { return Target.position; }
            if (!isOneTeamMode && PlayersInRoom.Count > 0)
            {
                Transform t = GetNearestPlayer;
                if (t != null)
                {
                    return t.position;
                }
                else { return m_Transform.position + (m_Transform.forward * 3); }
            }
            return Vector3.zero;
        }
    }

    public Transform GetNearestPlayer
    {
        get
        {
            if (PlayersInRoom.Count > 0)
            {
                Transform t = null;
                float d = 1000;
                for (int i = 0; i < PlayersInRoom.Count; i++)
                {
                    if (PlayersInRoom[i] == null || PlayersInRoom[i].name.Contains("(die)")) continue;
                    float dis = bl_UtilityHelper.Distance(m_Transform.localPosition, PlayersInRoom[i].position);
                    if (dis < d)
                    {
                        d = dis;
                        t = PlayersInRoom[i];
                    }
                }
                return t;
            }
            else { return null; }
        }
    }

    private MFPSPlayer m_MFPSActor;
    public MFPSPlayer BotMFPSActor
    {
        get
        {
            if (m_MFPSActor == null) { m_MFPSActor = bl_GameManager.Instance.GetMFPSPlayer(AIName); }
            return m_MFPSActor;
        }
    }

    bool isNewDebug = false;
    public void SetDebugState(int stateID, bool initial = false)
    {
        if (!DebugStates) return;
        if (initial && isNewDebug)
        {
            DebugLine = $"{stateID}"; isNewDebug = true; return;
        }
        DebugLine += $"&{stateID}";
    }
    public bool Probability(float probability) { return Random.value <= probability; }
    public bool ForceFollowAtHalfHealth => AIHealth.Health < 50 && behaviorSettings.forceFollowAtHalfHealth;

    public float TargetDistance { get { return bl_UtilityHelper.Distance(m_Transform.position, TargetPosition); } }
    public bool HasATarget { get => Target != null; }
    private bool CanCover(float inTimePassed) { return ((time - CoverTime) >= inTimePassed); }

#if UNITY_EDITOR
    /*
    private void OnDrawGizmos()
    {
        if (isDeath) return;
        if (Agent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Agent.destination, 0.3f);
            Gizmos.DrawLine(m_Transform.localPosition, Agent.destination);

            if (Agent.path != null && Agent.path.corners != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < Agent.path.corners.Length; i++)
                {
                    if (i == 0)
                    {
                        // Gizmos.DrawSphere(Agent.path.corners[i], 0.05f);
                        continue;
                    }
                    else if (i == 1)
                    {
                        Gizmos.DrawSphere(Agent.path.corners[i], 0.2f);
                    }

                    Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
                }
            }
        }
        if (Target != null)
        {
            Gizmos.color = playerInFront && !ObstacleBetweenTarget ? Color.green : Color.cyan;
            Gizmos.DrawLine(m_Transform.localPosition, TargetPosition);
        }
        if (m_Transform == null) return;
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(m_Transform.localPosition, LookAtPosition);
        Gizmos.DrawWireCube(LookAtPosition, Vector3.one * 0.3f);
    }
    */

    private void OnDrawGizmosSelected()
    {
        if (!DebugStates || soldierSettings == null) return;
        if (m_Transform == null) { m_Transform = transform; }
        Gizmos.color = Color.yellow;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.limitRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.farRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.yellow;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.mediumRange, 360, 12, Quaternion.identity);
        Gizmos.color = Color.white;
        bl_UtilityHelper.DrawWireArc(m_Transform.position, soldierSettings.closeRange, 360, 12, Quaternion.identity);
    }
#endif
}