using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class LoginUserInfo
{
    public string LoginName = "";
    public string NickName = "";
    public int ID = 0;
    public int Kills = 0;
    public int Deaths = 0;
    public int Score = 0;
    public int Coins = 0;
    public int PlayTime = 0;
    public string IP = "";
    public string SavedIP = "";
    public bl_UserMetaData metaData;
    public List<string> FriendList = new List<string>();
    public Status UserStatus = Status.NormalUser;
#if CLANS
    public bl_ClanInfo Clan = null;
#endif
#if SHOP
    public bl_ShopUserData ShopData = null;
#endif

    public enum Status
    {
        NormalUser = 0,
        Admin = 1,
        Moderator = 2,
        Banned = 3,
    }   

    public WWWForm AddData(WWWForm form)
    {
        form.AddField("kills", Kills);
        form.AddField("deaths", Deaths);
        form.AddField("score", Score);
        return form;
    }

    public void SetNewData(int score, int kills, int deaths)
    {
        Score += score;
        Kills += kills;
        Deaths += deaths;
    }

    public void SetFriends(string line)
    {
        if (!string.IsNullOrEmpty(line))
        {
            FriendList.Clear();
            string[] splitFriends = line.Split('/');
            FriendList.AddRange(splitFriends);
        }
    }

    public void ParseFullData(string[] data)
    {
        LoginName = data[1];
        NickName = data[2];
        Kills = int.Parse(data[3]);
        Deaths = int.Parse(data[4]);
        Score = int.Parse(data[5]);
        PlayTime = int.Parse(data[6]);
        int st = int.Parse(data[7]);
        UserStatus = (LoginUserInfo.Status)st;
        ID = int.Parse(data[8]);//unique identifier of the player in database
        SetFriends(data[9]);
        Coins = int.Parse(data[10]);
        SavedIP = data[11];
#if SHOP
        ShopData = new bl_ShopUserData();
        ShopData.GetInfo(data);
#endif
        string meta = data[15];
        metaData = new bl_UserMetaData();
        if (!string.IsNullOrEmpty(meta))
        {
            metaData = JsonUtility.FromJson<bl_UserMetaData>(meta);
        }
    }
}