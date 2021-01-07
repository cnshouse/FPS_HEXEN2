using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MFPS.PlayerSelector;
using UnityEngine.Serialization;

namespace MFPS.PlayerSelector
{
    public class bl_PlayerSelectorData : ScriptableObject
    {
        [Header("Players")]
        public List<bl_PlayerSelectorInfo> AllPlayers = new List<bl_PlayerSelectorInfo>();
        [Header("Player Per Team")]
        [FormerlySerializedAs("DeltaPlayers")]
        [PlayerSelectorID] public List<int> Team1Players = new List<int>();
        [FormerlySerializedAs("ReconPlayers")]
        [PlayerSelectorID] public List<int> Team2Players = new List<int>();
        [PlayerSelectorID] public List<int> FFAPlayers = new List<int>();

        [Header("Settings")]
        public PSType PlayerSelectorMode = PSType.InLobby;

        public const string OPERATORS_KEY = "mfps.operators";
        public const string FAV_TEAM_KEY = "mfps.operators.favteam";

        public bl_PlayerSelectorInfo GetPlayer(Team team, int id)
        {
            if (team == Team.Team1)
            {
                return AllPlayers[Team1Players[id]];
            }
            else if (team == Team.Team2)
            {
                return AllPlayers[Team2Players[id]]; 
            }
            else
            {
                return AllPlayers[FFAPlayers[id]];
            }
        }

        public bl_PlayerSelectorInfo GetPlayerByIndex(int id)
        {
            return AllPlayers[id];
        }


        public List<bl_PlayerSelectorInfo> GetPlayerList(Team team)
        {
            List<bl_PlayerSelectorInfo> list = new List<bl_PlayerSelectorInfo>();
            List<int> ids = team == Team.Team1 ? Team1Players : Team2Players;
            for (int i = 0; i < ids.Count; i++)
            {
                bl_PlayerSelectorInfo info = AllPlayers[ids[i]];
                info.ID = ids[i];
                info.team = team;
                list.Add(info);
            }
            return list;
        }

        public int GetPlayerID(string name)
        {
            return AllPlayers.FindIndex(x => x.Name == name);
        }

        public string[] AllPlayerStringList()
        {
            return AllPlayers.Select(x => x.Name).ToList().ToArray();
        }

        public static int GetTeamOperatorID(Team team)
        {
            return PlayerPrefs.GetInt(OPERATORS_KEY + team.ToString(), 0);
        }

        public static void SetTeamOperator(int operatorID, Team team)
        {
            PlayerPrefs.SetInt(OPERATORS_KEY + team.ToString(), operatorID);
        }

        public bl_PlayerSelectorInfo GetSelectedPlayerFromTeam(Team team)
        {
            int id = GetTeamOperatorID(team);
            if (team == Team.All || team == Team.None)
            {
                id = GetTeamOperatorID(GetFavoriteTeam());
            }
            return GetPlayerByIndex(id);
        }

        public static Team GetFavoriteTeam()
        {
            int id = PlayerPrefs.GetInt(FAV_TEAM_KEY, (int)Team.Team1);
            return (Team)id;
        }

        public void SetFavoriteTeam(Team team)
        {
            PlayerPrefs.SetInt(FAV_TEAM_KEY, (int)team);
        }

        [System.Serializable]
        public enum PSType
        {
            InMatch,
            InLobby,
        }

        private static bl_PlayerSelectorData m_Data;
        public static bl_PlayerSelectorData Instance
        {
            get
            {
                if (m_Data == null)
                {
                    m_Data = Resources.Load("PlayerSelector", typeof(bl_PlayerSelectorData)) as bl_PlayerSelectorData;
                }
                return m_Data;
            }
        }
    }
}