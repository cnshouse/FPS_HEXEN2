using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Object = UnityEngine.Object;
using MFPSEditor;
using UnityEngine.Serialization;
using MFPS.Runtime.Settings;
using MFPS.Internal.Structures;

public class bl_GameData : ScriptableObject
{
    [Header("Game Settings")]
    [LovattoToogle] public bool offlineMode = false;
    [LovattoToogle] public bool UseLobbyChat = true;
    [LovattoToogle] public bool UseVoiceChat = true;
    [LovattoToogle] public bool BulletTracer = false;
    [LovattoToogle] public bool DropGunOnDeath = true;
    [LovattoToogle] public bool SelfGrenadeDamage = true;
    [LovattoToogle] public bool CanFireWhileRunning = true;
    [LovattoToogle] public bool HealthRegeneration = true;
    [LovattoToogle] public bool ShowTeamMateHealthBar = true;
    [LovattoToogle] public bool CanChangeTeam = false;
    [LovattoToogle] public bool ShowBlood = true;
    [LovattoToogle] public bool DetectAFK = false;
    [LovattoToogle] public bool MasterCanKickPlayers = true;
    [LovattoToogle] public bool ArriveKitsCauseDamage = true;
    [LovattoToogle] public bool CalculateNetworkFootSteps = false;
    [LovattoToogle] public bool ShowNetworkStats = false;
    [LovattoToogle] public bool RememberPlayerName = true;
    [LovattoToogle] public bool ShowWeaponLoadout = true;
    [LovattoToogle] public bool useCountDownOnStart = true;
    [LovattoToogle] public bool showCrosshair = true;
    [LovattoToogle] public bool showDamageIndicator = true;
    [LovattoToogle] public bool doSpawnHandMeshEffect = true;
    [LovattoToogle] public bool playerCameraWiggle = true;
#if MFPSM
    [LovattoToogle] public bool AutoWeaponFire = false;
#endif
#if LM
    [LovattoToogle] public bool LockWeaponsByLevel = true;
#endif
    public AmmunitionType AmmoType = AmmunitionType.Bullets;
    public KillFeedWeaponShowMode killFeedWeaponShowMode = KillFeedWeaponShowMode.WeaponIcon;
    public LobbyJoinMethod lobbyJoinMethod = LobbyJoinMethod.WaitingRoom;
    public bl_KillCam.KillCameraType killCameraType = bl_KillCam.KillCameraType.ObserveDeath;
    public bl_KillFeed.LocalKillDisplay localKillsShowMode = bl_KillFeed.LocalKillDisplay.Queqe;
    public bl_GunManager.AutoChangeOnPickup switchToPickupWeapon = bl_GunManager.AutoChangeOnPickup.OnlyOnEmptySlots;
    
    [Header("Rewards")]
    public ScoreRewards ScoreReward;
    public VirtualCoin VirtualCoins;

    [Header("Settings")]
    public string GameVersion = "1.0";
    public string guestNameFormat = "Guest {0}";
    [Range(0, 10)] public int SpawnProtectedTime = 5;
    [Range(1, 60)] public int CountDownTime = 7;
    [Range(1, 10)] public float PlayerRespawnTime = 5.0f;
    [Range(1, 100)] public int MaxFriendsAdded = 25;
    public float AFKTimeLimit = 60;
    public int MaxChangeTeamTimes = 3;
    public int maxSuicideAttempts = 3;
    public string MainMenuScene = "MainMenu";
    public string OnDisconnectScene = "MainMenu";
    public Color highLightColor = Color.green;

    [Header("Levels Manager")]
    [Reorderable]
    public List<SceneInfo> AllScenes = new List<SceneInfo>();

    [Header("Weapons")]
    /* [Reorderable]*/
    public List<bl_GunInfo> AllWeapons = new List<bl_GunInfo>();

    [Header("Default Settings")]
    [ScriptableDrawer, SerializeField] private bl_RuntimeSettingsProfile defaultSettings;

    [Header("Game Modes Available"), Reorderable, FormerlySerializedAs("AllGameModes")]
    public List<GameModeSettings> gameModes = new List<GameModeSettings>();

