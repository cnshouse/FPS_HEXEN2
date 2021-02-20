using System;
using UnityEngine;
using System.Collections.Generic;
#if CLANS
using MFPS.Addon.Clan;
#endif

namespace MFPS.ULogin
{
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
        public string UserDate => rawData["user_date"];
        public Dictionary<string, string> rawData = new Dictionary<string, string>();
        public AuthenticationType authenticationType = AuthenticationType.ULogin;
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

        public void SetFriends(string line)
        {
            if (!string.IsNullOrEmpty(line))
            {
                FriendList.Clear();
                string[] splitFriends = line.Split('/');
                FriendList.AddRange(splitFriends);
            }
        }

        [Obsolete]
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

        public void ParseFullData(Dictionary<string, string> data)
        {
            rawData = data;
            LoginName = data["name"];
            NickName = data["nick"];
            Kills = int.Parse(data["kills"]);
            Deaths = int.Parse(data["deaths"]);
            Score = int.Parse(data["score"]);
            PlayTime = int.Parse(data["playtime"]);
            int st = int.Parse(data["status"]);
            UserStatus = (LoginUserInfo.Status)st;
            ID = int.Parse(data["id"]);//unique identifier of the player in database
            SetFriends(data["friends"]);
            Coins = int.Parse(data["coins"]);
            SavedIP = data["ip"];
#if SHOP
            ShopData = new bl_ShopUserData();
            ShopData.GetInfo(data);
#endif
            string meta = data["meta"];
            metaData = new bl_UserMetaData();
            if (!string.IsNullOrEmpty(meta))
            {
                metaData = JsonUtility.FromJson<bl_UserMetaData>(meta);
            }
        }
    }

    [Serializable]
    public class CustomAuthCredentials
    {
        public string UserName;
        public string NickName;
        public string UniqueID;
        public string Email;
        public AuthenticationType authenticationType = AuthenticationType.ULogin;
        public bool RequireNickName = true;

        public string GetUniquePassword()
        {
            char[] charArray = UniqueID.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}