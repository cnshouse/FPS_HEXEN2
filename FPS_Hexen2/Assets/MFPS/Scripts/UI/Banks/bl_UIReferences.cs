using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Internal;
using MFPS.Runtime.UI.Layout;
using MFPS.Runtime.UI;
using MFPS.Runtime.UI.Bindings;
using MFPS.Internal.Interfaces;

public class bl_UIReferences : bl_PhotonHelper, IInRoomCallbacks
{
    public RoomMenuState State = RoomMenuState.Init;

    [FlagEnum, SerializeField] private RoomUILayers m_uiMask = 0;
    public RoomUILayers UIMask
    {
        get => m_uiMask;
        set => m_uiMask = value;
    }

    public IMFPSResumeScreen ResumeScreen { get; set; }

    [Header("References")]
    [SerializeField] private Text SpawnProtectionText;
    public bl_PlayerScoreboard playerScoreboards;
    public GameObject OptionsUI;
    [SerializeField] private GameObject LocalKillPrefab = null;
    [SerializeField] private Transform LocalKillPanel = null;
    [SerializeField] private Transform LeftNotifierPanel = null;
    [SerializeField] private RectTransform ScoreBoardPopUp = null;
    [SerializeField] private GameObject LeftNotifierPrefab = null;
    [SerializeField] private GameObject BottonMenu = null;
    [SerializeField] private GameObject TopMenu = null;
    [SerializeField] private GameObject SuicideButton = null;
    [SerializeField] private GameObject AutoTeamUI = null;
    [SerializeField] private GameObject SpectatorButton = null;
    [SerializeField] private GameObject PingUI = null;
    public GameObject JumpLadder;
    [SerializeField] private GameObject ChangeTeamButton = null;
    public GameObject pauseMenuRoot;
    [SerializeField] private Text RoomNameText = null;
    [SerializeField] private Text GameModeText = null;
    [SerializeField] private Text MaxPlayerText = null;
    [SerializeField] private Text MaxKillsText = null;
    [SerializeField] private Text WaitingPlayersText = null;
    [SerializeField] private Text RespawnCountText = null;
    public Text AFKText;
    public bl_CanvasGroupFader blackScreen;
    public bl_RoundFinishUI roundFinishUI = null;
    public bl_ConfirmationWindow leaveRoomConfirmation;
    [SerializeField] private bl_LocalKillUI LocalKillIndividual = null;
    [SerializeField] private bl_KillCamUI KillCamUI = null;
    public bl_SpectatorMode spectatorMode = null;

