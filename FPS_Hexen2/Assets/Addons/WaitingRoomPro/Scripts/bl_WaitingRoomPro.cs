using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using HashTable = ExitGames.Client.Photon.Hashtable;
using MFPS.Runtime.Settings;

public class bl_WaitingRoomPro : bl_PhotonHelper
{
    public GameObject RoomSettingsButton;
    public bl_SingleSettingsBinding mapSelector;
    public bl_SingleSettingsBinding maxPlayersSelector;
    public bl_SingleSettingsBinding gameModeSelector;
    public bl_SingleSettingsBinding timeLimitSelector;
    public bl_SingleSettingsBinding goalSelector;
    public bl_SingleSettingsBinding pingSelector;
    public bl_SingleSettingsBinding botsActiveSelector;
    public bl_SingleSettingsBinding friendlyFireSelector;

    public Button[] changeTeamButtons;
    private string lastRoom = "";

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnter;
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.JoinRoom += OnJoinRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnter;
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.JoinRoom -= OnJoinRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeTeam(int teamID)
    {
        Team newTeam = (Team)teamID;
        bl_WaitingRoom.Instance.JoinToTeam(newTeam);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OpenRoomSettings()
    {
        RoomSettingsButton.SetActive(false);
        if (PhotonNetwork.CurrentRoom.Name == lastRoom) return;

        CopySelector(bl_LobbyRoomCreatorUI.Instance.MapSettingsSelector, mapSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.GameModeSelector, gameModeSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.MaxPlayersSelector, maxPlayersSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.MaxPingSelector, pingSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.TimeLimitSelector, timeLimitSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.GameGoalSelector, goalSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.FriendlyFireSelector, friendlyFireSelector);
        CopySelector(bl_LobbyRoomCreatorUI.Instance.BotsActiveSelector, botsActiveSelector);

        lastRoom = PhotonNetwork.CurrentRoom.Name;
        var currentInfo = PhotonNetwork.CurrentRoom.GetRoomInfo();
        mapSelector.SetCurrentFromOptionName(currentInfo.mapName);
        gameModeSelector.SetCurrentFromOptionName(currentInfo.gameMode.GetModeInfo().ModeName.Localized(currentInfo.gameMode.ToString().ToLower()));
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateChangeButtons()
    {
        if (isOneTeamMode) return;
        if(PhotonNetwork.PlayerList.Length >= PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            changeTeamButtons[0].gameObject.SetActive(false);
            changeTeamButtons[1].gameObject.SetActive(false);
            return;
        }

        Team t = PhotonNetwork.LocalPlayer.GetPlayerTeam();
        changeTeamButtons[0].gameObject.SetActive(t == Team.Team2);
        changeTeamButtons[1].gameObject.SetActive(t == Team.Team1);

        int oct1 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1).Length;
        int oct2 = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).Length;

        changeTeamButtons[0].interactable = (oct2 - oct1) >= 0;
        changeTeamButtons[1].interactable = (oct1 - oct2) >= 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConfirmRoomSettings()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        var gameMode = bl_Lobby.Instance.GameModes[gameModeSelector.currentOption];

        PhotonNetwork.CurrentRoom.MaxPlayers = (byte)gameMode.maxPlayers[maxPlayersSelector.currentOption];
        HashTable t = new HashTable();
        t.Add(PropertiesKeys.SceneNameKey, bl_GameData.Instance.AllScenes[mapSelector.currentOption].RealSceneName);
        if (GetGameModeUpdated != bl_Lobby.Instance.GameModes[gameModeSelector.currentOption].gameMode)
        {
            t.Add(PropertiesKeys.GameModeKey, bl_Lobby.Instance.GameModes[gameModeSelector.currentOption].gameMode.ToString());
        }
        t.Add(PropertiesKeys.TimeRoomKey, gameMode.timeLimits[timeLimitSelector.currentOption]);
        t.Add(PropertiesKeys.MaxPing, bl_Lobby.Instance.MaxPing[pingSelector.currentOption]);
        t.Add(PropertiesKeys.RoomFriendlyFire, friendlyFireSelector.currentOption == 1 ? true : false);
        t.Add(PropertiesKeys.WithBotsKey, gameMode.supportBots ? (botsActiveSelector.currentOption == 1 ? true : false) : false);
        t.Add(PropertiesKeys.RoomGoal, gameMode.GameGoalsOptions[goalSelector.currentOption]);

        PhotonNetwork.CurrentRoom.SetCustomProperties(t);
        RoomSettingsButton.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnGameModeChange(int id)
    {
        goalSelector.currentOption = 0;
        maxPlayersSelector.currentOption = 0;
        timeLimitSelector.currentOption = 0;

        var gameMode = bl_Lobby.Instance.GameModes[gameModeSelector.currentOption];

        maxPlayersSelector.SetOptions(gameMode.maxPlayers.AsStringArray($" Players"));
        timeLimitSelector.SetOptions(gameMode.timeLimits.Select(x => $"{(x / 60)} Minutes").ToArray());

        if (gameMode.GameGoalsOptions.Length > 0)
            goalSelector.SetOptions(gameMode.GameGoalsOptions.AsStringArray($" {gameMode.GoalName}"));
        else goalSelector.SetOptions(new string[1] { gameMode.GoalName });

        botsActiveSelector.gameObject.SetActive(gameMode.supportBots);
    }

    void OnPlayerEnter(Player newPlayer)
    {

    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PropertiesKeys.TeamKey))
        {
            UpdateChangeButtons();
        }
    }

    public void OnJoinRoom()
    {
        RoomSettingsButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    /// <summary>
    /// 
    /// </summary>
    void CopySelector(bl_SingleSettingsBinding source,bl_SingleSettingsBinding target)
    {
        target.optionsNames = source.optionsNames;
        target.currentOption = source.currentOption;
        target.ApplyCurrentValue();
    }
}