    [Header("Teams")]
    public string Team1Name = "Team1";
    public Color Team1Color = Color.blue;
    [Space(5)]
    public string Team2Name = "Team2";
    public Color Team2Color = Color.green;

    [Header("Players")]
    public bl_PlayerNetwork Player1;
    public bl_PlayerNetwork Player2;

    [Header("Bots")]
    public bl_AIShooter BotTeam1;
    public bl_AIShooter BotTeam2;

    [ScriptableDrawer]public bl_WeaponSlotRuler weaponSlotRuler;

    [Header("Game Team")]
    public List<GameTeamInfo> GameTeam = new List<GameTeamInfo>();

    public GameTeamInfo CurrentTeamUser { get; set; } = null;
    [HideInInspector] public bool isChating = false;

    [HideInInspector] public string _MFPSLicense = string.Empty;
    [HideInInspector] public int _MFPSFromStore = 2;
    [HideInInspector] public string _keyToken = "";

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        isDataCached = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetInstance()
    {
        m_settingProfile = null;
        isDataCached = false;
    }

    #region Getters
    /// <summary>
    /// Get a weapon info by they ID
    /// </summary>
    /// <param name="ID">the id of the weapon, this ID is the indexOf the weapon info in GameData</param>
    /// <returns></returns>
    public bl_GunInfo GetWeapon(int ID)
    {
        if (ID < 0 || ID > AllWeapons.Count - 1)
            return AllWeapons[0];

        return AllWeapons[ID];
    }

