using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class PropertiesKeys
{
    //Room
    public const string TimeRoomKey = "rTr";
    public const string GameModeKey = "rGm";
    public const string SceneNameKey = "rLn";
    public const string CustomSceneName = "rCsn";
    public const string RoomRoundKey = "rGpr";
    public const string RoomGoal = "rG";
    public const string TeamSelectionKey = "rAt";
    public const string RoomFriendlyFire = "rFf";
    public const string Team1Score = "rTs1";
    public const string Team2Score = "rTs2";
    public const string MaxPing = "rMp";
    public const string RoomPassword = "rPsw";
    public const string WithBotsKey = "rWb";
    public const string TimeState = "rTs";

    //Player
    public const string TeamKey = "Team";
    public const string KillsKey = "Kills";
    public const string DeathsKey = "Deaths";
    public const string ScoreKey = "Score";
    public const string KickKey = "Kick";
    public const string UserRole = "UserRole";
    public const string PlayerID = "playerID";
    public const string WaitingState = "WaitingState";

    //Prefs
    public const string PreferredRegion = "mfps.lobby.region";
    public const string PlayerName = "mfps.game.username";
    public const string RememberMe = "mfps.game.rememberme";
    public const string FrameRateOption = "mfps.game.frop";

    //Network Event Code
    public const byte KickPlayerEvent = 101;
    public const byte WaitingPlayerReadyEvent = 102;
    public const byte WaitingInitialSyncEvent = 103;
    public const byte WaitingStartGame = 104;
    public const byte ChatEvent = 105;
    public const byte KillFeedEvent = 106;
    public const byte DemolitionEvent = 107;
    public const byte DMBombEvent = 108;
    public const byte KillStreakEvent = 109;
    public const byte BattleRoyalEvent = 110;
    public const byte NetworkItemInstance = 111;
    public const byte NetworkItemChange = 112;
    public const byte WeaponPickUpEvent = 113;
    public const byte VoteEvent = 114;
    public const byte EliminationGameMode = 115;

    //Unique User
    public static string UserCoins { get { return string.Format("{0}.mfps.coins", PlayerPrefs.GetString(PlayerName)); } }
    public static string GetUniqueKey(string key) { return string.Format("{0}.{1}.{2}", Application.companyName, Application.productName, key); }

    public static bool GetBoolPrefs(this string key, bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1 ? true : false;
    }

    public static void SetBoolPrefs(this string key, bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }
}