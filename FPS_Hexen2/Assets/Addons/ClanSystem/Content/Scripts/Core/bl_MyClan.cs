using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

namespace MFPS.Addon.Clan
{
    public class bl_MyClan : MonoBehaviour
    {
        public GameObject MemberListUI;
        public GameObject JoinRequestListUI;
        public Transform JoinRequestPanel;
        public Transform MembersPanel;
        public Text ClanNameText;
        public Text LeaderText;
        public Text ScoreText;
        public Text DateText;
        public Text MembersText;
        public Text DescriptionText;
        public Text LastMemberText;
        public Text StateText;
        public Text InvitationText;
        public Button InvitationAllButton;
        public Button InvitationOLButton;
        public Button StatePublicButton;
        public Button StatePrivateButton;
        public InputField DescriptionInput;
        public GameObject EditInfoUI;
        public GameObject EditButton;
        public GameObject CantLeaveUI;
        private bool initialized = false;
        private List<GameObject> cacheInstanceUI = new List<GameObject>();

        private bool isClanPublic = false;
        private bool AllCanInvite = false;
        private bool editInfoOpen = false;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (bl_DataBase.Instance == null) return;

            if (!initialized)
            {
                StartCoroutine(GetUserClan());
            }
            else
            {
                Refresh();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DisplayInfo()
        {
            bl_ClanInfo i = bl_ClanManager.Instance.ClanInfo;
            ClanNameText.text = string.Format("<b>CLAN NAME:</b> {0}", i.Name);
            LeaderText.text = string.Format("<b>LEADER:</b> {0}", i.Leader().Name);
            ScoreText.text = string.Format("<b>SCORE:</b> {0}", i.Score);
            DateText.text = string.Format("<b>DATE:</b> {0}", i.Date);
            MembersText.text = string.Format("<b>MEMBERS:</b> {0} / 20", i.Members.Count);
            LastMemberText.text = string.Format("<b>LAST MEMBERS:</b> {0}", i.Members[i.Members.Count - 1].Name);
            DescriptionText.text = string.Format("<b>DESCRIPTIONS:\n</b><size=9>{0}</size>", i.Description);
            DescriptionInput.text = i.Description;
            StateText.text = string.Format("<b>STATE:</b> {0}", i.isPublic ? "PUBLIC" : "PRIVATE");
            InvitationText.text = string.Format("<b>INVITATIONS:</b> {0}", i.AllCanInvite ? "ALL" : "ONLY LEADER");
            StatePrivateButton.interactable = i.isPublic;
            StatePublicButton.interactable = !i.isPublic;
            InvitationAllButton.interactable = !i.AllCanInvite;
            InvitationOLButton.interactable = i.AllCanInvite;
            EditButton.SetActive((int)i.PlayerRole() > 1);
        }

        /// <summary>
        /// 
        /// </summary>
        void InstanceMembers()
        {
            if (bl_ClanManager.Instance.ClanInfo.Members.Count <= 0) return;

            bl_ClanInfo.ClanMember[] list = bl_ClanManager.Instance.ClanInfo.Members.ToArray();
            for (int i = 0; i < list.Length; i++)
            {
                GameObject m = Instantiate(MemberListUI) as GameObject;
                m.GetComponent<bl_MemberListUI>().Set(list[i], this);
                m.transform.SetParent(MembersPanel, false);
                cacheInstanceUI.Add(m);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GetUserClan()
        {
            bl_ClanManager.Instance.LoadingOverlays[1].SetActive(true);
            bl_ClanManager.Instance.LoadingOverlays[4].SetActive(true);
            int clanID = 0;
#if CLANS
       clanID = bl_DataBase.Instance.LocalUser.Clan.ID;
#endif
            WWWForm wf = new WWWForm();
            wf.AddField("type", 1);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanID);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("|"[0]);
                    if (split[0].Contains("yes"))
                    {
                        if (bl_ClanManager.Instance.ClanInfo == null) { bl_ClanManager.Instance.ClanInfo = new bl_ClanInfo(); }
                        bl_ClanManager.Instance.ClanInfo.Name = split[1];
                        bl_ClanManager.Instance.ClanInfo.Date = split[2];
                        DecompileMembers(split[3]);
                        DecompileClanRequests(split[4]);
                        bl_ClanManager.Instance.ClanInfo.Score = int.Parse(split[5]);
                        bl_ClanManager.Instance.ClanInfo.ID = clanID;
                        DecompileSettings(split[6]);
                        bl_ClanManager.Instance.ClanInfo.Description = split[7];
                        StartCoroutine(GetClanMembers());
                        DisplayInfo();
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
            }
            bl_ClanManager.Instance.LoadingOverlays[1].SetActive(false);
            initialized = true;
        }


        private void DecompileMembers(string line)
        {
            bl_ClanManager.Instance.ClanInfo.Members = new List<bl_ClanInfo.ClanMember>();
            bl_ClanManager.Instance.ClanInfo.SourceMembers = line;
            string[] split = line.Split(","[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                string[] info = split[i].Split("-"[0]);
                if (info.Length < 2) continue;
                bl_ClanInfo.ClanMember member = new bl_ClanInfo.ClanMember();
                member.ID = int.Parse(info[0]);
                member.Role = (ClanMemberRole)int.Parse(info[1]);
                bl_ClanManager.Instance.ClanInfo.Members.Add(member);
            }
        }

        private void DecompileClanRequests(string line)
        {
            bl_ClanManager.Instance.ClanInfo.ClanJoinRequests = new List<int>();
            string[] split = line.Split(","[0]);
            for (int i = 0; i < split.Length; i++)
            {
                if (string.IsNullOrEmpty(split[i])) continue;
                bl_ClanManager.Instance.ClanInfo.ClanJoinRequests.Add(int.Parse(split[i]));
            }
        }

        void DecompileSettings(string line)
        {
            string[] split = line.Split(","[0]);
            bl_ClanManager.Instance.ClanInfo.AllCanInvite = (int.Parse(split[0]) == 1 ? true : false);
            bl_ClanManager.Instance.ClanInfo.isPublic = (int.Parse(split[1]) == 1 ? true : false);
            AllCanInvite = bl_ClanManager.Instance.ClanInfo.AllCanInvite;
            isClanPublic = bl_ClanManager.Instance.ClanInfo.isPublic;
            bl_ClanManager.Instance.LoadingOverlays[0].SetActive(bl_ClanManager.Instance.ClanInfo.AllCanInvite);
            if ((int)bl_ClanManager.Instance.ClanInfo.PlayerRole() > 1 || bl_ClanManager.Instance.ClanInfo.AllCanInvite)
            {
                bl_ClanManager.Instance.LoadingOverlays[0].SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GetClanMembers()
        {
            WWWForm wf = new WWWForm();
            wf.AddField("type", 2);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("-"[0]);
                    for (int i = 0; i < split.Length; i++)
                    {
                        if (string.IsNullOrEmpty(split[i])) continue;
                        string[] info = split[i].Split("|"[0]);
                        int id = 0;
                        if (!int.TryParse(info[0], out id))
                        {
                            Debug.Log("corrupted member data: " + info[0]); continue;
                        }
                        if (info.Length < 2) continue;
                        var member = bl_ClanManager.Instance.ClanInfo.Members.Find(x => x.ID == id);
                        if (member != null)
                        {
                            member.Name = info[1];
                        }
                        else
                        {
                            bl_ClanManager.Instance.ClanInfo.Members.Add(new bl_ClanInfo.ClanMember()
                            {
                                Name = info[1],
                                ID = id,
                                Role = ClanMemberRole.Member,
                            });
                        }
                    }
                    InstanceMembers();
                    StartCoroutine(GetRequests());
                    DisplayInfo();
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GetRequests()
        {
            WWWForm wf = new WWWForm();
            wf.AddField("type", 6);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            string[] array = bl_ClanManager.Instance.ClanInfo.ClanJoinRequests.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", array);
            wf.AddField("userID", line);

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
                            if (i == 0) continue;
                            string[] info = split[i].Split(","[0]);
                            GameObject o = Instantiate(JoinRequestListUI) as GameObject;
                            o.GetComponent<bl_ClanJoinRequestUI>().Set(info[0], int.Parse(info[1]));
                            o.transform.SetParent(JoinRequestPanel, false);
                            cacheInstanceUI.Add(o);
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
            }
            bl_ClanManager.Instance.LoadingOverlays[4].SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Refresh()
        {
            ClearCacheUI();
            bl_ClanManager.Instance.ClanInfo = null;
            bl_ClanManager.Instance.ClanInfo = new bl_ClanInfo();
            StartCoroutine(GetUserClan());
        }

        /// <summary>
        /// 
        /// </summary>
        public void KickMember(bl_ClanInfo.ClanMember member, bool self)
        {
            string line = "";
            List<bl_ClanInfo.ClanMember> list = bl_ClanManager.Instance.ClanInfo.Members;
            int idx = list.FindIndex(x => x.ID == member.ID);
            list.RemoveAt(idx);
            for (int i = 0; i < list.Count; i++)
            {
                line += string.Format("{0}-{1},", list[i].ID, (int)list[i].Role);
            }
            StartCoroutine(ProcessKick(member, line, self));
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator ProcessKick(bl_ClanInfo.ClanMember member, string line, bool self)
        {
            bl_ClanManager.Instance.LoadingOverlays[4].SetActive(true);
            if (self) { bl_ClanManager.Instance.LoadingOverlays[1].SetActive(true); }
            WWWForm wf = new WWWForm();
            wf.AddField("type", 13);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            wf.AddField("settings", line);
            wf.AddField("userID", member.ID);
#if CLANS
        int dc = 0;
        if (bl_LoginProDataBase.Instance.DeleteEmptyClans)
        {
            dc = member.Role == ClanMemberRole.Leader ? 1 : 0;//if this is the leader, that means that the clan will be empty so we have to delete it from DB
            wf.AddField("desc", dc);
        }
#endif
            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("yes"))
                    {
                        if (self)
                        {
#if CLANS
                        bl_DataBase.Instance.LocalUser.Clan = new bl_ClanInfo();
#endif
                            initialized = false;
                            bl_ClanManager.Instance.LoadingOverlays[0].SetActive(true);
                            ClearCacheUI();
                            bl_ClanManager.Instance.ClanInfo = new bl_ClanInfo();
                            bl_ClanManager.Instance.Initialize();//reset the menu
                        }
                        else
                        {
                            Refresh();
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
            }
            if (self)
            {
                bl_ClanManager.Instance.LoadingOverlays[1].SetActive(false);
                bl_ClanManager.Instance.LoadingOverlays[4].SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void LeaveClan()
        {
#if CLANS
        bl_ClanInfo.ClanMember me = bl_ClanManager.Instance.ClanInfo.Members.Find(x => x.ID == bl_DataBase.Instance.LocalUser.ID);
        if (me == null) return;

        if (bl_DataBase.Instance.LocalUser.Clan.PlayerRole() != ClanMemberRole.Leader)
        {
            KickMember(me, true);
        }
        else
        {
            if(bl_ClanManager.Instance.ClanInfo.Members.Count > 1)
            {
                //can't leave yet
                CantLeaveUI.SetActive(true);
            }
            else
            {
                KickMember(me, true);
            }
        }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="ascend"></param>
        public void ChangeMemberRole(bl_ClanInfo.ClanMember member, bool ascend)
        {
            int cr = (int)member.Role;
            cr = ascend ? cr + 1 : cr - 1;
            string line = "";
            List<bl_ClanInfo.ClanMember> list = bl_ClanManager.Instance.ClanInfo.Members;
            int idx = list.FindIndex(x => x.ID == member.ID);
            list[idx].Role = (ClanMemberRole)cr;
            for (int i = 0; i < list.Count; i++)
            {
                line += string.Format("{0}-{1},", list[i].ID, (int)list[i].Role);
            }
            StartCoroutine(ProcessAscend(line));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        IEnumerator ProcessAscend(string line)
        {
            bl_ClanManager.Instance.LoadingOverlays[4].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 14);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            wf.AddField("settings", line);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("yes"))
                    {
                        Refresh();
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
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void EditInfo()
        {
            editInfoOpen = !editInfoOpen;
            EditInfoUI.SetActive(editInfoOpen);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isPublic"></param>
        public void SetClanState(bool isPublic)
        {
            isClanPublic = isPublic;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AllCan"></param>
        public void SetClanInvitationSettings(bool AllCan)
        {
            AllCanInvite = AllCan;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            string t = DescriptionInput.text;
            if (string.IsNullOrEmpty(t))
            {
                Debug.Log("Description can't be empty");
                return;
            }
            if (!Regex.IsMatch(t, @"^[a-zA-Z0-9_.; ]+$"))
            {
                Debug.Log("Description contain no allowed characters.");
                return;
            }
            StartCoroutine(UpdateSettings(t));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        IEnumerator UpdateSettings(string description)
        {
            bl_ClanManager.Instance.LoadingOverlays[1].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 16);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", bl_ClanManager.Instance.ClanInfo.ID);
            string settings = string.Format("{0},{1},", AllCanInvite ? 1 : 0, isClanPublic ? 1 : 0);
            wf.AddField("settings", settings);
            wf.AddField("desc", description);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
                        bl_ClanInfo i = bl_ClanManager.Instance.ClanInfo;
                        i.AllCanInvite = AllCanInvite;
                        i.isPublic = isClanPublic;
                        i.Description = description;
                        DisplayInfo();
                        editInfoOpen = false;
                        EditInfoUI.SetActive(editInfoOpen);
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
            }
            bl_ClanManager.Instance.LoadingOverlays[1].SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearCacheUI()
        {
            foreach (var item in cacheInstanceUI)
            {
                if (item != null)
                    Destroy(item);
            }
            cacheInstanceUI.Clear();
        }
    }
}