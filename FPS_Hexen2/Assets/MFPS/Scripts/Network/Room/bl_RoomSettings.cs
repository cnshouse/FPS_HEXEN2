using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;
using Photon.Realtime;

public class bl_RoomSettings : bl_MonoBehaviour
{
    [LovattoToogle] public bool canSuicide = true;

    #region Public properties
    public GameMode CurrentGameMode => CurrentRoomInfo.gameMode;
    public int GameGoal => CurrentRoomInfo.goal;
    public bool AutoTeamSelection => CurrentRoomInfo.autoTeamSelection;
    public MFPSRoomInfo CurrentRoomInfo { get; set; }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if ((!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom) && !bl_GameData.Instance.offlineMode)
            return;

        ResetRoom();
        GetRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator Start()
    {
        while (!bl_GameData.isDataCached) yield return null;
        if (bl_MFPS.Settings != null) bl_MFPS.Settings.ApplySettings(false, true);
    }

    /// <summary>
    /// Reset all the room properties to it's default values
    /// </summary>
    public void ResetRoom()
    {
        Hashtable table = new Hashtable();
        //Initialize new properties where the information will stay Room
        if (PhotonNetwork.IsMasterClient)
        {
            table.Add(PropertiesKeys.Team1Score, 0);
            table.Add(PropertiesKeys.Team2Score, 0);
            CurrentRoom.SetCustomProperties(table);
        }
        table.Clear();
        //Initialize new properties where the information will stay Players
        if (bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.DirectToMap || PhotonNetwork.OfflineMode)
        {
            table.Add(PropertiesKeys.TeamKey, Team.None.ToString());
        }
        table.Add(PropertiesKeys.KillsKey, 0);
        table.Add(PropertiesKeys.DeathsKey, 0);
        table.Add(PropertiesKeys.ScoreKey, 0);
        table.Add(PropertiesKeys.UserRole, bl_GameData.Instance.RolePrefix);
        LocalPlayer.SetCustomProperties(table);

#if ULSP && LM
        bl_DataBase db = FindObjectOfType<bl_DataBase>();
        int scoreLevel = 0;
        if (db != null)
        {
            scoreLevel = db.LocalUser.Score;
        }
        Hashtable PlayerTotalScore = new Hashtable();
        PlayerTotalScore.Add("TotalScore", scoreLevel);
        LocalPlayer.SetCustomProperties(PlayerTotalScore);
#endif      
    }

    /// <summary>
    /// Get the custom room properties of this room
    /// These properties was set by the room creator (Host)
    /// </summary>
    void GetRoomInfo()
    {
        CurrentRoomInfo = PhotonNetwork.CurrentRoom.GetRoomInfo();
        bl_MatchTimeManager.Instance.roundStyle = (RoundStyle)CurrentRoom.CustomProperties[PropertiesKeys.RoomRoundKey];
        CheckAutoSpawn();
    }

    /// <summary>
    /// Check if the local player should spawn automatically
    /// or should let him decide when spawn.
    /// </summary>
    public void CheckAutoSpawn()
    {
        if (CurrentRoomInfo.autoTeamSelection && bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.DirectToMap)
        {
            bl_UIReferences.Instance.AutoTeam(true);
            bl_UIReferences.Instance.ShowMenu(false);
            Invoke(nameof(SelectTeamAutomatically), 3);
        }
        else if (bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.WaitingRoom && LocalPlayer.GetPlayerTeam() == Team.None)
        {
            if (PhotonNetwork.OfflineMode && !CurrentRoomInfo.autoTeamSelection)
            {
                bl_UIReferences.Instance.SetUpUI();
                bl_UIReferences.Instance.SetUpJoinButtons(true);
                return;
            }

            bl_UIReferences.Instance.AutoTeam(true);
            bl_UIReferences.Instance.ShowMenu(false);
            Invoke(nameof(SelectTeamAutomatically), 3);
        }
    }

    /// <summary>
    /// Set the player that just join to the room to the team with less players on it automatically
    /// </summary>
    void SelectTeamAutomatically()
    {
        string joinText = isOneTeamMode ? bl_GameTexts.JoinedInMatch : bl_GameTexts.JoinIn;
#if LOCALIZATION
         joinText = isOneTeamMode ? bl_Localization.Instance.GetText(17) : bl_Localization.Instance.GetText(23);
#endif

        int teamDelta = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
        int teamRecon = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;
        Team team = Team.All;
        if (!isOneTeamMode)
        {
            if (teamDelta > teamRecon)
            {
                team = Team.Team2;
            }
            else if (teamDelta < teamRecon)
            {
                team = Team.Team1;
            }
            else if (teamDelta == teamRecon)
            {
                team = Team.Team1;
            }

            string jt = string.Format("{0} {1}", joinText, team);
            bl_KillFeed.Instance.SendTeamHighlightMessage(PhotonNetwork.NickName, jt, team);
        }
        else
        {
            bl_KillFeed.Instance.SendMessageEvent(string.Format("{0} {1}", PhotonNetwork.NickName, joinText));
        }
        bl_RoomMenu.Instance.OnAutoTeam();
        bl_UIReferences.Instance.AutoTeam(false);

        if (GetGameMode.GetGameModeInfo().onRoundStartedSpawn == GameModeSettings.OnRoundStartedSpawn.WaitUntilRoundFinish && bl_GameManager.Instance.GameMatchState == MatchState.Playing)
        {
            if (bl_RoomMenu.Instance.onWaitUntilRoundFinish != null) { bl_RoomMenu.Instance.onWaitUntilRoundFinish.Invoke(team); }
            bl_GameManager.Instance.SetLocalPlayerToTeam(team);
            return;
        }

        //spawn player
        bl_GameManager.Instance.SpawnPlayer(team);
    }

    private static bl_RoomSettings _instance;
    public static bl_RoomSettings Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_RoomSettings>(); }
            return _instance;
        }
    }
}