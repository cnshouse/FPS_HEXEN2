using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;
using MFPS.Audio;

[DefaultExecutionOrder(-998)]
public class bl_Lobby : bl_PhotonHelper, IConnectionCallbacks, ILobbyCallbacks, IMatchmakingCallbacks
{
    #region Public members
    [Header("Photon")]
    public SeverRegionCode DefaultServer = SeverRegionCode.usw;
    [LovattoToogle] public bool ShowPhotonStatistics;

    [Header("Room Options")]
    //Room Max Ping
    public int[] MaxPing = new int[] { 100, 200, 500, 1000 };
    [Header("References")]
    [SerializeField] private GameObject PhotonGamePrefab; 
    #endregion

    #region Public properties
    public int CurrentMaxPing { get; set; }
    public GameModeSettings[] GameModes { get; set; }
    public bool rememberMe { get; set; }
    public string justCreatedRoomName { get; set; }
    public string CachePlayerName { get; set;}
    public Action onShowMenu;
    #endregion

    #region Private members
    private RoomInfo checkingRoom;
    private int PendingRegion = -1;
    private bool FirstConnectionMade = false;
    private bool AppQuit = false;
    private bool isSeekingMatch = false;
    bool alreadyLoadHome = false;
    private string playerName;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
#if ULSP
        if (bl_DataBase.Instance == null && bl_LoginProDataBase.Instance.ForceLoginScene)
        {
            bl_UtilityHelper.LoadLevel("Login");
            return;
        }
#endif
        SetupPhotonSettings();
        bl_UtilityHelper.BlockCursorForUser = false;
        bl_UtilityHelper.LockCursor(false);

        StartCoroutine(StartFade());//show loading screen

