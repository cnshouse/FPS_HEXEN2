using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Serialization;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[RequireComponent(typeof(PhotonView))]
public class bl_PlayerNetwork : bl_MonoBehaviour, IPunObservable
{
    #region Public members
    /// <summary>
    /// the player's team is not ours
    /// </summary>
    public Team RemoteTeam { get; set; }
    /// <summary>
    /// the current state of the local player in FPV
    /// </summary>
    public PlayerFPState FPState = PlayerFPState.Idle;
    /// <summary>
    /// the object to which the player looked
    /// </summary>
    [FormerlySerializedAs("HeatTarget")]
    public Transform HeadTarget;
    /// <summary>
    /// smooth interpolation amount
    /// </summary>
    public float SmoothingDelay = 8f;
    /// <summary>
    /// list all remote weapons
    /// </summary>
    public List<bl_NetworkGun> NetworkGuns = new List<bl_NetworkGun>();
    [SerializeField]
    PhotonTransformViewPositionModel m_PositionModel = new PhotonTransformViewPositionModel();
    [SerializeField]
    PhotonTransformViewRotationModel m_RotationModel = new PhotonTransformViewRotationModel();
    public bl_GunManager gunManager;
    public bl_PlayerAnimations m_PlayerAnimation;
    public Material InvicibleMat;
    #endregion

    #region Private members
    //private
    private bl_NamePlateDrawer DrawName;
    private bool SendInfo = false;
    private bool isWeaponBlocked = false;
    private bl_NetworkGun currentGun;
    private Transform m_Transform;
    private Vector3 HeadPos = Vector3.zero;// Head Look to
    private bool networkIsCrouching;
    private string RemotePlayerName = string.Empty;
    private Vector3 velocity;
    PhotonTransformViewPositionControl m_PositionControl;
    PhotonTransformViewRotationControl m_RotationControl;
    bool m_ReceivedNetworkUpdate = false;
    private Vector3 headEuler = Vector3.zero;
#if UMM
    private bl_MiniMapItem MiniMapItem = null;
#endif
    [HideInInspector] public bool ObservedComponentsFoldoutOpen = true;
    #endregion

    #region Public Properties
    public bool isFriend { get; set; }
    public int networkGunID { get; set; }
    public bl_NetworkGun CurrenGun { get; set; }
    public int currentGunID { get; set; } = -1;
    public PlayerState NetworkBodyState { get; set; }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPhotonPlayerConnected;
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) return;

        m_Transform = transform;
        m_PositionControl = new PhotonTransformViewPositionControl(m_PositionModel);
        m_RotationControl = new PhotonTransformViewRotationControl(m_RotationModel);
        DrawName = GetComponent<bl_NamePlateDrawer>();
        NetworkGuns.ForEach(x => x.gameObject.SetActive(false));
#if UMM
        MiniMapItem = this.GetComponent<bl_MiniMapItem>();
        if (isMine && MiniMapItem != null)
        {
            MiniMapItem.enabled = false;
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        InvokeRepeating(nameof(SlowLoop), 0, 1);
    }

    /// <summary>
    /// serialization method of photon
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        m_PositionControl.OnPhotonSerializeView(m_Transform.localPosition, stream, info);
        m_RotationControl.OnPhotonSerializeView(m_Transform.localRotation, stream, info);

#if UNITY_EDITOR
        if (isMine == false)
        {
            DoDrawEstimatedPositionError();
        }