    /// <summary>
    /// Get a weapon info by they Name
    /// </summary>
    /// <param name="gunName"></param>
    /// <returns></returns>
    public int GetWeaponID(string gunName)
    {
        int id = -1;
        if (AllWeapons.Exists(x => x.Name == gunName))
        {
            id = AllWeapons.FindIndex(x => x.Name == gunName);
        }
        return id;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string[] AllWeaponStringList()
    {
        return AllWeapons.Select(x => x.Name).ToList().ToArray();
    }

    /// <summary>
    /// 
    /// </summary>
    public int CheckPlayerName(string pName)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (pName == GameTeam[i].UserName)
            {
                return 1;
            }
        }
        if (pName.Contains('[') || pName.Contains('{'))
        {
            return 2;
        }
        CurrentTeamUser = null;
        return 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CheckPasswordUse(string PName, string Pass)
    {
        for (int i = 0; i < GameTeam.Count; i++)
        {
            if (PName == GameTeam[i].UserName)
            {
                if (Pass == GameTeam[i].Password)
                {
                    CurrentTeamUser = GameTeam[i];
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
#if CLANS
    private string _role = string.Empty;
#endif
    public string RolePrefix
    {
        get
        {
#if !CLANS
            if (CurrentTeamUser != null && !string.IsNullOrEmpty(CurrentTeamUser.UserName))
            {
                return string.Format("<color=#{1}>[{0}]</color>", CurrentTeamUser.m_Role.ToString(), ColorUtility.ToHtmlStringRGBA(CurrentTeamUser.m_Color));
            }
            else
            {
                return string.Empty;
            }
#else
            if(bl_DataBase.Instance == null || !bl_DataBase.Instance.isLogged || !bl_DataBase.Instance.LocalUser.Clan.haveClan)
            {
                return string.Empty;
            }
            else
            {
                if (string.IsNullOrEmpty(_role))
                {
                    _role = string.Format("[{0}]", bl_DataBase.Instance.LocalUser.Clan.Name);
                }
                return _role;
            }
#endif
        }

    }

    private bl_RuntimeSettingsProfile m_settingProfile;
    public bl_RuntimeSettingsProfile RuntimeSettings
    {
        get
        {
            if (m_settingProfile == null) m_settingProfile = Instantiate(defaultSettings);
            return m_settingProfile;
        }
    }


    private static bl_PhotonNetwork PhotonGameInstance = null;
    public static bool isDataCached = false;
    private static bool isCaching = false;
    private static bl_GameData m_instance;
    public static bl_GameData Instance
    {
        get
        {
            if (m_instance == null && !isCaching)
            {
                if (!isDataCached && Application.isPlaying)
                {
                    Debug.Log("GameData was cached synchronous, that could cause bottleneck on load, try caching it asynchronous with AsyncLoadData()");
                    isDataCached = true;
                }
                m_instance = Resources.Load("GameData", typeof(bl_GameData)) as bl_GameData;
            }

            //check that there's an instance of the Photon object in scene
            if (PhotonGameInstance == null && Application.isPlaying)
            {
                if (bl_RoomMenu.Instance != null && bl_RoomMenu.Instance.isApplicationQuitting) return m_instance;

                PhotonGameInstance = bl_PhotonNetwork.Instance;
                if (PhotonGameInstance == null)
                {
                    try
                    {
                        var pgo = new GameObject("PhotonGame");
                        PhotonGameInstance = pgo.AddComponent<bl_PhotonNetwork>();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return m_instance;
        }
    }
    #endregion

    /// <summary>
    /// cache the GameData from Resources asynchronous to avoid overhead and freeze the main thread the first time we access to the instance
    /// </summary>
    /// <returns></returns>
    public static IEnumerator AsyncLoadData()
    {
        if (m_instance == null)
        {
            isCaching = true;
            ResourceRequest rr = Resources.LoadAsync("GameData", typeof(bl_GameData));
            while (!rr.isDone) { yield return null; }
            m_instance = rr.asset as bl_GameData;
            isCaching = false;
        }
        isDataCached = true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        for (int i = 0; i < AllScenes.Count; i++)
        {
            if (AllScenes[i].m_Scene == null) continue;
            AllScenes[i].RealSceneName = AllScenes[i].m_Scene.name;
        }
    }
#endif

    #region Local Classes
    [Serializable]
    public class SceneInfo
    {
        public string ShowName;
        [SerializeField]
        public Object m_Scene;
        [HideInInspector] public string RealSceneName;
        [SpritePreview] public Sprite Preview;
    }

    [Serializable]
    public class DefaultSettingsData
    {
        [Range(1, 20)] public float DefaultSensitivity = 5.0f;
        [Range(1, 20)] public float DefaultSensitivityAim = 2;
        public int DefaultQualityLevel = 3;
        public int DefaultAnisoTropic = 2;
        [Range(0, 1)] public float DefaultVolume = 1;
        [Range(40, 100)] public int DefaultWeaponFoV = 60;
        public bool DefaultShowFrameRate = false;
        public bool DefaultMotionBlur = true;
        public int[] frameRateOptions = new int[] { 30, 60, 120, 144, 200, 260, 0 };
        public int defaultFrameRate = 2;
    }

    [System.Serializable]
    public class ScoreRewards
    {
        public int ScorePerKill = 50;
        public int ScorePerHeadShot = 25;
        public int ScoreForWinMatch = 100;
        [Tooltip("Per minute played")]
        public int ScorePerTimePlayed = 3;

        public int GetScorePerTimePlayed(int time)
        {
            if (ScorePerTimePlayed <= 0) return 0;
            return time * ScorePerTimePlayed;
        }
    }

    [System.Serializable]
    public class VirtualCoin
    {
        public int InitialCoins = 1000;
        [Tooltip("how much score/xp worth one coin")]
        public int CoinScoreValue = 1000;//how much score/xp worth one coin

        public int UserCoins { get; set; }

        public void LoadCoins(string userName)
        {
            UserCoins = PlayerPrefs.GetInt(string.Format("{0}.{1}", userName, PropertiesKeys.UserCoins), InitialCoins);
        }

        public void SetCoins(int coins, string userName)
        {
            LoadCoins(userName);
            int total = UserCoins + coins;
            PlayerPrefs.SetInt(string.Format("{0}.{1}", userName, PropertiesKeys.UserCoins), total);
            UserCoins = total;
        }

        public int GetCoinsPerScore(int score)
        {
            if (score <= 0 || score < CoinScoreValue || CoinScoreValue <= 0) return 0;

            return score / CoinScoreValue;
        }
    }

    [System.Serializable]
    public class GameTeamInfo
    {
        public string UserName;
        public Role m_Role = Role.Moderator;
        public string Password;
        public Color m_Color;

        public enum Role
        {
            Admin = 0,
            Moderator = 1,
        }
    }
    #endregion
}