    private bool inTeam = false;
    private int MaxRoomPing = 2000;
    private bool startKickingByPing = false;
    private int ChangeTeamTimes = 0;
    public Player PlayerPopUp { get; set; }
#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 126, 22, 38, 32, 33, 34, 127 };
    private string[] LocaleStrings;
#endif

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.InRoom)
            return;

        PhotonNetwork.AddCallbackTarget(this);
        blackScreen.FadeOut(1);

        GetRoomInfo();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        SetUpUI();
        if (MaxRoomPing > 0)
        {
            InvokeRepeating(nameof(CheckPing), 5, 5);
        }
        bl_EventHandler.onLocalPlayerSpawn += OnPlayerSpawn;
        bl_EventHandler.onPickUpHealth += OnPicUpMedKit;
        bl_EventHandler.onLocalPlayerDeath += OnLocalPlayerDeath;
        bl_EventHandler.onRoundEnd += OnRoundFinish;
        bl_PhotonCallbacks.LeftRoom += OnLeftRoom;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpUI()
    {
#if LOCALIZATION
        if(LocaleStrings == null) LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
#endif
        playerScoreboards.gameObject.SetActive(true);
        OptionsUI.SetActive(false);
        spectatorMode?.SetActive(false);
        ChangeTeamButton.SetActive(false);
        pauseMenuRoot.SetActive(true);
        PlayerUI.SpeakerIcon.SetActive(false);
        ScoreBoardPopUp.gameObject.SetActive(false);
        SetUpJoinButtons();
        UpdateDisplayUI();

        if (bl_RoomMenu.Instance.isPlaying)
        {
            SuicideButton.SetActive(bl_RoomSettings.Instance.canSuicide);
        }
        else
        {
            TopMenu.SetActive(false);
            SuicideButton.SetActive(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            if (MaxKillsText != null)
            {
                int MaxKills = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.RoomGoal];
#if LOCALIZATION
                MaxKillsText.text = string.Format("{0} {1}", MaxKills, LocaleStrings[0]);
#else
            MaxKillsText.text = string.Format("{0} KILLS", MaxKills);
#endif
            }
        }
#if LMS
        if (GetGameMode == GameMode.BR)
        {
            ShowMenu(false);
        }
#endif
        if (bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.WaitingRoom)
        {
            if (PhotonNetwork.OfflineMode && !CurrentRoom.GetRoomInfo().autoTeamSelection) return;

            SetUpJoinButtons(true);
            SpectatorButton.SetActive(false);
            ShowMenu(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUpJoinButtons(bool forced = false)
    {
        if (!inTeam && !bl_RoomSettings.Instance.AutoTeamSelection || forced)
        {
            playerScoreboards.SetActiveByTeamMode(true);
        }
        else { playerScoreboards.SetActiveJoinButtons(false); }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
        bl_EventHandler.onLocalPlayerSpawn -= OnPlayerSpawn;
        bl_EventHandler.onPickUpHealth -= OnPicUpMedKit;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalPlayerDeath;
        bl_EventHandler.onRoundEnd -= OnRoundFinish;
        bl_PhotonCallbacks.LeftRoom -= OnLeftRoom;
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalPlayerDeath()
    {
        JumpLadder.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnRoundFinish()
    {
        playerScoreboards.SetActiveByTeamMode();
        StopAllCoroutines();
        blackScreen.SetAlpha(0);
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckPing()
    {
        int ping = PhotonNetwork.GetPing();
        if (ping >= MaxRoomPing)
        {
            PingUI.SetActive(true);
            if (!startKickingByPing) { Invoke(nameof(StartPingKick), 11); startKickingByPing = true; }
        }
        else
        {
            PingUI.SetActive(false);
            if (startKickingByPing) { CancelInvoke(nameof(StartPingKick)); startKickingByPing = false; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GetRoomInfo()
    {
        GameMode mode = GetGameMode;
        RoomNameText.text = PhotonNetwork.CurrentRoom.Name.ToUpper();
        GameModeText.text = mode.GetName().ToUpper();
        int vs = (!isOneTeamMode) ? PhotonNetwork.CurrentRoom.MaxPlayers / 2 : PhotonNetwork.CurrentRoom.MaxPlayers - 1;
        MaxPlayerText.text = (!isOneTeamMode) ? string.Format("{0} VS {1}", vs, vs) : string.Format("1 VS {0}", vs);
        MaxRoomPing = (int)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.MaxPing];
    }

    /// <summary>
    /// Local notification when kill someone
    /// </summary>
    public void SetLocalKillFeed(KillInfo info, bl_KillFeed.LocalKillDisplay display)
    {
        if (display == bl_KillFeed.LocalKillDisplay.List)
        {
            if (info.byHeadShot)
            {
                GameObject newkillfeedh = Instantiate(LocalKillPrefab) as GameObject;
                newkillfeedh.GetComponent<bl_LocalKillUI>().InitMultiple(info, true);
                newkillfeedh.transform.SetParent(LocalKillPanel, false);
                newkillfeedh.transform.SetAsFirstSibling();
            }
            GameObject newkillfeed = Instantiate(LocalKillPrefab) as GameObject;
            newkillfeed.GetComponent<bl_LocalKillUI>().InitMultiple(info, false);
            newkillfeed.transform.SetParent(LocalKillPanel, false);
            newkillfeed.transform.SetAsFirstSibling();
        }
        else
        {
            LocalKillIndividual.InitIndividual(info);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowMenu(bool active)
    {
        playerScoreboards.SetActive(active);
        BottonMenu.SetActive(active);
        TopMenu.SetActive(active);
        if (bl_GameData.Instance.CanChangeTeam && !isOneTeamMode && ChangeTeamTimes <= bl_GameData.Instance.MaxChangeTeamTimes && !bl_RoomSettings.Instance.AutoTeamSelection && bl_MatchTimeManager.Instance.TimeState == RoomTimeState.Started)
        {
#if BDGM
            if(GetGameMode != GameMode.SND)
            {
                ChangeTeamButton.SetActive(true);
            }
#else
            ChangeTeamButton.SetActive(true);
#endif
        }
        if (active)
        {
            if (bl_RoomMenu.Instance.isPlaying)
            {
                SuicideButton.SetActive(bl_RoomSettings.Instance.canSuicide);
            }
            else
            {
                SuicideButton.SetActive(false);
            }
        }
        else
        {
            OptionsUI.SetActive(false);
            ScoreBoardPopUp.gameObject.SetActive(false);
#if PSELECTOR
            if (bl_PlayerSelector.Instance != null)
            {
                bl_PlayerSelector.Instance.isChangeOfTeam = false;
            }
#endif
        }
#if INPUT_MANAGER
        if (bl_Input.isGamePad)
        {
            MFPS.InputManager.bl_GamePadPointer.Instance?.SetActive(active);
        }
#endif
        State = (active) ? RoomMenuState.Full : RoomMenuState.Hidde;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetUIMask(RoomUILayers newMask)
    {
        UIMask = newMask;
        UpdateDisplayUI();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Resume()
    {
        bl_UtilityHelper.LockCursor(true);
        ShowMenu(false);
        State = RoomMenuState.Hidde;
        bl_UCrosshair.Instance.Show(true);
        ScoreBoardPopUp.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveOptions()
    {
        playerScoreboards.SetActive(false);
        OptionsUI.SetActive(true);
        State = RoomMenuState.Options;
        ScoreBoardPopUp.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveScoreboard()
    {
        playerScoreboards.SetActive(true);
        OptionsUI.SetActive(false);
        State = RoomMenuState.Scoreboard;
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateDisplayUI()
    {
        PlayerUI?.UpdateUIDisplay();
        if (playerScoreboards != null)
        {
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards));
            playerScoreboards.BlockScoreboards = !UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards);
            playerScoreboards.SetActive(UIMask.IsEnumFlagPresent(RoomUILayers.Scoreboards));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoTeam(bool v)
    {
        AutoTeamUI.SetActive(v);
        if (!v)
        {
            playerScoreboards.SetActive(false);
            BottonMenu.SetActive(false);
            spectatorMode?.SetActive(false);
            SpectatorButton.SetActive(false);
            inTeam = true;
            State = RoomMenuState.Hidde;
        }
        else
        {
            if (PhotonNetwork.OfflineMode)
            {
                AutoTeamUI.GetComponentInChildren<Text>().text = bl_GameTexts.StartingOfflineRoom;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LeftRoom(bool quit)
    {
        if (!quit)
        {
            bl_UtilityHelper.LockCursor(false);
            leaveRoomConfirmation.AskConfirmation("", () => { LeftRoom(true); }, () => { bl_UtilityHelper.LockCursor(true); });
        }
        else
        {
#if ULSP
            if (bl_DataBase.Instance != null)
            {
                Player p = PhotonNetwork.LocalPlayer;
                bl_ULoginMFPS.SaveLocalPlayerKDS();
                bl_DataBase.Instance.StopAndSaveTime();
            }
#endif
            blackScreen.FadeIn(1);
            PhotonNetwork.LeaveRoom();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Suicide()
    {
        bl_MFPS.LocalPlayer.Suicide();
        bl_UtilityHelper.LockCursor(false);
        ShowMenu(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnKillCam(bool active, string killer = "", int gunID = 0)
    {
        if (active)
        {
            KillCamUI.Show(killer, gunID);
            StartCoroutine(RespawnCountDown());
        }
        KillCamUI.gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator RespawnCountDown()
    {
        float d = 0;
        float rt = bl_GameData.Instance.PlayerRespawnTime;
        while (d < 1)
        {
            d += Time.deltaTime / rt;
#if LOCALIZATION
            RespawnCountText.text = string.Format(LocaleStrings[2], Mathf.FloorToInt(rt * (1 - d)));
#else
            RespawnCountText.text = string.Format(bl_GameTexts.RespawnIn, Mathf.FloorToInt( rt * (1 - d)));
#endif
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    public void JoinTeam(int id)
    {
#if ELIM
        if(GetGameMode == GameMode.ELIM)
        {
            if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
            {
                bl_Elimination.Instance.WaitForRoundFinish();
            }
        }
#endif
        bl_RoomMenu.Instance.JoinTeam(id);
        playerScoreboards.SetActive(false);
        BottonMenu.SetActive(false);
        inTeam = true;
        State = RoomMenuState.Hidde;
        AutoTeamUI.SetActive(false);
        spectatorMode?.SetActive(false);
        SpectatorButton.SetActive(false);
        TopMenu.SetActive(false);
        if (bl_GameManager.Joined) { ChangeTeamTimes++; }
        if (bl_GameData.Instance.CanChangeTeam && !isOneTeamMode && ChangeTeamTimes <= bl_GameData.Instance.MaxChangeTeamTimes && bl_MatchTimeManager.Instance.TimeState == RoomTimeState.Started)
        {
            ChangeTeamButton.SetActive(true);
        }
    }

    public void ActiveChangeTeam()
    {
#if PSELECTOR
        if (bl_PlayerSelector.Instance != null)
        {
            bl_PlayerSelector.Instance.isChangeOfTeam = true;
        }
#endif
        playerScoreboards.SetActive(false, true);
        ChangeTeamButton.SetActive(false);
        TopMenu.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public void ShowScoreboard(bool active)
    {
        playerScoreboards.SetActive(active);
        State = (active) ? RoomMenuState.Scoreboard : RoomMenuState.Hidde;
    }

    public void OnSpawnCount(int count)
    {
#if LOCALIZATION
        SpawnProtectionText.text = string.Format(LocaleStrings[6].ToUpper(), count);
#else
        SpawnProtectionText.text = string.Format("SPAWN PROTECTION DISABLE IN: <b>{0}</b>", count);
#endif
        SpawnProtectionText.gameObject.SetActive(count > 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SpectatorMode(bool active)
    {
        if (spectatorMode == null) return;

        spectatorMode?.SetSpectatorMode(active);
        playerScoreboards.SetActive(!active);
        BottonMenu.SetActive(!active);
    }

    public void OpenScoreboardPopUp(bool active, Player player = null)
    {
        PlayerPopUp = player;
        if (active)
        {
            ScoreBoardPopUp.gameObject.SetActive(true);
            ScoreBoardPopUp.position = Input.mousePosition;
        }
        else
        {
            ScoreBoardPopUp.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="e"></param>
    void OnPlayerSpawn()
    {
        if (KillCamUI != null) KillCamUI.gameObject.SetActive(false);
#if INPUT_MANAGER
        if (bl_Input.isGamePad)
        {
            MFPS.InputManager.bl_GamePadPointer.Instance?.SetActive(false);
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t_amount"></param>
    void OnPicUpMedKit(int Amount)
    {
        AddLeftNotifier(string.Format("+{0} Health", Amount));
    }

    void OnLeftNotifier(string _Text)
    {
        AddLeftNotifier(_Text);
    }

    public void AddLeftNotifier(string text)
    {
        GameObject nn = Instantiate(LeftNotifierPrefab) as GameObject;
        nn.GetComponent<bl_UILeftNotifier>().SetInfo(text.ToUpper(), 7);
        nn.transform.SetParent(LeftNotifierPanel, false);
    }

    public void SetWaitingPlayersText(string text = "", bool show = false)
    {
        WaitingPlayersText.text = text;
        WaitingPlayersText.transform.parent.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetRound()
    {
        if (roundFinishUI) roundFinishUI.Hide();
        inTeam = false;
        blackScreen.FadeOut(1);
        SetUpUI();
        ShowMenu(true);
        if (!bl_RoomSettings.Instance.AutoTeamSelection)
        {
            playerScoreboards.ResetJoinButtons();
            SetUpJoinButtons();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void StartPingKick()
    {
        bl_PhotonNetwork.Instance.OnPingKick();
        PhotonNetwork.LeaveRoom();
    }

    public void SetActiveTimeUI(bool active)
    {
        PlayerUI.TimeUIRoot.SetActive(active);
    }

    public void SetActiveMaxKillsUI(bool active) => PlayerUI.MaxKillsUIRoot.SetActive(active);
    public void SetFinalText(string winner) => roundFinishUI.Show(winner);
    public void SetCountDown(int count) => roundFinishUI.SetCountDown(count);

    public IEnumerator FinalFade(bool fadein, bool goToLobby = true, float delay = 1)
    {
        if (!goToLobby)
        {
            if (bl_RoomMenu.Instance.isFinish) { blackScreen.SetAlpha(0); yield break; }
        }

        if (fadein)
        {
            yield return new WaitForSeconds(delay);
            yield return blackScreen.FadeIn(1);
#if ULSP
            if (bl_DataBase.Instance != null && bl_DataBase.Instance.IsRunningTask)
            {
                while (bl_DataBase.Instance.IsRunningTask) { yield return null; }
            }
#endif
            if (goToLobby)
            {
                bl_UtilityHelper.LoadLevel(bl_GameData.Instance.MainMenuScene);
            }
        }
        else
        {
            yield return new WaitForSeconds(delay);
            blackScreen.FadeOut(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isMenuActive => !(State == RoomMenuState.Hidde);

    /// <summary>
    /// 
    /// </summary>
    public bool isOnlyMenuActive => (State == RoomMenuState.Options);

    /// <summary>
    /// 
    /// </summary>
    public bool isScoreboardActive => (State == RoomMenuState.Scoreboard);


    public void OnLeftRoom()
    {
        ShowMenu(false);
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
    }

    #region Classes
    [System.Serializable]
    public enum RoomMenuState
    {
        Scoreboard = 0,
        Options = 1,
        Full = 2,
        Hidde = 3,
        Init = 4,
    }

    private static bl_UIReferences _instance;
    public static bl_UIReferences Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_UIReferences>(); }
            return _instance;
        }
    }

    private bl_PlayerUIBank _PlayerBank;
    public bl_PlayerUIBank PlayerUI
    {
        get
        {
            if (_PlayerBank == null) { _PlayerBank = transform.GetComponentInChildren<bl_PlayerUIBank>(true); }
            return _PlayerBank;
        }
    }
    #endregion
}