#endif

        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(GetHeadLookPosition());
            stream.SendNext(fpControler.State);
            stream.SendNext(fpControler.isGrounded);
            stream.SendNext((byte)gunManager.GetCurrentGunID);//send as byte, max value is 255.
            stream.SendNext(FPState);
            stream.SendNext(fpControler.Velocity);
        }
        else
        {
            //Network player, receive data
            HeadPos = (Vector3)stream.ReceiveNext();
            NetworkBodyState = (PlayerState)stream.ReceiveNext();
            networkIsCrouching = (bool)stream.ReceiveNext();
            networkGunID = (int)((byte)stream.ReceiveNext());
            FPState = (PlayerFPState)stream.ReceiveNext();
            velocity = (Vector3)stream.ReceiveNext();
            m_ReceivedNetworkUpdate = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (bl_GameData.Instance.DropGunOnDeath)
        {
            NetGunsRoot.gameObject.SetActive(false);
        }
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPhotonPlayerConnected;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ///if the player is not ours, then
        if (isConnected == false || !PhotonNetwork.InRoom) return;

        if (photonView != null && !isMine)
        {
            OnRemotePlayer();
        }
        else
        {
            OnLocalPlayer();
        }
    }

    /// <summary>
    /// Function called each frame when the player is local
    /// </summary>
    void OnLocalPlayer()
    {
        //send the state of player local for remote animation
        m_PlayerAnimation.BodyState = fpControler.State;
        m_PlayerAnimation.grounded = fpControler.isGrounded;
        m_PlayerAnimation.velocity = fpControler.Velocity;
        m_PlayerAnimation.FPState = FPState;
    }

    /// <summary>
    /// Function called each frame when the player is remote
    /// </summary>
    void OnRemotePlayer()
    {
        if (m_Transform.parent == null)
        {
            UpdatePosition();
            UpdateRotation();
        }

        HeadTarget.LookAt(HeadPos);

        m_PlayerAnimation.BodyState = NetworkBodyState;//send the state of player local for remote animation
        m_PlayerAnimation.grounded = networkIsCrouching;
        m_PlayerAnimation.velocity = velocity;
        m_PlayerAnimation.FPState = FPState;

        if (!isOneTeamMode)
        {
            //Determine if remote player is teamMate or enemy
            if (isFriend)
                OnTeammatePlayer();
            else
                OnEnemyPlayer();
        }
        else
        {
            OnEnemyPlayer();
        }

        if (!isWeaponBlocked)
        {
            CurrentTPVGun();
            currentGunID = networkGunID;
        }
    }

    /// <summary>
    /// Function called each frame when this Remote player is an enemy
    /// </summary>
    void OnEnemyPlayer()
    {
        playerHealthManager.DamageEnabled = true;
        DrawName.enabled = bl_SpectatorMode.IsActive();
#if UMM
        if (bl_MiniMapData.Instance.showEnemysWhenFire)
        {
#if KSA
            if (bl_KillStreakHandler.Instance.activeAUVs > 0) return;
#endif
            if (FPState == PlayerFPState.Firing || FPState == PlayerFPState.FireAiming)
            {
                MiniMapItem?.ShowItem();
            }
            else
            {
                MiniMapItem?.HideItem();
            }
        }
#endif
    }

    /// <summary>
    /// Function called each frame when this Remote player is a teammate.
    /// </summary>
    void OnTeammatePlayer()
    {
        playerHealthManager.DamageEnabled = bl_RoomSettings.Instance.CurrentRoomInfo.friendlyFire;
        DrawName.enabled = true;
        characterController.enabled = false;

        if (!SendInfo)
        {
            SendInfo = true;
            GetComponentInChildren<bl_BodyPartManager>().IgnorePlayerCollider();
        }

#if UMM
        MiniMapItem?.ShowItem();
#endif
    }

    /// <summary>
    /// This function control which TP Weapon should be showing on remote players.
    /// </summary>
    public void CurrentTPVGun(bool local = false)
    {
        if (gunManager == null)
            return;
        if (networkGunID == currentGunID) return;

        //Get the current gun ID local and sync with remote
        for(int i = 0; i < NetworkGuns.Count; i++)
        {
            currentGun = NetworkGuns[i];
            if (currentGun == null) continue;

            int currentID = (local) ? gunManager.GetCurrentWeapon().GunID : networkGunID;
            if (currentGun.GetWeaponID == currentID)
            {
                currentGun.gameObject.SetActive(true);
                if (!local)
                {
                    CurrenGun = currentGun;
                    CurrenGun.SetUpType();
                }
            }
            else
            {
                if(currentGun != null)
                    currentGun.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SlowLoop()
    {
        if (photonView == null || photonView.Owner == null) return;
        RemotePlayerName = photonView.Owner.NickName;
        RemoteTeam = photonView.Owner.GetPlayerTeam();
        gameObject.name = RemotePlayerName;
        if (DrawName != null) { DrawName.SetName(RemotePlayerName); }
        isFriend = (RemoteTeam == PhotonNetwork.LocalPlayer.GetPlayerTeam());
    }

    /// <summary>
    /// Change the current TPWeapon on this remote player.
    /// </summary>
    public void SetNetworkWeapon(GunType weaponType, bl_NetworkGun networkGun)
    {
        m_PlayerAnimation?.SetNetworkWeapon(weaponType, networkGun);
    }

    /// <summary>
    /// Send a call to all other clients to sync a bullet
    /// </summary>
    public void IsFire(GunType m_type, Vector3 hitPosition)
    {
        photonView.RPC(nameof(FireSync), RpcTarget.Others, new object[] { (int)m_type, hitPosition });
        if (m_PlayerAnimation.isActiveAndEnabled)
        {
            m_PlayerAnimation.PlayFireAnimation(m_type);
        }
    }

    /// <summary>
    /// public method to send the RPC shot synchronization
    /// </summary>
    public void IsFireGrenade(float t_spread, Vector3 pos, Quaternion rot, Vector3 angular)
    {
        photonView.RPC(nameof(FireGrenadeRpc), RpcTarget.Others, new object[] { t_spread, pos, rot, angular });
    }

    /// <summary>
    /// Synchronize the shot with the current remote weapon
    /// send the information necessary so that fire
    /// impact in the same direction as the local
    /// </summary>
    [PunRPC]
    void FireSync(int m_type, Vector3 hitPosition)
    {
        if (CurrenGun == null) return;
        GunType t = (GunType)(m_type);
        switch (t)
        {
            case GunType.Machinegun:
                CurrenGun.Fire(hitPosition);
                m_PlayerAnimation.PlayFireAnimation(GunType.Machinegun);
                break;
            case GunType.Pistol:
                CurrenGun.Fire(hitPosition);
                m_PlayerAnimation.PlayFireAnimation(GunType.Pistol);
                break;
            case GunType.Burst:
            case GunType.Sniper:
            case GunType.Shotgun:
                CurrenGun.Fire(hitPosition);
                break;
            case GunType.Knife:
                CurrenGun.KnifeFire();//if you need add your custom fire launcher in networkgun
                m_PlayerAnimation.PlayFireAnimation(GunType.Knife);
                break;
            default:
                Debug.LogWarning("Not defined weapon type to sync bullets.");
                break;
        }
    }

    public void SyncCustomProjectile(Hashtable data) => photonView.RPC(nameof(RPCFireCustom), RpcTarget.Others, data);

    [PunRPC]
    void RPCFireCustom(Hashtable data)
    {
        CurrenGun?.FireCustomLogic(data);
        if(CurrenGun != null)
        m_PlayerAnimation.PlayFireAnimation(CurrenGun.Info.Type);
    }

    [PunRPC]
    void FireGrenadeRpc(float m_spread, Vector3 pos, Quaternion rot, Vector3 angular)
    {
        CurrenGun.GetComponent<bl_NetworkGun>().GrenadeFire(m_spread, pos, rot, angular);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveGrenade(bool active)
    {
        photonView.RPC(nameof(SyncOffAmmoGrenade), RpcTarget.Others, active);
    }

    [PunRPC]
    void SyncOffAmmoGrenade(bool active)
    {
        if (CurrenGun == null)
        {
            Debug.LogError("Grenade is not active on TPS Player");
            return;
        }
        CurrenGun.GetComponent<bl_NetworkGun>().DesactiveGrenade(active, InvicibleMat);
    }

    public void SetWeaponBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (!PhotonNetwork.OfflineMode)
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, blockState);
        else
            RPCSetWBlocked(blockState);
    }

    [PunRPC]
    public void RPCSetWBlocked(int blockState)
    {
        isWeaponBlocked = blockState == 1;
        if (isWeaponBlocked)
        {
            for (int i = 0; i < NetworkGuns.Count; i++)
            {
                NetworkGuns[i].gameObject.SetActive(false);
            }
        }
        m_PlayerAnimation.OnWeaponBlock(blockState);
        currentGunID = -1;
    }

#if CUSTOMIZER
    [PunRPC]
    void SyncCustomizer(int weaponID, string line, PhotonMessageInfo info)
    {
        if (photonView.ViewID != info.photonView.ViewID) return;

        bl_NetworkGun ng = NetworkGuns.Find(x => x.LocalGun.GunID == weaponID);
        if(ng != null)
        {
            if(ng.GetComponent<bl_CustomizerWeapon>() != null)
            {
                ng.GetComponent<bl_CustomizerWeapon>().ApplyAttachments(line);
            }
            else
            {
                Debug.LogWarning("You have not setup the attachments in the TPWeapon: " + weaponID);
            }
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    void UpdatePosition()
    {
        if (m_PositionModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        m_Transform.localPosition = m_PositionControl.UpdatePosition(m_Transform.localPosition);
    }
    /// <summary>
    /// 
    /// </summary>
    void UpdateRotation()
    {
        if (m_RotationModel.SynchronizeEnabled == false || m_ReceivedNetworkUpdate == false)
        {
            return;
        }

        m_Transform.localRotation = m_RotationControl.GetRotation(m_Transform.localRotation);
    }

    /// <summary>
    /// 
    /// </summary>
    void DoDrawEstimatedPositionError()
    {
        if (NetworkBodyState == PlayerState.InVehicle) return;

        Vector3 targetPosition = m_PositionControl.GetNetworkPosition();

        Debug.DrawLine(targetPosition, m_Transform.position, Color.red, 2f);
        Debug.DrawLine(m_Transform.position, m_Transform.position + Vector3.up, Color.green, 2f);
        Debug.DrawLine(targetPosition, targetPosition + Vector3.up, Color.red, 2f);
    }

    /// <summary>
    /// These values are synchronized to the remote objects if the interpolation mode
    /// or the extrapolation mode SynchronizeValues is used. Your movement script should pass on
    /// the current speed (in units/second) and turning speed (in angles/second) so the remote
    /// object can use them to predict the objects movement.
    /// </summary>
    /// <param name="speed">The current movement vector of the object in units/second.</param>
    /// <param name="turnSpeed">The current turn speed of the object in angles/second.</param>
    public void SetSynchronizedValues(Vector3 speed, float turnSpeed)
    {
        m_PositionControl.SetSynchronizedValues(speed, turnSpeed);
    }

    public void OnPhotonPlayerConnected(Player newPlayer)
    {
        if (photonView.IsMine)
        {
            photonView.RPC(nameof(RPCSetWBlocked), RpcTarget.Others, isWeaponBlocked ? 1 : 0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetHeadLookPosition()
    {
        if (PlayerReferences == null || PlayerReferences.playerIK == null)
            return HeadTarget.position + HeadTarget.forward;

        return PlayerReferences.playerIK.HeadLookTarget.position;
    }

    public Transform NetGunsRoot
    {
        get { if (!bl_GameData.Instance.DropGunOnDeath) { CurrentTPVGun(true); } return NetworkGuns[0].transform.parent; }
    }

    private bl_PlayerReferences playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (playerReferences == null) playerReferences = GetComponent<bl_PlayerReferences>();
            return playerReferences;
        }
    }
    private bl_FirstPersonController fpControler => PlayerReferences.firstPersonController;
    private bl_PlayerHealthManager playerHealthManager => PlayerReferences.playerHealthManager;
    private CharacterController characterController => PlayerReferences.characterController;
}