        if(bl_AudioController.Instance != null) { bl_AudioController.Instance.PlayBackground(); }
        if (bl_GameData.isDataCached)
        {
            SetUpGameModes();
            bl_LobbyUI.Instance.LoadSettings();
            bl_LobbyUI.Instance.FullSetUp();
        }
        bl_LobbyUI.Instance.InitialSetup();
    }

    /// <summary>
    /// 
    /// </summary>
    void SetupPhotonSettings()
    {
        PhotonNetwork.AddCallbackTarget(this);
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.IsMessageQueueRunning = true;
        if (bl_PhotonNetwork.Instance == null) { Instantiate(PhotonGamePrefab); }
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpGameModes()
    {
        List<GameModeSettings> gm = new List<GameModeSettings>();
        for (int i = 0; i < bl_GameData.Instance.gameModes.Count; i++)
        {
            if (bl_GameData.Instance.gameModes[i].isEnabled)
            {
                gm.Add(bl_GameData.Instance.gameModes[i]);
            }
        }
        GameModes = gm.ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ConnectPhoton()
    {
        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.NickName);
            //if we don't have a custom region to connect or we are using a self hosted server
            if (PendingRegion == -1 || !PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
            {
                //connect using the default PhotonServerSettings
                PhotonNetwork.GameVersion = bl_GameData.Instance.GameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
            else
            {
                //Change the cloud server region
                FirstConnectionMade = true;
                ChangeServerCloud(PendingRegion);
            }
#if LOCALIZATION
            bl_LobbyUI.Instance.LoadingScreenText.text = bl_Localization.Instance.GetText(40);
#else
            bl_LobbyUI.Instance.LoadingScreenText.text = bl_GameTexts.ConnectingToGameServer;
#endif
            StartCoroutine(ShowLoadingScreen());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Disconect()
    {
        SetLobbyChat(false);
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }
    void DelayDisconnect() { PhotonNetwork.Disconnect(); }

    /// <summary>
    /// 
    /// </summary>
    private void OnApplicationQuit()
    {
        AppQuit = true;
        bl_GameData.isDataCached = false;
        Disconect();
    }

    /// <summary>
    /// 
    /// </summary>
    public void ServerList(List<RoomInfo> roomList)
    {
        bl_LobbyUI.Instance.SetRoomList(roomList);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetPlayerName(string InputName)
    {
        CachePlayerName = InputName;       
        playerName = CachePlayerName;
        playerName = playerName.Replace("\n", "");
        PlayerPrefs.SetString(PropertiesKeys.PlayerName, playerName);
        PhotonNetwork.NickName = playerName;
        ConnectPhoton();
        if (rememberMe)
        {
            PlayerPrefs.SetString(PropertiesKeys.RememberMe, playerName);
        }
        //load the user coins
        //NOTE: Coins are store locally, so is highly recommended to store in a database, you can use ULogin for it.
#if !ULSP
        bl_GameData.Instance.VirtualCoins.LoadCoins(PhotonNetwork.NickName);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public bool EnterPassword(string password)
    {
        if (bl_GameData.Instance.CheckPasswordUse(CachePlayerName, password))
        {
            playerName = CachePlayerName;
            playerName = playerName.Replace("\n", "");
            PhotonNetwork.NickName = playerName;
            ConnectPhoton();
            if (rememberMe)
            {
                PlayerPrefs.SetString(PropertiesKeys.RememberMe, playerName);
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void AutoMatch()
    {
        if (isSeekingMatch)
            return;

        isSeekingMatch = true;
        StartCoroutine(AutoMatchIE());
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator AutoMatchIE()
    {
        //active the search match UI
        bl_LobbyUI.Instance.SeekingMatchUI.SetActive(true);
        yield return new WaitForSeconds(3);
        PhotonNetwork.JoinRandomRoom();
        isSeekingMatch = false;
        bl_LobbyUI.Instance.SeekingMatchUI.SetActive(false);
    }

    /// <summary>
    /// When there is not rooms to join (when matchmaking)
    /// </summary>
    public void OnNoRoomsToJoin(short returnCode, string message)
    {
        Debug.Log("No games to join found on matchmaking, creating one.");
        //create random room properties
        int propsCount = 11;
        string roomName = string.Format("[PUBLIC] {0}{1}", PhotonNetwork.NickName.Substring(0, 2), Random.Range(0, 9999));
        justCreatedRoomName = roomName;
        int modeRandom = Random.Range(0, GameModes.Length);
        var gameMode = GameModes[modeRandom];

        int scid = Random.Range(0, bl_GameData.Instance.AllScenes.Count);
        int maxPlayersRandom = Random.Range(0, gameMode.maxPlayers.Length);
        int timeRandom = Random.Range(0, gameMode.timeLimits.Length);
        int randomGoal = Random.Range(0, GameModes[modeRandom].GameGoalsOptions.Length);

        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = gameMode.timeLimits[timeRandom];
        roomOption[PropertiesKeys.GameModeKey] = gameMode.gameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = bl_GameData.Instance.AllScenes[scid].RealSceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = RoundStyle.OneMacht;
        roomOption[PropertiesKeys.TeamSelectionKey] = gameMode.AutoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = bl_GameData.Instance.AllScenes[scid].ShowName;
        roomOption[PropertiesKeys.RoomGoal] = gameMode.GetGoalValue(randomGoal);
        roomOption[PropertiesKeys.RoomFriendlyFire] = false;
        roomOption[PropertiesKeys.MaxPing] = MaxPing[CurrentMaxPing];
        roomOption[PropertiesKeys.RoomPassword] = string.Empty;
        roomOption[PropertiesKeys.WithBotsKey] = gameMode.supportBots;

        string[] properties = new string[propsCount];
        properties[0] = PropertiesKeys.TimeRoomKey;
        properties[1] = PropertiesKeys.GameModeKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomRoundKey;
        properties[4] = PropertiesKeys.TeamSelectionKey;
        properties[5] = PropertiesKeys.CustomSceneName;
        properties[6] = PropertiesKeys.RoomGoal;
        properties[7] = PropertiesKeys.RoomFriendlyFire;
        properties[8] = PropertiesKeys.MaxPing;
        properties[9] = PropertiesKeys.RoomPassword;
        properties[10] = PropertiesKeys.WithBotsKey;

        PhotonNetwork.CreateRoom(roomName, new RoomOptions()
        {
            MaxPlayers = (byte)gameMode.maxPlayers[maxPlayersRandom],
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = properties,
            BroadcastPropsChangeToAll = true,

        }, null);
        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.StopBackground(); }
    }


    /// <summary>
    /// 
    /// </summary>
    public void CreateRoom()
    {
        SetLobbyChat(false);
        int propsCount = 11;
        PhotonNetwork.NickName = playerName;

        var roomInfo = bl_LobbyRoomCreator.Instance.BuildRoomInfo();
        justCreatedRoomName = roomInfo.roomName;
        //Save Room properties for load in room
        ExitGames.Client.Photon.Hashtable roomOption = new ExitGames.Client.Photon.Hashtable();
        roomOption[PropertiesKeys.TimeRoomKey] = roomInfo.time;
        roomOption[PropertiesKeys.GameModeKey] = roomInfo.gameMode.ToString();
        roomOption[PropertiesKeys.SceneNameKey] = roomInfo.sceneName;
        roomOption[PropertiesKeys.RoomRoundKey] = roomInfo.roundStyle;
        roomOption[PropertiesKeys.TeamSelectionKey] = roomInfo.autoTeamSelection;
        roomOption[PropertiesKeys.CustomSceneName] = roomInfo.mapName;
        roomOption[PropertiesKeys.RoomGoal] = roomInfo.goal;
        roomOption[PropertiesKeys.RoomFriendlyFire] = roomInfo.friendlyFire;
        roomOption[PropertiesKeys.MaxPing] = roomInfo.maxPing;
        roomOption[PropertiesKeys.RoomPassword] = roomInfo.password;
        roomOption[PropertiesKeys.WithBotsKey] = roomInfo.withBots;

        string[] properties = new string[propsCount];
        properties[0] = PropertiesKeys.TimeRoomKey;
        properties[1] = PropertiesKeys.GameModeKey;
        properties[2] = PropertiesKeys.SceneNameKey;
        properties[3] = PropertiesKeys.RoomRoundKey;
        properties[4] = PropertiesKeys.TeamSelectionKey;
        properties[5] = PropertiesKeys.CustomSceneName;
        properties[6] = PropertiesKeys.RoomGoal;
        properties[7] = PropertiesKeys.RoomFriendlyFire;
        properties[8] = PropertiesKeys.MaxPing;
        properties[9] = PropertiesKeys.RoomPassword;
        properties[10] = PropertiesKeys.WithBotsKey;

        PhotonNetwork.CreateRoom(roomInfo.roomName, new RoomOptions()
        {
            MaxPlayers = (byte)roomInfo.maxPlayers,
            IsVisible = true,
            IsOpen = true,
            CustomRoomProperties = roomOption,
            CleanupCacheOnLeave = true,
            CustomRoomPropertiesForLobby = properties,
            PublishUserId = true,
            EmptyRoomTtl = 0,
            BroadcastPropsChangeToAll = true,
        }, null);
        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.3f);
        if (bl_AudioController.Instance != null) { bl_AudioController.Instance.StopBackground(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        PlayerPrefs.SetString(PropertiesKeys.RememberMe, string.Empty);
        Disconect();
        bl_LobbyUI.Instance.ChangeWindow("player name");
#if ULSP
        bl_LoginProDataBase.Instance.DeleteRememberCredentials();
        if (bl_DataBase.Instance != null) bl_DataBase.Instance.LocalUser = new MFPS.ULogin.LoginUserInfo();
        if (bl_PhotonNetwork.Instance != null) bl_PhotonNetwork.LocalPlayer.NickName = string.Empty;
        bl_UtilityHelper.LoadLevel("Login");
#endif
    }

    #region UGUI
    public void SetRememberMe(bool value)
    {
        rememberMe = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public void CheckRoomPassword(RoomInfo room)
    {
        checkingRoom = room;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool SetRoomPassworld(string pass)
    {
        if (checkingRoom == null)
        {
            Debug.Log("Checking room is not assigned more!");
            return false;
        }

        if ((string)checkingRoom.CustomProperties[PropertiesKeys.RoomPassword] == pass && checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
        {
            if (PhotonNetwork.GetPing() < (int)checkingRoom.CustomProperties[PropertiesKeys.MaxPing])
            {
                bl_LobbyUI.Instance.blackScreenFader.FadeIn(1);
                if (checkingRoom.PlayerCount < checkingRoom.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(checkingRoom.Name);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool serverchangeRequested = false;
    public void ChangeServerCloud(int id)
    {
        if (PhotonNetwork.IsConnected && FirstConnectionMade)
        {
            serverchangeRequested = true;
#if LOCALIZATION
            bl_LobbyUI.Instance.LoadingScreenText.text = bl_Localization.Instance.GetText(40);
#else
            bl_LobbyUI.Instance.LoadingScreenText.text = bl_GameTexts.ConnectingToGameServer;
#endif
            StartCoroutine(ShowLoadingScreen());
            PendingRegion = id;
            Invoke("DelayDisconnect", 0.2f);
            return;
        }
        if (!string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime))
        {
            if (!FirstConnectionMade)
            {
                PendingRegion = id;
                serverchangeRequested = true;
                return;
            }
            serverchangeRequested = false;
            SeverRegionCode code = SeverRegionCode.usw;
            if (id > 3) { id++; }
            code = (SeverRegionCode)id;
            PhotonNetwork.NetworkingClient.AppId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime;
            PhotonNetwork.ConnectToRegion(code.ToString());
            PlayerPrefs.SetString(PropertiesKeys.PreferredRegion, code.ToString());
        }
        else
        {
            Debug.LogWarning("Need your AppId for change server, please add it in PhotonServerSettings");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadLocalLevel(string level)
    {
        bl_LobbyUI.Instance.blackScreenFader.FadeIn(0.75f, () =>
         {
             bl_UtilityHelper.LoadLevel(level);
         });
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    void GetPlayerName()
    {
        bool isNameEmpty = string.IsNullOrEmpty(PhotonNetwork.NickName);
        if (isNameEmpty)
        {
#if ULSP
            if (bl_DataBase.Instance != null && !bl_DataBase.Instance.isGuest)
            {
                playerName = bl_DataBase.Instance.LocalUser.NickName;
                PhotonNetwork.NickName = playerName;
                bl_LobbyUI.Instance.UpdateCoinsText();
                GoToMainMenu();
            }
            else
            {
                GeneratePlayerName();
            }
#else
            GeneratePlayerName();
#endif
        }
        else
        {
            bl_LobbyUI.Instance.UpdateCoinsText();
            playerName = PhotonNetwork.NickName;
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GeneratePlayerName()
    {
        if (!rememberMe)
        {
            if (!PlayerPrefs.HasKey(PropertiesKeys.PlayerName) || !bl_GameData.Instance.RememberPlayerName)
            {
                playerName = string.Format(bl_GameData.Instance.guestNameFormat, Random.Range(1, 9999));
            }
            else if (bl_GameData.Instance.RememberPlayerName)
            {
                playerName = PlayerPrefs.GetString(PropertiesKeys.PlayerName, string.Format(bl_GameData.Instance.guestNameFormat, Random.Range(1, 9999)));
            }
            bl_LobbyUI.Instance.PlayerNameField.text = playerName;
            PhotonNetwork.NickName = playerName;
            bl_LobbyUI.Instance.ChangeWindow("player name");
        }
        else
        {
            playerName = PlayerPrefs.GetString(PropertiesKeys.RememberMe);
            if (string.IsNullOrEmpty(playerName))
            {
                rememberMe = false;
                GeneratePlayerName();
                return;
            }
            PhotonNetwork.NickName = playerName;
            GoToMainMenu();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void GoToMainMenu()
    {
        if (!PhotonNetwork.IsConnected)
        {
            ConnectPhoton();
        }
        else
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                if (!alreadyLoadHome) { bl_LobbyUI.Instance.Home(); alreadyLoadHome = true; }
                SetLobbyChat(true);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private IEnumerator MoveToGameScene()
    {
        //Wait for check
        SetLobbyChat(false);
        while (!PhotonNetwork.InRoom)
        {
            yield return null;
        }
        PhotonNetwork.IsMessageQueueRunning = false;
        bl_UtilityHelper.LoadLevel((string)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.SceneNameKey]);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowLevelList()
    {
#if LM
        var lp = FindObjectOfType<MFPS.Addon.LevelManager.bl_LevelPreview>();
        if (lp != null)
        {
            lp.ShowList();
        }
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator StartFade()
    {
        bl_LobbyUI.Instance.LoadingScreen.gameObject.SetActive(true);
#if LOCALIZATION
         bl_LobbyUI.Instance.LoadingScreenText.text = bl_Localization.Instance.GetText(39);
#else
        bl_LobbyUI.Instance.LoadingScreenText.text = bl_GameTexts.LoadingLocalContent;
#endif
        if (!bl_GameData.isDataCached)
        {
            yield return StartCoroutine(bl_GameData.AsyncLoadData());
            yield return new WaitForEndOfFrame();
            SetUpGameModes();
            bl_LobbyUI.Instance.LoadSettings();
            bl_LobbyUI.Instance.FullSetUp();
        }
        yield return new WaitForSeconds(0.5f);
        yield return bl_LobbyUI.Instance.blackScreenFader.FadeOut(1);
        yield return StartCoroutine(ShowLoadingScreen(true, 2));
        GetPlayerName();
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator ShowLoadingScreen(bool autoHide = false, float showTime = 2)
    {
        bl_LobbyUI.Instance.LoadingScreen.gameObject.SetActive(true);
        bl_LobbyUI.Instance.LoadingScreen.alpha = 1;
        Animator bottomAnim = bl_LobbyUI.Instance.LoadingScreen.GetComponentInChildren<Animator>();
        bottomAnim.SetBool("show", true);
        bottomAnim.Play("show", 0, 0);
        if (autoHide)
        {
            yield return new WaitForSeconds(showTime);
            float d = 1;
            bottomAnim.SetBool("show", false);
            while (d > 0)
            {
                d -= Time.deltaTime / 0.5f;
                bl_LobbyUI.Instance.LoadingScreen.alpha = bl_LobbyUI.Instance.blackScreenFader.fadeCurve.Evaluate(d);
                yield return new WaitForEndOfFrame();
            }
            bl_LobbyUI.Instance.LoadingScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SetLobbyChat(bool connect)
    {
        if (bl_LobbyChat.Instance == null) return;
        if (!bl_GameData.Instance.UseLobbyChat) return;

        if(connect) { bl_LobbyChat.Instance.Connect(bl_GameData.Instance.GameVersion); }
        else { bl_LobbyChat.Instance.Disconnect(); }
    }

    #region Photon Callbacks
    /// <summary>
    /// 
    /// </summary>
    public void OnConnected()
    {
        FirstConnectionMade = true;
        if (PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer)
        {
            Debug.Log("Server connection established to: " + PhotonNetwork.CloudRegion);
        }
        else
        {
            Debug.Log($"Server connection established to: {PhotonNetwork.ServerAddress} ({PhotonNetwork.Server.ToString()})");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if (cause == DisconnectCause.DisconnectByClientLogic)
        {
            if (AppQuit)
            {
                Debug.Log("Disconnect from Server!");
                return;
            }
            if (PendingRegion == -1)
            {
                Debug.Log("Disconnect from cloud!");
            }
            else if (serverchangeRequested)
            {
                Debug.Log("Changing server!");
                ChangeServerCloud(PendingRegion);
            }
            else
            {
                Debug.Log("Disconnect from Server.");
            }
        }
        else
        {
            bl_LobbyUI.Instance.blackScreenFader.FadeOut(1);
#if LOCALIZATION
           bl_LobbyUI.Instance. DisconnectCauseUI.GetComponentInChildren<Text>().text = string.Format(bl_Localization.Instance.GetText(41), cause.ToString());
#else
            bl_LobbyUI.Instance.DisconnectCauseUI.GetComponentInChildren<Text>().text = string.Format(bl_GameTexts.DisconnectCause, cause.ToString());
#endif
            bl_LobbyUI.Instance.DisconnectCauseUI.SetActive(true);
            Debug.LogWarning("Failed to connect to server, cause: " + cause);
        }
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room list updated, total rooms: " + roomList.Count);
        ServerList(roomList);
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {

    }

    public void OnJoinedLobby()
    {
        Debug.Log($"Player <b>{PhotonNetwork.LocalPlayer.UserId}</b> joined to the lobby");
        bl_LobbyUI.Instance.Home();
        if (PendingRegion != -1) { }
        StartCoroutine(ShowLoadingScreen(true, 2));

        SetLobbyChat(true);
        ResetValues();
    }

    public void ResetValues()
    {
        if(bl_LobbyRoomCreatorUI.Instance == null) { bl_LobbyRoomCreatorUI.Instance = transform.GetComponentInChildren<bl_LobbyRoomCreatorUI>(true); }
        //Create a random name for a future room that player create
        bl_LobbyRoomCreatorUI.Instance.SetupSelectors();
        PhotonNetwork.IsMessageQueueRunning = true; 
    }

    public void OnJoinedRoom()
    {
        if (bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.DirectToMap)
        {
            Debug.Log($"Local client joined to the room '{PhotonNetwork.CurrentRoom.Name}'");
            StartCoroutine(MoveToGameScene());
        }
        else
        {
            SetLobbyChat(false);
        }
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {

    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
    }

    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
    }

    public void OnLeftLobby()
    {
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
    }

    public void OnCreatedRoom()
    {
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        OnNoRoomsToJoin(returnCode, message);
    }

    public void OnLeftRoom()
    {
    }
    #endregion

    public AddonsButtonsHelper AddonsButtons = new AddonsButtonsHelper();
    public class AddonsButtonsHelper
    {
        public GameObject this[int index]
        {
            get
            {
                return bl_LobbyUI.Instance.AddonsButtons[index];
            }
        }
    }

    private static bl_Lobby _instance;
    public static bl_Lobby Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<bl_Lobby>();
            }
            return _instance;
        }
    }
}