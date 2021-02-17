using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

namespace MFPS.Addon.Clan
{
    [Serializable]
    public class bl_ClanInfo
    {
        public int ID = -1;
        public string Name = "";
        public int Score = 0;
        public string Date = "";
        public string Description = "";
        public bool AllCanInvite = true;
        public bool isPublic = true;
        public List<ClanMember> Members = new List<ClanMember>();
        public string SourceMembers = string.Empty;
        public List<int> ClanJoinRequests = new List<int>();

        public bool haveClan => ID != -1;
        public List<int> MyInvitations = new List<int>();

        /// <summary>
        /// 
        /// </summary>
        public bl_ClanInfo()
        {
            ID = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ClanMemberRole PlayerRole()
        {
            if (Members.Count > 0 && bl_DataBase.Instance.LocalUser != null)
            {
                var member = Members.Find(x => x.ID == bl_DataBase.Instance.LocalUser.ID);
                if (member != null)
                    return member.Role;
            }
            return ClanMemberRole.Member;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="split"></param>
        public void GetSplitInfo(string[] split)
        {
            ID = int.Parse(split[12]);
            string[] splitInvitations = split[13].Split(","[0]);
            MyInvitations.Clear();
            for (int i = 0; i < splitInvitations.Length; i++)
            {
                if (string.IsNullOrEmpty(splitInvitations[i])) continue;
                MyInvitations.Add(int.Parse(splitInvitations[i]));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="split"></param>
        public void GetSplitInfo(Dictionary<string, string> split)
        {
            ID = int.Parse(split["clan"]);
            string[] splitInvitations = split["clan_invitations"].Split(", "[0]);
            MyInvitations.Clear();
            for (int i = 0; i < splitInvitations.Length; i++)
            {
                if (string.IsNullOrEmpty(splitInvitations[i])) continue;
                MyInvitations.Add(int.Parse(splitInvitations[i]));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetClanBasicInfo(Action callback = null)
        {
            if (ID == -1)
            {
                callback?.Invoke();
                yield break;
            }

            WWWForm wf = new WWWForm();
            wf.AddField("type", 4);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", ID);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("|"[0]);
                    if (split[0].Contains("yes"))
                    {
                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i])) continue;
                            Name = split[1];
                            SourceMembers = split[2];
                            DecompileMembers(split[2]);
                        }
                    }
                    else
                    {
                        Debug.Log(t);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
                callback?.Invoke();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void DecompileMembers(string line)
        {
            Members = new List<ClanMember>();
            string[] split = line.Split(","[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split("-"[0]);
                ClanMember member = new ClanMember();
                member.ID = int.Parse(info[0]);
                member.Role = (ClanMemberRole)int.Parse(info[1]);
                Members.Add(member);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        public void DecompileSettings(string line)
        {
            string[] split = line.Split(","[0]);
            AllCanInvite = (int.Parse(split[0]) == 1 ? true : false);
            isPublic = (int.Parse(split[1]) == 1 ? true : false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ClanMember Leader()
        {
            return Members.Find(x => x.Role == ClanMemberRole.Leader);
        }

        public int MembersCount { get { return Members.Count; } }

        [Serializable]
        public class ClanMember
        {
            public int ID = 0;
            public string Name = "";
            public ClanMemberRole Role = ClanMemberRole.Member;

            public string GetNameWithRole() { return string.Format("{0} <size=9><b>[{1}]</b></size>", Name, Role.ToString().ToUpper()); }
        }
    }

    public enum ClanMemberRole
    {
        Member = 0,
        Officer = 1,
        Commander = 2,
        Leader = 3,
    }

    [Serializable]
    public class bl_ClanRequestStatus
    {
        public int RequestID = 0;
        public int Status = 0;
    }
}