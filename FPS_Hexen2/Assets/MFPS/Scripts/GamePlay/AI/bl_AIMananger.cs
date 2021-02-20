using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable; //Replace default Hashtables with Photon hashtables
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class bl_AIMananger : bl_PhotonHelper
{
    [Header("Settings")]
    public int updateBotsLookAtEach = 50;
    [HideInInspector] public List<Transform> AllBotsTransforms = new List<Transform>();
    public List<BotsStats> BotsStatistics = new List<BotsStats>();

    private bl_GameManager GameManager;
    private List<PlayersSlots> Team1PlayersSlots = new List<PlayersSlots>();
    private List<PlayersSlots> Team2PlayersSlots = new List<PlayersSlots>();

    public bool BotsActive { get; set; }
    private List<string> BotsNames = new List<string>();
    private List<bl_AIShooter> SpawningBots = new List<bl_AIShooter>();
    private List<string> lastLifeBots = new List<string>();
    private int NumberOfBots = 5;
    private List<bl_AIShooter> AllBots = new List<bl_AIShooter>();

    public delegate void EEvent(List<BotsStats> stats);
    public static EEvent OnMaterStatsReceived;
    public delegate void StatEvent(BotsStats stat);
    public static StatEvent OnBotStatUpdate;
    [HideInInspector]public bool hasMasterInfo = false;
    private bool isMasterAlredyInTeam = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        GameManager = bl_GameManager.Instance;
        BotsNames.AddRange(bl_GameTexts.RandomNames);
        bl_PhotonCallbacks.PlayerPropertiesUpdate += OnPhotonPlayerPropertiesChanged;
        bl_PhotonCallbacks.PlayerEnteredRoom += OnPlayerEnter;
        bl_PhotonCallbacks.MasterClientSwitched += OnMasterClientSwitched;
        bl_PhotonCallbacks.PlayerLeftRoom += OnPlayerLeft;
        if (!PhotonNetwork.IsConnected)
            return;

        BotsActive = (bool)PhotonNetwork.CurrentRoom.CustomProperties[PropertiesKeys.WithBotsKey];
        NumberOfBots = PhotonNetwork.CurrentRoom.MaxPlayers;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        FirstSpawn();
        if(bl_GameData.Instance.lobbyJoinMethod == LobbyJoinMethod.WaitingRoom && PhotonNetwork.IsMasterClient)
        {
            this.InvokeAfter(2, SyncBotsDataToAllOthers);
        }
    }
    
    /// <summary>
    /// Instance all bots for the first time
    /// </summary>
    void FirstSpawn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SetUpSlots(true);
            if (BotsActive)
            {
                if (isOneTeamMode)
                {
                    int requiredBots = EmptySlotsCount(Team.All);
                    for (int i = 0; i < requiredBots; i++)
                    {
                        SpawnBot(null, Team.All);
                    }
                }
                else
                {
                    int half = EmptySlotsCount(Team.Team1);
                    for (int i = 0; i < half; i++)
                    {
                        SpawnBot(null, Team.Team1);
                    }
                    half = EmptySlotsCount(Team.Team2);
                    for (int i = 0; i < half; i++)
                    {
                        SpawnBot(null, Team.Team2);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_PhotonCallbacks.PlayerPropertiesUpdate -= OnPhotonPlayerPropertiesChanged;
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnPlayerEnter;
        bl_PhotonCallbacks.MasterClientSwitched -= OnMasterClientSwitched;
        bl_PhotonCallbacks.PlayerLeftRoom -= OnPlayerLeft;
    }

    /// <summary>
    /// Send the bots data to all other clients in the room
    /// This data will automatically send to new players
    /// </summary>
    void SyncBotsDataToAllOthers()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Player[] players = PhotonNetwork.PlayerList;
        string line = GetCompiledBotsData();
        //and send to the new player so him can have the data and update locally.
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 0);

        //also send the slots data so all player have the same list in case the Master Client leave the game
        line = GetCompiledSlotsData();
        //and send to the new player so him can have the data and update locally.
        photonView.RPC(nameof(SyncAllBotsStats), RpcTarget.Others, line, 1);
        bl_EventHandler.onBotsInitializated?.Invoke();
    }

    /// <summary>
    /// Gets the bots data as a string line
    /// </summary>
    /// <returns></returns>
    public string GetCompiledBotsData()
    {
        //so first we recollect all the stats from the master client and join it in a string line
        string line = string.Empty;
        for (int i = 0; i < BotsStatistics.Count; i++)
        {
            BotsStats b = BotsStatistics[i];
            line += string.Format("{0},{1},{2},{3},{4},{5}|", b.Name, b.Kills, b.Deaths, b.Score, (int)b.Team, b.ViewID);
        }
        return line;
    }

    /// <summary>
    /// Get the slots list in a string line
    /// </summary>
    /// <returns></returns>
    public string GetCompiledSlotsData()
    {
        string line = string.Empty;
        for (int i = 0; i < Team1PlayersSlots.Count; i++)
        {
            var d = Team1PlayersSlots[i];
            line += string.Format("{0},{1}|", d.Player, d.Bot);
        }
        line += "&";
        if (!isOneTeamMode)
        {
            for (int i = 0; i < Team2PlayersSlots.Count; i++)
            {
                var d = Team2PlayersSlots[i];
                line += string.Format("{0},{1}|", d.Player, d.Bot);
            }
        }
        return line;
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpSlots(bool addExistingPlayers)
    {
        Team1PlayersSlots.Clear();
        Team2PlayersSlots.Clear();
        var team1Players = PhotonNetwork.PlayerList.GetPlayersInTeam(isOneTeamMode ? Team.All : Team.Team1).ToList();

        if (!isOneTeamMode)
        {
            var team2Players = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2).ToList();

            int ptp = NumberOfBots / 2;
            for (int i = 0; i < ptp; i++)
            {
                PlayersSlots s = new PlayersSlots();
                s.Bot = string.Empty;
                if (addExistingPlayers && team1Players.Count > 0)
                {
                    s.Player = team1Players[0].NickName;
                    team1Players.RemoveAt(0);
                }
                else
                {
                    s.Player = string.Empty;
                }
                Team1PlayersSlots.Add(s);
            }
            for (int i = 0; i < ptp; i++)
            {
                PlayersSlots s = new PlayersSlots();
                s.Bot = string.Empty;
                if (addExistingPlayers && team2Players.Count > 0)
                {
                    s.Player = team2Players[0].NickName;
                    team2Players.RemoveAt(0);
                }
                else
                {
                    s.Player = string.Empty;
                }
                Team2PlayersSlots.Add(s);
            }
        }
        else
        {
            for (int i = 0; i < NumberOfBots; i++)
            {
                PlayersSlots s = new PlayersSlots();
                s.Bot = string.Empty;
                if (addExistingPlayers && team1Players.Count > 0)
                {
                    s.Player = team1Players[0].NickName;
                    team1Players.RemoveAt(0);
                    Debug.Log("Set default player in slot: " + s.Player);
                }
                else
                {
                    s.Player = string.Empty;
                }
                Team1PlayersSlots.Add(s);
            }
        }
    }

    /// <summary>
    /// Spawn the a bot in the selected team
    /// You can set the agent as null to instantiate it for the first time
    /// </summary>
    public bl_AIShooter SpawnBot(bl_AIShooter agent = null, Team _team = Team.None)
    {
        Transform spawnPoint = bl_SpawnPointManager.Instance.GetSingleRandom();
        string AiName = bl_GameData.Instance.BotTeam1.name;
        if (agent != null)//if is a already instanced bot
        {
            AiName = (agent.AITeam == Team.Team2) ? bl_GameData.Instance.BotTeam2.name : bl_GameData.Instance.BotTeam1.name;
            if (agent.AITeam == Team.None) { Debug.LogError($"bot {agent.AIName} has not team"); }

            //Check if the bot has been assigned to a team, or if not, check if there's a space for him
            if (VerifyTeamAffiliation(agent, agent.AITeam))
            {
                spawnPoint = bl_SpawnPointManager.Instance.GetSpawnPointForTeam(agent.AITeam, bl_SpawnPointManager.SpawnMode.Sequential);
            }
            else // there's not space in the team for this bot
            {
                //Check if the bot was registered in a team before
                int ind = BotsStatistics.FindIndex(x => x.Name == agent.AIName);
                if (ind != -1 && ind <= BotsStatistics.Count - 1)
                {
                    //delete the bot data since it won't play anymore.
                    BotsStatistics.RemoveAt(ind);
                }
                return null;
            }
        }
        else
        {
            AiName = (_team == Team.Team2) ? bl_GameData.Instance.BotTeam2.name : bl_GameData.Instance.BotTeam1.name;
            if (!isOneTeamMode)//if team mode, spawn bots in the respective team spawn points.
            {
                spawnPoint = bl_SpawnPointManager.Instance.GetSpawnPointForTeam(_team, bl_SpawnPointManager.SpawnMode.Sequential);
            }
        }

        int rbn = Random.Range(0, BotsNames.Count);
        string AIName = agent == null ? "BOT " + BotsNames[rbn] : agent.AIName;
        Team AITeam = agent == null ? _team : agent.AITeam;

        object[] botEssentialData = new object[] { AIName, AITeam };
        //use InstantiateSceneObject to make the bots by controlled by Master Client but not destroy them when MC leave the room.
        GameObject bot = PhotonNetwork.InstantiateRoomObject(AiName, spawnPoint.position, spawnPoint.rotation, 0, botEssentialData);

        bl_AIShooter newAgent = bot.GetComponent<bl_AIShooter>();
        //if this bot was already in the game
        if (agent != null)
        {
            newAgent.AIName = agent.AIName;
            newAgent.AITeam = agent.AITeam;
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, agent.AIName, bot.GetComponent<PhotonView>().ViewID, (byte)3);
        }
        else//if this is the first time instancing this bot
        {
            newAgent.AIName = AIName;
            newAgent.AITeam = _team;
            BotsNames.RemoveAt(rbn);
            //insert bot stats
            BotsStats bs = new BotsStats();
            bs.Name = newAgent.AIName;
            bs.Team = _team;
            bs.ViewID = bot.GetComponent<PhotonView>().ViewID;
            BotsStatistics.Add(bs);
            //reserve a space in the team for this bot
            VerifyTeamAffiliation(newAgent, _team);
        }
        newAgent.Init();

        //Build Player Data
        MFPSPlayer playerData = new MFPSPlayer()
        {
            Name = newAgent.AIName,
            Team = newAgent.AITeam,
            Actor = newAgent.transform,
            AimPosition = newAgent.AimTarget,
            isRealPlayer = false,
            isAlive = true,
        };

        bl_EventHandler.OnRemoteActorChange(newAgent.AIName, playerData, true);
        AllBots.Add(newAgent);
        AllBotsTransforms.Add(newAgent.AimTarget);

        return newAgent;
    }

    /// <summary>
    /// Check if the bot is already assigned in a Team slot
    /// </summary>
    /// <returns></returns>
    private bool VerifyTeamAffiliation(bl_AIShooter agent, Team team)
    {
        var playerSlots = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        //check if the bot is assigned in the team
        if (playerSlots.Exists(x => x.Bot == agent.AIName)) return true;
        else
        {
            //if it's not assigned, check if we can add him
            if (hasSpaceInTeamForBot(team))
            {
                //assign the bot to the team
                int index = playerSlots.FindIndex(x => x.Player == string.Empty && x.Bot == string.Empty);
                playerSlots[index].Bot = agent.AIName;
                return true;
            }
            else { return false; }//bot can't be assigned in team
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnBotDeath(bl_AIShooter agent, bl_AIShooter killer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        AllBots.Remove(agent);
        AllBotsTransforms.Remove(agent.AimTarget);
        for (int i = 0; i < AllBots.Count; i++)
        {
            AllBots[i].CheckTargets();
        }

        SpawningBots.Add(agent);
        //automatically spawn the bot after the re-spawn time
        if (GetGameMode.GetGameModeInfo().onPlayerDie == GameModeSettings.OnPlayerDie.SpawnAfterDelay)
        {
            Invoke(nameof(SpawnPendingBot), bl_GameData.Instance.PlayerRespawnTime);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void RespawnAllBots()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        for (int i = AllBots.Count - 1; i >= 0; i--)
        {
            if (AllBots[i].gameObject == null)
            {
                SpawnBot(AllBots[i]);
                Destroy(AllBots[i]);
            }
        }

        for (int i = SpawningBots.Count - 1; i >= 0; i--)
        {
            if (SpawningBots[i] == null || SpawningBots[i].gameObject == null) continue;

            SpawnBot(SpawningBots[i]);
            Destroy(SpawningBots[i].gameObject);
            SpawningBots.RemoveAt(i);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SpawnPendingBot()
    {
        if (SpawningBots[0] != null)
        {
            SpawnBot(SpawningBots[0]);
            Destroy(SpawningBots[0].gameObject);
            SpawningBots.RemoveAt(0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetBotKill(string killer)
    {
        int index = BotsStatistics.FindIndex(x => x.Name == killer);
        if (index <= -1) return;

        BotsStatistics[index].Kills++;
        BotsStatistics[index].Score += bl_GameData.Instance.ScoreReward.ScorePerKill;
        photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, BotsStatistics[index].Name, 0, (byte)0);

        bl_GameManager.Instance.SetPoint(1, GameMode.FFA, Team.All);
        bl_GameManager.Instance.SetPoint(1, GameMode.TDM, BotsStatistics[index].Team);
    }

    /// <summary>
    /// Called in all clients when a bot die
    /// </summary>
    /// <param name="killed">bot that die</param>
    public void SetBotDeath(string killed)
    {
        //if this bots was already replaced by a real player
        if (lastLifeBots.Contains(killed))
        {
            //this is his last life, so since he die, remove his data
            int bi = bl_GameManager.Instance.OthersActorsInScene.FindIndex(x => x.Name == killed);
            if (bi != -1)
            {
                bl_GameManager.Instance.OthersActorsInScene.RemoveAt(bi);
                lastLifeBots.Remove(killed);
                return;
            }
        }
        int index = BotsStatistics.FindIndex(x => x.Name == killed);
        if (index <= -1) return;

        bl_EventHandler.EventBotDeath(killed);
        BotsStatistics[index].Deaths++;
        photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, BotsStatistics[index].Name, 0, (byte)1);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateBotView(bl_AIShooter bot, int viewID)
    {
        if(BotsStatistics.Exists(x => x.Name == bot.AIName))
        {
            BotsStatistics.Find(x => x.Name == bot.AIName).ViewID = viewID;
        }
    }

    #region Photon Events
    /// <summary>
    /// Event called when a new player enter in the match
    /// </summary>
    void OnPlayerEnter(Player player)
    {
        if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) return;

        //cause bots statistics are not sync by Hashtables as player data do we need sync it by RPC
        //so for sync it just one time (after will be update by the local client) we send it when a new player enter (only to the new player)
        if (PhotonNetwork.IsMasterClient)
        {
            //so first we recollect all the stats from the master client and join it in a string line
            string line = GetCompiledBotsData();
            //and send to the new player so him can have the data and update locally.
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 0);

            //also send the slots data so all player have the same list in case the Master Client leave the game
            line = GetCompiledSlotsData();
            photonView.RPC(nameof(SyncAllBotsStats), player, line, 1);
        }
    }

    /// <summary>
    /// Event called when a player change some property
    /// This used to listen when a player change of Team
    /// </summary>
    public void OnPhotonPlayerPropertiesChanged(Player player, Hashtable changedProps)
    {
        if (!BotsActive)
            return;
        if (!changedProps.ContainsKey(PropertiesKeys.TeamKey)) return;

        string remplaceBot = string.Empty;
        string teamName = (string)changedProps[PropertiesKeys.TeamKey];
        var slotList = teamName == Team.Team2.ToString() ? Team2PlayersSlots : Team1PlayersSlots;

        //check if this player was already assigned (maybe just change of team)
        if (slotList.Exists(x => x.Player == player.NickName)) return;
        //find a empty slot in the team
        int index = slotList.FindIndex(x => x.Player == string.Empty);
        if (index != -1)
        {
            //replace the bot slot with the new player
            remplaceBot = slotList[index].Bot;
            DeleteBot(remplaceBot);
            slotList[index].Player = player.NickName;
            slotList[index].Bot = string.Empty;

            //sync the slot change with other players
            if (PhotonNetwork.IsMasterClient)
            {
                int teamCmdID = teamName == Team.Team2.ToString() ? 2 : 1;
                photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{teamCmdID}|{player.NickName}", index, (byte)4);
            }
          //  Debug.Log($"Bot {remplaceBot} was replaced by {player.NickName}");
        }

        //remove the bot that the master client replace
        if (player.IsMasterClient && PhotonNetwork.IsMasterClient && !isMasterAlredyInTeam && !string.IsNullOrEmpty(remplaceBot))
        {
            bl_AIShooter bot = AllBots.Find(x => x.AIName == remplaceBot);
            if (bot != null)
            {
                PhotonView bv = bot.GetComponent<PhotonView>();
                bot.References.shooterHealth.DestroyBot();//destroy on remote clients
                AllBots.Remove(bot);
                AllBotsTransforms.Remove(bot.AimTarget);
                PhotonNetwork.Destroy(bv.gameObject);
            }
            isMasterAlredyInTeam = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newMasterClient"></param>
    public void OnMasterClientSwitched(Player newMasterClient)
    {
        //if the new master client is the local client
        if (newMasterClient.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            if(Team1PlayersSlots == null || Team1PlayersSlots.Count <= 0)
            SetUpSlots(false);

            //since bots where not collected on the new master client, lets take them manually
            bl_AIShooter[] allBots = FindObjectsOfType<bl_AIShooter>();
            foreach(var bot in allBots)
            {
                if (bot.isDeath)//if the bot was death when master client leave the game
                {
                    //invoke the spawn in the new client
                    SpawningBots.Add(bot);
                    Invoke(nameof(SpawnPendingBot), bl_GameData.Instance.PlayerRespawnTime);
                    continue;
                }
                AllBots.Add(bot);
                AllBotsTransforms.Add(bot.transform);
                bot.Init();
            }
           // Debug.Log("Bots data has been build in new Master Client");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnPlayerLeft(Player player)
    {
        if (!BotsActive) return;

        //Check if the player was occupying a slot
        Team team = player.GetPlayerTeam();
        var slotList = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        int index = slotList.FindIndex(x => x.Player == player.NickName);
        //empty the occupied slot
        if (index != -1) { slotList[index].Player = ""; }

        //make the master client instance a new bot to replace the player that just left
        if (PhotonNetwork.IsMasterClient)
        {
            //try to instance the new bot
            var newAgent = SpawnBot(null, team);
            if (newAgent == null) return;

            //find the slot id where the bot was assigned
            int botIndex = slotList.FindIndex(x => x.Bot == newAgent.AIName);
            //sync the new slot with all other players
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, $"{(int)team}|{newAgent.AIName}|{newAgent.photonView.ViewID}", botIndex, (byte)5);
            //show a notification in all players with the new bot name
            bl_KillFeed.Instance.SendMessageEvent($"{newAgent.AIName} has joined to {team.GetTeamName()}");
        }
    }
    #endregion

    [PunRPC]
    public void SyncBotStat(string data, int value, byte cmd)
    {
        BotsStats bs = BotsStatistics.Find(x => x.Name == data);
        if (bs == null && cmd != 5) return;
        if (cmd == 0)//add kill
        {
            bs.Kills++;
            bs.Score += bl_GameData.Instance.ScoreReward.ScorePerKill;
        }
        else if (cmd == 1)//death
        {
            bs.Deaths++;
        }
        else if (cmd == 2)//remove bot
        {
            RemoveBotInfo(data);
        }
        else if (cmd == 3)//update view id
        {
            bs.ViewID = value;
            OnBotStatUpdate?.Invoke(bs);
        }
        else if (cmd == 4)//replace bot slot with a player
        {
            string[] dataSplit = data.Split('|');
            var list = int.Parse(dataSplit[0]) == 1 ? Team1PlayersSlots : Team2PlayersSlots;
            if (list.Count <= 0) { Debug.LogWarning("Team slots has not been setup yet."); return; }
            list[value].Player = dataSplit[1];
            list[value].Bot = "";
        }
        else if (cmd == 5)//add single new bot
        {
            string[] dataSplit = data.Split('|');
            Team team = (Team)int.Parse(dataSplit[0]);
            var list = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
            if (list.Count <= 0) { Debug.LogWarning("Team slots has not been setup yet."); return; }
            list[value].Player = "";
            list[value].Bot = dataSplit[1];

            //add the bot statistic
            bs = new BotsStats();
            bs.Name = dataSplit[1];
            bs.Team = team;
            bs.ViewID = int.Parse(dataSplit[2]);
            BotsStatistics.Add(bs);
        }
    }

    [PunRPC]
    void SyncAllBotsStats(string data, int cmd)
    {
        if (cmd == 0)//bots statistics
        {
            BotsStatistics.Clear();
            string[] split = data.Split("|"[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split(","[0]);
                BotsStats bs = new BotsStats();
                bs.Name = info[0];
                bs.Kills = int.Parse(info[1]);
                bs.Deaths = int.Parse(info[2]);
                bs.Score = int.Parse(info[3]);
                bs.Team = (Team)int.Parse(info[4]);
                bs.ViewID = int.Parse(info[5]);
                BotsStatistics.Add(bs);

                if(!bl_GameManager.Instance.OthersActorsInScene.Exists(x => x.Name == bs.Name))
                {
                    bl_GameManager.Instance.OthersActorsInScene.Add(new MFPSPlayer()
                    {
                        Name = bs.Name,
                        isRealPlayer = false,
                        Team = bs.Team,
                    });
                }
            }
            OnMaterStatsReceived?.Invoke(BotsStatistics);
            hasMasterInfo = true;
        }
        else if (cmd == 1)//team slots info
        {
            SetUpSlots(false);
            string[] teams = data.Split('&');
            string[] teamInfo = teams[0].Split('|');//get the first team slots
            for (int i = 0; i < teamInfo.Length; i++)
            {
                if (string.IsNullOrEmpty(teamInfo[i])) continue;

                string[] slot = teamInfo[i].Split(',');
                Team1PlayersSlots[i].Player = slot[0];
                Team1PlayersSlots[i].Bot = slot[1];
            }
            if (!isOneTeamMode)
            {
                teamInfo = teams[1].Split('|');//get the second team slots
                for (int i = 0; i < teamInfo.Length; i++)
                {
                    if (string.IsNullOrEmpty(teamInfo[i])) continue;

                    string[] slot = teamInfo[i].Split(',');
                    Team2PlayersSlots[i].Player = slot[0];
                    Team2PlayersSlots[i].Bot = slot[1];
                }
            }
            bl_EventHandler.onBotsInitializated?.Invoke();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void RemoveBotInfo(string botName)
    {
        int bi = BotsStatistics.FindIndex(x => x.Name == botName);
        if (bi != -1)BotsStatistics.RemoveAt(bi);

        bi = bl_GameManager.Instance.OthersActorsInScene.FindIndex(x => x.Name == botName);
        if (bi != -1)
        {
            bl_GameManager.Instance.OthersActorsInScene.RemoveAt(bi);
        }

        lastLifeBots.Add(botName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    void DeleteBot(string Name)
    {
        if (BotsStatistics.Exists(x => x.Name == Name))
        {
            RemoveBotInfo(Name);
            photonView.RPC(nameof(SyncBotStat), RpcTarget.Others, Name, 0, (byte)2);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public List<MFPSPlayer> GetAllBotsInTeam(Team team)
    {
        List<MFPSPlayer> list = new List<MFPSPlayer>();
        for (int i = 0; i < bl_GameManager.Instance.OthersActorsInScene.Count; i++)
        {
            if (bl_GameManager.Instance.OthersActorsInScene[i].isRealPlayer) continue;

            if (bl_GameManager.Instance.OthersActorsInScene[i].Team == team)
            {
                list.Add(bl_GameManager.Instance.OthersActorsInScene[i]);
            }
        }
        return list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<Transform> GetOtherBots(Transform bot, Team _team)
    {
        List<Transform> all = new List<Transform>();
        if (isOneTeamMode)
        {
            all.AddRange(AllBotsTransforms);
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i] == null) continue;
                if (all[i].transform.root.name.Contains("(die)") || all[i].transform.root == bot.root)
                {
                    all.RemoveAt(i);
                }
            }
        }
        else //if TDM game mode
        {
            for (int i = 0; i < AllBotsTransforms.Count; i++)
            {
                if (AllBotsTransforms[i] == null) continue;

                Transform t = AllBotsTransforms[i].root;
                bl_AIShooter asa = t.GetComponent<bl_AIShooter>();
                if (t.name.Contains("(die)") || asa.isDeath) continue;

                if (asa.AITeam != _team && asa.AITeam != Team.None && t != bot.root)
                {
                    all.Add(AllBotsTransforms[i]);
                }
            }
        }
        if (all.Contains(bot))
        {
            all.Remove(bot);
        }
        return all;
    }

    /// <summary>
    /// Count the empty slots (not occupied by a real player)
    /// </summary>
    /// <returns></returns>
    public int EmptySlotsCount(Team team)
    {
        int count = 0;
        var list = team == Team.Team2 ? Team2PlayersSlots : Team1PlayersSlots;
        for (int i = 0; i < list.Count; i++)
        {
            if (string.IsNullOrEmpty(list[i].Player)) count++;
        }
        return count;
    }

    /// <summary>
    /// 
    /// </summary>
    private bool hasSpaceInTeam(Team team)
    {
        if (team == Team.Team2)
        {
            return Team2PlayersSlots.Exists(x => x.Player == string.Empty);
        }
        else
        {
            return Team1PlayersSlots.Exists(x => x.Player == string.Empty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private bool hasSpaceInTeamForBot(Team team)
    {
        if (team == Team.Team2)
        {
            return Team2PlayersSlots.Exists(x => x.Player == string.Empty && x.Bot == string.Empty);
        }
        else
        {
            return Team1PlayersSlots.Exists(x => x.Player == string.Empty && x.Bot == string.Empty);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bl_AIShooter GetBot(int viewID)
    {
        foreach (bl_AIShooter agent in AllBots)
        {
            if (agent.photonView.ViewID == viewID)
            {
                return agent;
            }
        }
        return null;
    }

    #region SubClasses
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public BotsStats GetBotWithMoreKills()
    {
        if(BotsStatistics == null || BotsStatistics.Count <= 0)
        {
            BotsStats bs = new BotsStats()
            {
                Name = "None",
                Kills = 0,
                Team = Team.None,
                Score = 0,
            };
            return bs;
        }
        int high = 0;
        int id = 0;
        for (int i = 0; i < BotsStatistics.Count; i++)
        {
            if (BotsStatistics[i].Kills > high)
            {
                high = BotsStatistics[i].Kills;
                id = i;
            }
        }
        return BotsStatistics[id];
    }

    [System.Serializable]
    public class PlayersSlots
    {
        public string Player;
        public string Bot;
    }

    [System.Serializable]
    public class BotsStats
    {
        public string Name;
        public int Kills;
        public int Deaths;
        public int Score;
        public Team Team;
        public int ViewID;
    }
    #endregion

    private static bl_AIMananger _instance;
    public static bl_AIMananger Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_AIMananger>(); }
            return _instance;
        }
    }
}