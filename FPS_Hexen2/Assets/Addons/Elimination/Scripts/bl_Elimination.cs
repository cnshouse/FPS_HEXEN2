using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Hastable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;
using MFPS.GameModes.Elimination;

public class bl_Elimination : bl_PhotonHelper, IGameMode
{
    [Header("SETTINGS")]
    [Range(1, 10)] public int FinishRoundWait = 6;
    [Header("REFERENCES")]
    [SerializeField] private AudioClip RoundFinishSound;
    [SerializeField] private GameObject ModeContent;
    [SerializeField] private GameObject SpectatorCamera;
    public bl_EliminationUI UI;

    public ElimState elimState = ElimState.Waiting;
    private Dictionary<string, MFPSPlayer> Team1AlivePlayers = new Dictionary<string, MFPSPlayer>();
    private Dictionary<string, MFPSPlayer> Team2AlivePlayers = new Dictionary<string, MFPSPlayer>();
    private bl_KillCam LocalKillCamera;
    private int NumberOfRounds = 1;
    private bool InitializatedRound = false;
    private AudioSource ASource;
    private bool finalized = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        ASource = GetComponent<AudioSource>();
        Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Initialize()
    {
        //check if this is the game mode of this room
        if (bl_GameManager.Instance.IsGameMode(GameMode.ELIM, this))
        {
            bl_GameManager.Instance.onAllPlayersRequiredIn += FirstStart;
            bl_RoomMenu.Instance.onWaitUntilRoundFinish += SetSpectatorCamera;
            bl_EventHandler.onBotDeath += OnBotDeath;
            bl_EventHandler.onBotsInitializated += OnBotsInitialized;
            bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.EliminationGameMode, OnNetworkMessage);
            NumberOfRounds = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal] + 1;
            if (bl_GameManager.Instance.WaitForPlayers(GameMode.ELIM.GetModeInfo().RequiredPlayersToStart))//if still waiting for players
            {

            }
            ModeContent.SetActive(true);
            UI.Init();
        }
        else
        {
            ModeContent.SetActive(false);
            enabled = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        if (GetGameMode == GameMode.ELIM)
        {
            bl_GameManager.Instance.onAllPlayersRequiredIn -= FirstStart;
            bl_RoomMenu.Instance.onWaitUntilRoundFinish -= SetSpectatorCamera;
            bl_EventHandler.onBotDeath -= OnBotDeath;
            bl_EventHandler.onBotsInitializated -= OnBotsInitialized;
            bl_PhotonNetwork.Instance.RemoveCallback(OnNetworkMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnNetworkMessage(Hastable data)
    {
        var cmd = (byte)data["cmd"];
        switch (cmd)
        {
            case 0:
                RPCStartElimRound(data);
                break;
            case 1:
                RPCPlayerDeathElim(data);
                break;
            case 2:
                OnFinishRoundElim();
                break;
        }
    }

    /// <summary>
    /// This is called when a player join but the game is already started
    /// so the player will wait until this round finish
    /// </summary>
    public void SetSpectatorCamera(Team team)
    {
        UI.WaitingForRoundUI.SetActive(true);
        bl_GameManager.Instance.m_RoomCamera.gameObject.SetActive(false);
        SpectatorCamera.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void FirstStart()
    {
        this.InvokeAfter(4, StartRound);
    }

    /// <summary>
    /// This is called when is waiting for player and the last needed enter
    /// </summary>
    public void StartRound()
    {
        //master set the call to all other to start the game
        if (PhotonNetwork.IsMasterClient)
        {
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", (byte)0);
            data.Add("state", (byte)0);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.EliminationGameMode, data);
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void RPCStartElimRound(Hastable data)
    {
        bl_GameManager.Instance.SetGameState(MatchState.Playing);
        if (LocalKillCamera != null) { Destroy(LocalKillCamera.gameObject); }
        UI.WaitingForRoundUI.SetActive(false);
        Team1AlivePlayers.Clear();
        Team2AlivePlayers.Clear();
        //listed all players ready to play this round
        Player[] t1 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1);
        Player[] t2 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2);

        for (int i = 0; i < t1.Length; i++)
        {
            Team1AlivePlayers.Add(t1[i].NickName, bl_GameManager.Instance.GetMFPSPlayer(t1[i].NickName));
        }
        for (int i = 0; i < t2.Length; i++)
        {
            Team2AlivePlayers.Add(t2[i].NickName, bl_GameManager.Instance.GetMFPSPlayer(t2[i].NickName));
        }

        if (bl_AIMananger.Instance.BotsActive)
        {
            var bt1 = bl_AIMananger.Instance.GetAllBotsInTeam(Team.Team1);
            var bt2 = bl_AIMananger.Instance.GetAllBotsInTeam(Team.Team2);

            for (int i = 0; i < bt1.Count; i++)
            {
                Team1AlivePlayers.Add(bt1[i].Name, bt1[i]);
            }
            for (int i = 0; i < bt2.Count; i++)
            {
                Team2AlivePlayers.Add(bt2[i].Name, bt2[i]);
            }
        }
        UI.InstancePlayers();
        UI.WaitingForRoundUI.SetActive(false);
        bl_GameManager.Instance.SpawnPlayer(PhotonNetwork.LocalPlayer.GetPlayerTeam());
        bl_MatchTimeManager.Instance.RestartTime();
        bl_UIReferences.Instance.ShowScoreboard(false);
        InitializatedRound = true;
        //disable spectator camera
        SpectatorCamera.SetActive(false);

        if (PhotonNetwork.IsMasterClient && data.ContainsKey("state") && (byte)data["state"] == 1)
        {
            if (bl_AIMananger.Instance.BotsActive)
            {
                bl_AIMananger.Instance.RespawnAllBots();
            }
        }
    }

    /// <summary>
    /// This is only on local player when this die
    /// </summary>
    public void OnLocalPlayerDeath()
    {
        //notify all others that I have die in this round
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("cmd", (byte)1);
        data.Add("name", string.Empty);
        data.Add("team", Team.None);
        data.Add("sname", PhotonNetwork.LocalPlayer.NickName);
        data.Add("steam", PhotonNetwork.LocalPlayer.GetPlayerTeam());
        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.EliminationGameMode, data);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="botName"></param>
    void OnBotDeath(string botName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            var mfpsPlayer = bl_GameManager.Instance.GetMFPSPlayer(botName);
            if (mfpsPlayer == null)
            {
                Debug.LogError($"MFPS Player '{botName}' can't be found.");
                return;
            }

            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("cmd", (byte)1);
            data.Add("name", botName);
            data.Add("team", mfpsPlayer.Team);
            data.Add("sname", PhotonNetwork.LocalPlayer.NickName);
            data.Add("steam", PhotonNetwork.LocalPlayer.GetPlayerTeam());
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.EliminationGameMode, data);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnBotsInitialized()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void RPCPlayerDeathElim(Hastable data)
    {
        if (!InitializatedRound) return;

        var team = (Team)data["team"];
        var nickName = (string)data["name"];
        var senderTeam = (Team)data["steam"];
        var senderNickName = (string)data["sname"];

        Team t = team == Team.None ? senderTeam : team;
        string playerName = string.IsNullOrEmpty(nickName) ? senderNickName : nickName;

        //remove the death players from alive player list
        if (t == Team.Team1)
        {
            Team1AlivePlayers.Remove(playerName);
        }
        else
        {
            Team2AlivePlayers.Remove(playerName);
        }
        // Debug.Log($"Remaining {Team1AlivePlayers.Count} on t1 and {Team2AlivePlayers.Count} on t2");
        UI.OnPlayerDeath(playerName, t);
        //check if one of the team have 0 players after remove the player
        if (Team1AlivePlayers.Count <= 0 || Team2AlivePlayers.Count <= 0)
        {
            //if so finish the round, master set the call to finish this round
            if (PhotonNetwork.IsMasterClient)
            {
                var newdata = bl_UtilityHelper.CreatePhotonHashTable();
                newdata.Add("cmd", (byte)2);
                bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.EliminationGameMode, data);
                bl_GameManager.Instance.SetGameState(MatchState.Waiting);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnFinishRoundElim()
    {
        bl_EventHandler.DispatchRoundEndEvent();
        NumberOfRounds--;
        if (NumberOfRounds >= 0)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Hastable t = new Hastable();
                t.Add(PropertiesKeys.RoomGoal, NumberOfRounds);
                PhotonNetwork.CurrentRoom.SetCustomProperties(t);
                PhotonNetwork.CurrentRoom.SetTeamScore(GetWinner);
                if (NumberOfRounds <= 1)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    PhotonNetwork.CurrentRoom.IsVisible = false;
                }
            }
            bl_MatchTimeManager.Instance.Pause = true;
            StartCoroutine(RoundFinish(FinishRoundWait));
        }
        else
        {
            if (!finalized)
            {
                finalized = true;
                GameOver();
            }
        }
        if (ASource != null && RoundFinishSound != null)
        {
            ASource.clip = RoundFinishSound;
            ASource.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GameOver()
    {
        bl_MatchTimeManager.Instance.FinishRound();
        //determine the winner
        string finalText = "";
        if (GetWinner != Team.None)
        {
            finalText = GetWinner.GetTeamName();
        }
        else
        {
            finalText = bl_GameTexts.NoOneWonName;
        }
        bl_UIReferences.Instance.SetFinalText(finalText);
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
            UI.RoundStateText.text = string.Format("NEXT ROUND IN: {0}", Mathf.CeilToInt(FinishRoundWait - (wait * d)));
            yield return null;
        }
        bl_MatchTimeManager.Instance.StartNewRoundAndKeepData();
        //bl_MatchTimeManager.Instance.Pause = false;
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("state", (byte)1);
        RPCStartElimRound(data);
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
        if (GetGameMode != GameMode.ELIM) return;
        LocalKillCamera = kc;
        MFPSPlayer friend = null;
        //after local die, and round have not finish, find a team player to spectating
        if (PhotonNetwork.LocalPlayer.GetPlayerTeam() == Team.Team1)
        {
            if (Team1AlivePlayers.Count > 0)
                friend = Team1AlivePlayers.Values.ElementAt(Random.Range(0, Team1AlivePlayers.Values.Count));
        }
        else
        {
            if (Team2AlivePlayers.Count > 0)
                friend = Team2AlivePlayers.Values.ElementAt(Random.Range(0, Team2AlivePlayers.Values.Count));
        }
        if (friend != null && friend.Actor != null)
        {
            GameObject player = friend.Actor.gameObject;
            if (player != null)
                LocalKillCamera.SpectPlayer(player.transform);
        }
        else
        {
            Debug.Log("Any friend available to spectate");
        }
    }

    public void OnLocalPoint(int point, Team team)
    {

    }

    private Team GetWinner
    {
        get
        {
            if (Team1AlivePlayers.Count <= 0) { return Team.Team2; }
            else if (Team2AlivePlayers.Count <= 0) { return Team.Team1; } else { return Team.None; }
        }
    }

    [PunRPC]
    void SyncElimGame(ElimState state, string[] alivePlayers)
    {
        elimState = state;
        UI.InstancePlayersFromServer(alivePlayers);

        int t1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
        UI.Team1ScoreText.text = t1.ToString();
        int t2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
        UI.Team2ScoreText.text = t2.ToString();
    }

    public void OnOtherPlayerEnter(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        List<string> alivePlayers = new List<string>();
        alivePlayers.AddRange(Team1AlivePlayers.Keys.ToArray());
        alivePlayers.AddRange(Team2AlivePlayers.Keys.ToArray());

        photonView.RPC(nameof(SyncElimGame), newPlayer, elimState, alivePlayers.ToArray());
    }

    public void OnOtherPlayerLeave(Player otherPlayer)
    {
        Team t = otherPlayer.GetPlayerTeam();
        if (t != Team.None)
        {
            UI.OnPlayerDeath(otherPlayer.NickName, t);
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
                    GameOver();
                }
            }
        }
    }

    public void OnRoomPropertiesUpdate(Hastable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team1Score))
        {
            int t1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
            UI.Team1ScoreText.text = t1.ToString();
        }
        else if (propertiesThatChanged.ContainsKey(PropertiesKeys.Team2Score))
        {
            int t2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
            UI.Team2ScoreText.text = t2.ToString();
        }
    }

    public void OnFinishTime(bool gameOver)
    {
        if (finalized) return;

        OnFinishRoundElim();
    }

    public void OnLocalPlayerKill()
    {
    }

    public bool isLocalPlayerWinner
    {
        get
        {
            return GetWinner == PhotonNetwork.LocalPlayer.GetPlayerTeam();
        }
    }

    public enum ElimState
    {
        Waiting = 0,
        Playing = 1,
        Restarting = 2,
        Finish = 3,
    }

    private static bl_Elimination _instance;
    public static bl_Elimination Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_Elimination>(); }
            return _instance;
        }
    }
}