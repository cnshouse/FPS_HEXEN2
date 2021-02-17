using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hastable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using MFPS.GameModes.Demolition;
using MFPS.Audio;

public class bl_Demolition : bl_PhotonHelper, IGameMode
{
    [Header("SETTINGS")]
    [Range(1, 25)] public int DetonationTime = 30;
    [Range(1, 10)] public int plantDuration = 5;
    [Range(1, 10)] public int defuseDuration = 5;
    [Range(1, 10)] public int FinishRoundWait = 6;
    public Team terroristsTeam = Team.Team2;
    public TerroristWinRequire terroristWinRequire = TerroristWinRequire.DefuseBomb;
    public BombAssignMethod bombAssignMethod = BombAssignMethod.PlayerPicked;
    public KeyCode plantKey = KeyCode.F;
    [HideInInspector]  public bool useHandsAnimation = false;//WIP
    [Header("References")]
    public GameObject Content;
    [SerializeField] private AudioClip RoundFinishSound;
    [SerializeField] private GameObject SpectatorCamera;
    [Header("Audio")]
    public bl_VirtualAudioController virtualAudioController;

    public bool isLocalInZone { get; set; }
    public bool canPlant { get; set; }
    public delegate void EnterInZoneEvent(bool enter);
    public static EnterInZoneEvent EnterInZone;

    private Dictionary<int, Player> Team1AlivePlayers = new Dictionary<int, Player>();
    private Dictionary<int, Player> Team2AlivePlayers = new Dictionary<int, Player>();
    private bl_KillCam LocalKillCamera;
    private int NumberOfRounds = 1;
    private bool InitializatedRound = false;
    private AudioSource ASource;
    private bl_DemolitionUI UI;
    public bl_DemolitionBombZone plantingZone { get; set; }
    public bool bombInSight { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!isRoomReady)
            return;

