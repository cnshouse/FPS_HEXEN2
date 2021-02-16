using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.AI;

public class bl_AIShooterNetwork : bl_MonoBehaviour, IPunObservable
{
    private Transform m_Transform;
    private bool firstPackage = false;
    private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
    private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this

    public NavMeshAgent Agent { get; set; }
    public Vector3 Velocity { get; set; }

    private bl_AIMananger.BotsStats BotStat = null;
    public Transform AimTarget { get; set; }

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        Agent = References.Agent;

        bl_AIMananger.OnMaterStatsReceived += OnMasterStatsReceived;
        bl_AIMananger.OnBotStatUpdate += OnBotStatUpdate;

        GetEssentialData();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_AIMananger.OnMaterStatsReceived -= OnMasterStatsReceived;
        bl_AIMananger.OnBotStatUpdate -= OnBotStatUpdate;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(m_Transform.localPosition);
            stream.SendNext(m_Transform.localRotation);
            stream.SendNext(Agent.velocity);
        }
        else
        {
            correctPlayerPos = (Vector3)stream.ReceiveNext();
            correctPlayerRot = (Quaternion)stream.ReceiveNext();
            Velocity = (Vector3)stream.ReceiveNext();

            //Fix the translation effect on remote clients
            if (!firstPackage)
            {
                m_Transform.localPosition = correctPlayerPos;
                m_Transform.localRotation = correctPlayerRot;
                firstPackage = true;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!PhotonNetwork.IsMasterClient)//if not master client, then get position from server
        {
            m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, correctPlayerPos, Time.deltaTime * 7);
            m_Transform.localRotation = Quaternion.Lerp(m_Transform.localRotation, correctPlayerRot, Time.deltaTime * 7);
        }
        else
        {
            Velocity = Agent.velocity;
            if (bl_MatchTimeManager.Instance.isFinish)
            {
                Agent.isStopped = true;
                return;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetEssentialData()
    {
        object[] data = photonView.InstantiationData;
        AIName = (string)data[0];
        AITeam = (Team)data[1];
        gameObject.name = AIName;
        CheckNamePlate();
        //since Non master client doesn't update the view ID when bots are created, lets do it on Start
        if (!PhotonNetwork.IsMasterClient)
        {
            bl_AIMananger.Instance.UpdateBotView(References.aiShooter, photonView.ViewID);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMasterStatsReceived(List<bl_AIMananger.BotsStats> stats)
    {
        ApplyMasterInfo(stats);
    }

    /// <summary>
    /// 
    /// </summary>
    void ApplyMasterInfo(List<bl_AIMananger.BotsStats> stats)
    {
        int viewID = photonView.ViewID;
        bl_AIMananger.BotsStats bs = stats.Find(x => x.ViewID == viewID);
        if (bs != null)
        {
            AIName = bs.Name;
            AITeam = bs.Team;
            gameObject.name = AIName;
            BotStat = new bl_AIMananger.BotsStats();
            BotStat.Name = AIName;
            BotStat.Score = bs.Score;
            BotStat.Kills = bs.Kills;
            BotStat.Deaths = bs.Deaths;
            BotStat.ViewID = bs.ViewID;
            bl_EventHandler.OnRemoteActorChange(AIName, BuildPlayer(), true);
            CheckNamePlate();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stat"></param>
    void OnBotStatUpdate(bl_AIMananger.BotsStats stat)
    {
        if (stat.ViewID != photonView.ViewID) return;

        BotStat = stat;
        AIName = stat.Name;
        AITeam = BotStat.Team;
        gameObject.name = AIName;
        bl_EventHandler.OnRemoteActorChange(AIName, BuildPlayer(), true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private MFPSPlayer BuildPlayer(bool isAlive = true)
    {
        MFPSPlayer player = new MFPSPlayer()
        {
            Name = AIName,
            ActorView = photonView,
            isRealPlayer = false,
            Actor = transform,
            AimPosition = AimTarget,
            Team = AITeam,
            isAlive = isAlive,
        };
        return player;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        bl_EventHandler.OnRemoteActorChange(AIName, BuildPlayer(false), false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckNamePlate()
    {
        References.namePlateDrawer.SetName(AIName);
        if (!isOneTeamMode && bl_GameManager.Instance.LocalPlayer != null && !References.aiShooter.isDeath)
        {
            References.namePlateDrawer.enabled = bl_GameManager.Instance.LocalPlayerTeam == AITeam;
        }
        else
        {
            References.namePlateDrawer.enabled = false;
        }
    }

    private string AIName { get => References.aiShooter.AIName; set => References.aiShooter.AIName = value; }
    private Team AITeam { get => References.aiShooter.AITeam; set => References.aiShooter.AITeam = value; }

    private bl_AIShooterReferences _References;
    public bl_AIShooterReferences References
    {
        get
        {
            if (_References == null) _References = GetComponent<bl_AIShooterReferences>();
            return _References;
        }
    }
}