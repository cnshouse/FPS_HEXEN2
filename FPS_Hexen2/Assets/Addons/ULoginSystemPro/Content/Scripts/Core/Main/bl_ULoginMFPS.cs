using System;
using System.Collections.Generic;
using UnityEngine;
using MFPS.ULogin;

public static class bl_ULoginMFPS
{
    /// <summary>
    /// Save the Kills, Deaths and Score of the local player
    /// </summary>
    public static void SaveLocalPlayerKDS(Action<bool> callback = null)
    {
        if (!bl_DataBase.IsUserLogged)
        {
            Debug.Log("Player has to be logged in order to save data");
            return;
        }

        var lp = bl_PhotonNetwork.LocalPlayer;
        var lu = bl_DataBase.Instance.LocalUser;

        lu.SendNewData(lp.GetKills(), lp.GetDeaths(), lp.GetPlayerScore());
        var fields = new ULoginUpdateFields();
        fields.AddField("kills", lu.Kills);
        fields.AddField("deaths", lu.Deaths);
        fields.AddField("score", lu.Score);

        bl_DataBase.Instance.UpdateUserData(fields, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="friends"></param>
    public static void SaveFriendList(string friends, Action callback = null)
    {
        bl_DataBase.Instance.SaveValue("friends", friends, callback);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void SendNewData(this LoginUserInfo userInfo, int kills, int deaths, int score)
    {
        userInfo.Score += score;
        userInfo.Kills += kills;
        userInfo.Deaths += deaths;
    }
}