        PhotonNetwork.AddCallbackTarget(this);
        ASource = GetComponent<AudioSource>();
        UI = bl_DemolitionUI.Instance;
        Initialize();
    }

    /// <summary>
    /// Called locally only when the local player enter in the bomb zone trigger
    /// </summary>
    public void OnEnterInZone(bool enter, bl_DemolitionBombZone zone)
    {
        isLocalInZone = enter;
        if (enter)
        {
            //since is locally call, check if the local player is the bomb carrier
            if (bl_DemolitionBombManager.Instance.isLocalPlayerTheCarrier)
            {
                if (bl_DemolitionBombManager.Instance.Bomb.bombStatus == bl_DemolitionBomb.BombStatus.Carried)
                {
                    plantingZone = zone;
                    //so the player is in zone and him got the bomb -> plant when you want
                    canPlant = true;
                    bl_DemolitionUI.Instance.ShowPlantGuide(true);
                }
            }
            //When a counter terrorist player enter in a bomb zone that have the bomb activated
            else if (PhotonNetwork.LocalPlayer.GetPlayerTeam() != terroristsTeam)
            {
                bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerSettings>().PlayerCamera.GetComponent<bl_CameraRay>().AddTrigger(bl_DemolitionBombManager.Instance.Bomb.bombModel.name, OnBombInSight, 40);
            }
        }
        else
        {
            canPlant = false;
            bl_DemolitionUI.Instance.ShowPlantGuide(false);
            if(PhotonNetwork.LocalPlayer.GetPlayerTeam() != terroristsTeam)
            {
                bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerSettings>().PlayerCamera.GetComponent<bl_CameraRay>().RemoveTrigger(bl_DemolitionBombManager.Instance.Bomb.bombModel.name, 40);
            }
        }
        if (EnterInZone != null) { EnterInZone(enter); }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnBombInSight(bool inSight)
    {
        bombInSight = inSight;
        bl_DemolitionUI.Instance.ShowDefuseGuide(inSight);
    }

    #region ELIMINATION
    /// <summary>
    /// This is called when a player join but the game is already started
    /// so the player will wait until this round finish
    /// </summary>
    public void SetSpectatorCamera(Team team)
    {
        UI.WaitingForRoundUI.SetActive(true);
        bl_RoomCamera.Instance?.SetActive(false);
        SpectatorCamera.SetActive(true);
    }

    /// <summary>
    /// This is called when is waiting for player and the last needed enter
    /// </summary>
    public void StartRound()
    {
        //master set the call to all other to start the game
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(DMStartRound), RpcTarget.All);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnEventReceived(Hastable data)
    {
        DemolitionEventType t = (DemolitionEventType)data["type"];
        switch (t)
        {
            case DemolitionEventType.CarrierAssign:
                bl_DemolitionBombManager.Instance.OnSelectPlayerToCarrieBomb(data);
                break;
            case DemolitionEventType.RoundFinish:
                OnFinishRound(data);
                break;
        }
    }

    [PunRPC]
    void DMStartRound()
    {
        bl_GameManager.Instance.SetGameState(MatchState.Playing);
        //if we was death and was expecting
        if (LocalKillCamera != null) { Destroy(LocalKillCamera.gameObject); }
        UI.WaitingForRoundUI.SetActive(false);
        Team1AlivePlayers.Clear();
        Team2AlivePlayers.Clear();
        //listed all players ready to play this round
        Player[] t1 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1);
        Player[] t2 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2);
        //instance the player icons next to the scoreboard
        for (int i = 0; i < t1.Length; i++)
        {
            Team1AlivePlayers.Add(t1[i].ActorNumber, t1[i]);
        }
        for (int i = 0; i < t2.Length; i++)
        {
            Team2AlivePlayers.Add(t2[i].ActorNumber, t2[i]);
        }
        //select a carrier automatically
        if (bombAssignMethod == BombAssignMethod.AutomaticallyRandom && PhotonNetwork.IsMasterClient)
        {
            //select the carrier
            int viewID = 0;
            int actorNumber = 0;
            SelectPlayerToCarrieBomb(out viewID, out actorNumber);
            Hastable data = new Hastable()
            {
                {"type", DemolitionEventType.CarrierAssign },
                {"carrierID", actorNumber },
                { "viewID", viewID }
            };
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DemolitionEvent, data);
        }
        else if (bombAssignMethod == BombAssignMethod.PlayerPicked)
        {
            bl_DemolitionBombManager.Instance.Bomb.ResetToInit();
        }
        UI.OnRoundStart();
        UI.InstancePlayers();
        UI.WaitingForRoundUI.SetActive(false);
        bl_GameManager.Instance.spawnInQueque = false;
        this.InvokeAfter(1, () =>
        {
            bl_GameManager.Instance.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
            SpectatorCamera.SetActive(false);
        });
        bl_MatchTimeManager.Instance.RestartTime();
        bl_UIReferences.Instance.ShowScoreboard(false);
        InitializatedRound = true;
        //disable spectator camera
    }

    /// <summary>
    /// 
    /// </summary>
    void SelectPlayerToCarrieBomb(out int viewID, out int actorNumber)
    {
        MFPSPlayer[] players = bl_GameManager.Instance.OthersActorsInScene.Where(x => x.Team == terroristsTeam).ToArray();
        if (players.Length <= 0)
        {
            if (PhotonNetwork.LocalPlayer.GetPlayerTeam() == terroristsTeam)
            {
                actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
                viewID = bl_GameManager.LocalPlayerViewID;
            }
            else
            {
                Debug.Log("There's not players in Terrorist team");
                actorNumber = 0;
                viewID = 0;
            }
        }
        else
        {
            MFPSPlayer p = players[Random.Range(0, players.Length)];
            actorNumber = p.ActorView.Owner.ActorNumber;
            viewID = p.ActorView.ViewID;
        }
    }


    [PunRPC]
    void RPCPlayerDeath(PhotonMessageInfo info)
    {
        if (!InitializatedRound) return;

        Team t = info.Sender.GetPlayerTeam();
        //remove the death players from alive player list
        if (t == Team.Team1)
        {
            Team1AlivePlayers.Remove(info.Sender.ActorNumber);
        }
        else
        {
            Team2AlivePlayers.Remove(info.Sender.ActorNumber);
        }
        UI.OnPlayerDeath(info.Sender, t);
        //check if one of the team have 0 players after remove the player
        if (Team1AlivePlayers.Count <= 0 || Team2AlivePlayers.Count <= 0)
        {
            //if to win require always defuse the bomb and the bomb is planted
            if(terroristWinRequire == TerroristWinRequire.DefuseBomb && bl_DemolitionBombManager.Instance.Bomb.bombStatus == bl_DemolitionBomb.BombStatus.Actived)
            {
                //check if the eliminated team is the counter terrorist team (that need to defuse the bomb)
                if (terroristsTeam == Team.Team1 && Team1AlivePlayers.Count <= 0) return;//don't finish the round yet
                else if (terroristsTeam == Team.Team2 && Team2AlivePlayers.Count <= 0) return;
            }
            //if so finish the round, master set the call to finish this round
            CallRoundFinish();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void CallRoundFinish()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Hastable data = new Hastable()
            {
                {"type", DemolitionEventType.RoundFinish },
                {"finalType",0 }
            };
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DemolitionEvent, data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnFinishRound(Hastable data)
    {
        int finalType = (int)data["finalType"];
        bl_EventHandler.DispatchRoundEndEvent();
        bl_DemolitionBombManager.Instance.StopAll();
        NumberOfRounds--;
        if (NumberOfRounds >= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Hastable t = new Hastable();
                t.Add(PropertiesKeys.RoomGoal, NumberOfRounds);
                PhotonNetwork.CurrentRoom.SetCustomProperties(t);
                if (NumberOfRounds <= 1)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
                if (finalType == 0) { PhotonNetwork.CurrentRoom.SetTeamScore(GetWinner); }//win by kill all members of a team
                else if (finalType == 1) { PhotonNetwork.CurrentRoom.SetTeamScore((Team)data["winner"]); }//win by planting or defusing the bomb
            }
            bl_MatchTimeManager.Instance.Pause = true;
            StartCoroutine(RoundFinish(FinishRoundWait));
        }
        else
        {
            bl_MatchTimeManager.Instance.FinishRound();
        }
        bl_GameManager.Instance.spawnInQueque = true;//reserve the spawn until the next round start
        if (ASource != null && RoundFinishSound != null)
        {
            ASource.clip = RoundFinishSound;
            ASource.Play();
        }
        virtualAudioController.StopClip("planted loop");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator RoundFinish(float wait)
    {
        float d = 0;
        UI.RoundStateText.gameObject.SetActive(true);
        while (d < 1)
        {
            d += Time.deltaTime / wait;
            UI.RoundStateText.text = string.Format("NEXT ROUND IN: {0}", Mathf.FloorToInt(FinishRoundWait - (wait * d)));
            yield return null;
        }
        bl_MatchTimeManager.Instance.StartNewRoundAndKeepData();
        //bl_MatchTimeManager.Instance.Pause = false;
        DMStartRound();
        UI.RoundStateText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void WaitForRoundFinish()
    {
        UI.WaitingForRoundUI.SetActive(true);
    }

    public void OnLocalDeath(bl_KillCam kc)
    {
        if (GetGameMode != GameMode.DM) return;

        bl_DemolitionBombManager.Instance.OnLocalDeath();
        LocalKillCamera = kc;
        Player friend = null;
        //after local die, and round have not finish, find a team player to spectating
        if (PhotonNetwork.LocalPlayer.GetPlayerTeam() == Team.Team1)
        {
            if(Team1AlivePlayers.Values.Count > 0)
            friend = Team1AlivePlayers.Values.ElementAt(UnityEngine.Random.Range(0, Team1AlivePlayers.Values.Count));
        }
        else
        {
            if (Team2AlivePlayers.Values.Count > 0)
                friend = Team2AlivePlayers.Values.ElementAt(UnityEngine.Random.Range(0, Team2AlivePlayers.Values.Count));
        }
        if (friend != null)
        {
            GameObject player = FindPhotonPlayer(friend);
            LocalKillCamera.SpectPlayer(player.transform);
        }
    }

    private Team GetWinner
    {
        get
        {
            if (Team1AlivePlayers.Count <= 0) { return Team.Team2; }
            else if (Team2AlivePlayers.Count <= 0) { return Team.Team1; } else { return Team.None; }
        }
    }

    public void OnRoomPropertiesUpdate(Hastable propertiesThatChanged)
    {
        int t1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        UI.Team1ScoreText.text = t1.ToString();

        int t2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
        UI.Team2ScoreText.text = t2.ToString();
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, Hastable changedProps)
    {      
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {     
    }
    #endregion

    #region Game Mode Interface
    public bool isLocalPlayerWinner
    {
        get { return false; } }


    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.DM, this))
        {
            bl_GameManager.Instance.onAllPlayersRequiredIn += StartRound;
            bl_RoomMenu.Instance.onWaitUntilRoundFinish += SetSpectatorCamera;
            NumberOfRounds = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal] + 1;
            bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.DemolitionEvent, OnEventReceived);
            if (bl_GameManager.Instance.WaitForPlayers(GameMode.DM.GetModeInfo().RequiredPlayersToStart))//if still waiting for players
            {
              
            }
            bl_DemolitionUI.Instance.Init();
            Content.SetActive(true);
            virtualAudioController.Initialized(this);
        }
        else
        {
            Content.SetActive(false);
            enabled = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if (GetGameMode != GameMode.DM) return;

        bl_GameManager.Instance.onAllPlayersRequiredIn -= StartRound;
        bl_PhotonNetwork.Instance.RemoveCallback(OnEventReceived);
        bl_RoomMenu.Instance.onWaitUntilRoundFinish -= SetSpectatorCamera;
    }


    public void OnFinishTime(bool gameOver)
    {
    }

    public void OnLocalPoint(int points, Team teamToAddPoint)
    {
    }

    public void OnLocalPlayerKill()
    {
    }

    public void OnLocalPlayerDeath()
    {
        //notify all others that I have die in this round
        photonView.RPC(nameof(RPCPlayerDeath), RpcTarget.All);
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {
        Team t = otherPlayer.GetPlayerTeam();
        if (t != Team.None)
        {
            UI.OnPlayerDeath(otherPlayer, t);
        }
        //if match already start
        if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
        {
            //and half of the match has pass
            if (bl_MatchTimeManager.Instance.CurrentTime < (bl_MatchTimeManager.Instance.RoundDuration * 0.5f))
            {
                // and one of the team have 0 player lefts
                if (PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length <= 0 || PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length <= 0)
                {
                    //finish match
                    NumberOfRounds = 0;
                    bl_EventHandler.DispatchRoundEndEvent();
                    bl_MatchTimeManager.Instance.FinishRound();
                }
            }
        }
    }
    #endregion
  
    [System.Serializable]
    public enum BombAssignMethod
    {
        AutomaticallyRandom,
        PlayerPicked,
    }

    [System.Serializable]
    public enum DemolitionEventType
    {
        CarrierAssign = 0,
        RoundFinish = 1
    }

    [System.Serializable]
    public enum TerroristWinRequire
    {
        DefuseBomb = 0,
        KillAllEnemysOrDefuse = 1
    }

    private static bl_Demolition _instance;
    public static bl_Demolition Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_Demolition>(); }
            return _instance;
        }
    }
}