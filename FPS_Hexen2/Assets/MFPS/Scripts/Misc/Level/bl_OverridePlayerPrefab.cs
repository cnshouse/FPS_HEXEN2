using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_OverridePlayerPrefab : MonoBehaviour
{
    public bl_PlayerNetwork Team1Player, Team2Player;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        bl_GameManager.Instance.OverridePlayerPrefab = this;
    }

    /// <summary>
    /// 
    /// </summary>
    public GameObject GetPlayerForTeam(Team team)
    {
        var player = Team1Player.gameObject;
        if (team == Team.Team2)
        {
            player = Team2Player.gameObject;
            if (player == null) player = bl_GameData.Instance.Player2.gameObject;
        }
        else
        {
            if (player == null) player = bl_GameData.Instance.Player1.gameObject;
        }

        return player;
    }
}