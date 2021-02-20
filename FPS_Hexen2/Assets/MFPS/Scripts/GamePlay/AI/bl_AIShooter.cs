using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MFPS.Runtime.AI;

public abstract class bl_AIShooter : bl_MonoBehaviour
{
    public AIAgentState AgentState { get; set; } = AIAgentState.Idle;
    public Transform Target { get; set; }
    public virtual Transform AimTarget { get; protected set; }
    public bool isDeath { get; set; } = false;
    public Team AITeam { get; set; } = Team.None;
    public bool isTeamMate { get { return (AITeam == PhotonNetwork.LocalPlayer.GetPlayerTeam() && !isOneTeamMode); } }

    private string _AIName = string.Empty;
    public string AIName
    {
        get
        {
            return _AIName;
        }
        set
        {
            _AIName = value;
            gameObject.name = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void Init() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void OnDeath() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual void CheckTargets() { }

    /// <summary>
    /// 
    /// </summary>
    public virtual Vector3 TargetPosition
    {
        get => Target.position;
    }

    /// <summary>
    /// 
    /// </summary>
    private Vector3 m_lookAtDirection = Vector3.forward;
    public virtual Vector3 LookAtDirection
    {
        get => m_lookAtDirection;
        set => m_lookAtDirection = value;
    }

    /// <summary>
    /// 
    /// </summary>
    private Vector3 m_lookAtPosition = Vector3.forward;
    public virtual Vector3 LookAtPosition
    {
        get => m_lookAtPosition;
        set => m_lookAtPosition = value;
    }

    private bl_AIShooterReferences _references;
    public bl_AIShooterReferences References
    {
        get
        {
            if (_references == null) _references = GetComponent<bl_AIShooterReferences>();
            return _references;
        }
    }

}