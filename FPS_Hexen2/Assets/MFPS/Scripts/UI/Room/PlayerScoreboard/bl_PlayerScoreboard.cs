using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class bl_PlayerScoreboard : bl_MonoBehaviour
{
    public GameObject playerscoreboardUIBinding;
    public bl_PlayerScoreboardTable[] TwoTeamScoreboards;
    public bl_PlayerScoreboardTable OneTeamScoreboard;
    public Text SpectatorsCountText;

    Dictionary<int, bl_PlayerScoreboardUI> cachedUIBindings = new Dictionary<int, bl_PlayerScoreboardUI>();
    Dictionary<bl_AIMananger.BotsStats, bl_PlayerScoreboardUI> cachedBotsUIBindings = new Dictionary<bl_AIMananger.BotsStats, bl_PlayerScoreboardUI>();
    bool botsScoreInstance = false;
    private List<bl_PlayerScoreboardUI> cachePlayerScoreboardSorted = new List<bl_PlayerScoreboardUI>();
    private List<bl_PlayerScoreboardUI> cachePlayerScoreboardSorted2 = new List<bl_PlayerScoreboardUI>();

    public bool isShowingTables { get; set; } = false;

    /// <summary>
    /// If you don't want to show the scoreboard, simply set this property to false
    /// </summary>
    private bool blockScoreboards = false;
    public bool BlockScoreboards
    {
        get { return blockScoreboards; }
        set
        {
            if (value == true)
            {
                SetActive(false);
            }
            blockScoreboards = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonCallbacks.PlayerLeftRoom += OnPlayerLeftRoom;
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.RoomPropertiesUpdate += OnRoomPropertiesUpdate;
        ForceUpdateAll();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPlayerLeftRoom;
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
        bl_PhotonCallbacks.RoomPropertiesUpdate -= OnRoomPropertiesUpdate;
        CancelInvoke();
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateTables()
    {
        int spectators = 0;
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Player p = players[i];
            //check if the player has selected a team
            if (p.GetPlayerTeam() != Team.None)
            {
                //is the ui binding already created for this player?
                if (cachedUIBindings.ContainsKey(p.ActorNumber))
                {
                    //if it's so, then simply refresh his info
                    cachedUIBindings[p.ActorNumber]?.Refresh();
                }
                else
                {
                    bl_PlayerScoreboardUI script = null;
                    if (!isOneTeamMode)
                    {
                        if (p.GetPlayerTeam() == Team.Team1)
                            script = TwoTeamScoreboards[0].Instance(p, playerscoreboardUIBinding);
                        else
                            script = TwoTeamScoreboards[1].Instance(p, playerscoreboardUIBinding);
                    }
                    else
                    {
                        script = OneTeamScoreboard.Instance(p, playerscoreboardUIBinding);
                    }
                    cachedUIBindings.Add(p.ActorNumber, script);
                }
            }
            else { spectators++; }//if has not team
        }
        UpdateBotScoreboard();
        if (SpectatorsCountText != null)
        {
#if LOCALIZATION
        SpectatorsCountText.text = string.Format(bl_Localization.Instance.GetTextPlural(122), spectators);
#else
            SpectatorsCountText.text = string.Format(bl_GameTexts.Spectators, spectators);
#endif
        }
        SortScoreboard();
    }

    /// <summary>
    /// Update the scoreboard players and bots fields
    /// </summary>
    void UpdateBotScoreboard()
    {
        if (bl_AIMananger.Instance == null || !bl_AIMananger.Instance.BotsActive || bl_AIMananger.Instance.BotsStatistics.Count <= 0) return;

        int c = bl_AIMananger.Instance.BotsStatistics.Count;
        for (int i = 0; i < c; i++)
        {
            bl_AIMananger.BotsStats stat = bl_AIMananger.Instance.BotsStatistics[i];
            if (botsScoreInstance)
            {
                if (cachedBotsUIBindings.ContainsKey(stat))
                {
                    cachedBotsUIBindings[stat]?.UpdateBot();
                }
                else
                {
                    InstanceBotUIBinding(stat);
                }
            }
            else
            {
                InstanceBotUIBinding(stat);
            }
        }
        botsScoreInstance = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void InstanceBotUIBinding(bl_AIMananger.BotsStats info)
    {
        bl_PlayerScoreboardUI script = null;
        if (!isOneTeamMode)
        {
            if (info.Team == Team.Team1)
                script = TwoTeamScoreboards[0].InstanceBot(info, playerscoreboardUIBinding);
            else
                script = TwoTeamScoreboards[1].InstanceBot(info, playerscoreboardUIBinding);
        }
        else
        {
            script = OneTeamScoreboard.InstanceBot(info, playerscoreboardUIBinding);
        }
        cachedBotsUIBindings.Add(info, script);
    }

    /// <summary>
    /// Force update the scoreboard information
    /// </summary>
    public void ForceUpdateAll()
    {
        List<bl_PlayerScoreboardUI> ailist = cachedBotsUIBindings.Values.ToList();
        foreach (var item in ailist)
        {
            item?.Refresh();
        }
        UpdateTables();
    }

    /// <summary>
    /// Sort the scoreboard players by their score
    /// </summary>
    void SortScoreboard()
    {
        if (isOneTeamMode)
        {
            cachePlayerScoreboardSorted.Clear();
            cachePlayerScoreboardSorted.AddRange(cachedUIBindings.Values.ToArray());
            cachePlayerScoreboardSorted.AddRange(cachedBotsUIBindings.Values.ToArray());
            cachePlayerScoreboardSorted = cachePlayerScoreboardSorted.OrderBy(x => x.GetScore()).ToList();

            for (int i = 0; i < cachePlayerScoreboardSorted.Count; i++)
            {
                if (cachePlayerScoreboardSorted[i] == null) return;
                cachePlayerScoreboardSorted[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted.Count - 1) - i);
            }
        }
        else
        {
            cachePlayerScoreboardSorted.Clear();
            cachePlayerScoreboardSorted2.Clear();
            List<bl_PlayerScoreboardUI> all = new List<bl_PlayerScoreboardUI>();
            all.AddRange(cachedUIBindings.Values.ToArray());
            all.AddRange(cachedBotsUIBindings.Values.ToArray());

            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].GetTeam() == Team.Team1)
                {
                    cachePlayerScoreboardSorted.Add(all[i]);
                }
                else if (all[i].GetTeam() == Team.Team2)
                {
                    cachePlayerScoreboardSorted2.Add(all[i]);
                }
            }
            cachePlayerScoreboardSorted = cachePlayerScoreboardSorted.OrderBy(x => x.GetScore()).ToList();
            cachePlayerScoreboardSorted2 = cachePlayerScoreboardSorted2.OrderBy(x => x.GetScore()).ToList();
            for (int i = 0; i < cachePlayerScoreboardSorted.Count; i++)
            {
                if (cachePlayerScoreboardSorted[i] == null) return;
                cachePlayerScoreboardSorted[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted.Count - 1) - i);
            }
            for (int i = 0; i < cachePlayerScoreboardSorted2.Count; i++)
            {
                if (cachePlayerScoreboardSorted2[i] == null) return;
                cachePlayerScoreboardSorted2[i].transform.SetSiblingIndex((cachePlayerScoreboardSorted2.Count - 1) - i);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveByTeamMode(bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm);
        foreach(var table in TwoTeamScoreboards) { table?.SetActive(!tm); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = true;
        ForceUpdateAll();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActive(bool active, bool enableJoinButtons = false)
    {
        if (BlockScoreboards) return;

        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActive(tm && active);
        foreach (var table in TwoTeamScoreboards) { table?.SetActive(!tm && active); }
        SetActiveJoinButtons(enableJoinButtons);
        isShowingTables = active;
        if (active) { ForceUpdateAll(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetJoinButtons()
    {
        OneTeamScoreboard?.ResetJoinButton();
        foreach (var table in TwoTeamScoreboards) { table?.ResetJoinButton(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveJoinButtons(bool active)
    {
        bool tm = isOneTeamMode;
        OneTeamScoreboard?.SetActiveJoinButton(active && tm);
        foreach (var table in TwoTeamScoreboards) { table?.SetActiveJoinButton(active && !tm); }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool RemoveUIBinding(bl_PlayerScoreboardUI uiBinding)
    {
        if (uiBinding.isBotBinding)
        {
            if (cachedBotsUIBindings.ContainsKey(uiBinding.Bot))
            {
                Destroy(cachedBotsUIBindings[uiBinding.Bot].gameObject);
                cachedBotsUIBindings.Remove(uiBinding.Bot);
                return true;
            }
        }
        else
        {
            if (cachedUIBindings.ContainsKey(uiBinding.cachePlayer.ActorNumber))
            {
                Destroy(cachedUIBindings[uiBinding.cachePlayer.ActorNumber].gameObject);
                cachedUIBindings.Remove(uiBinding.cachePlayer.ActorNumber);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (cachedUIBindings.ContainsKey(otherPlayer.ActorNumber))
        {
            RemoveUIBinding(cachedUIBindings[otherPlayer.ActorNumber]);
        }
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!isShowingTables) return;
        UpdateTables();
    }

    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (!isShowingTables) return;
        UpdateTables();
    }


    private static bl_PlayerScoreboard _instance;
    public static bl_PlayerScoreboard Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PlayerScoreboard>(); }
            return _instance;
        }
    }
}