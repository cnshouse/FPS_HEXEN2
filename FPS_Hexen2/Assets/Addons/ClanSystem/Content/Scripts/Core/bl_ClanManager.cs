using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System;

namespace MFPS.Addon.Clan
{
    public class bl_ClanManager : MonoBehaviour
    {

        public GameObject[] Windows;
        public GameObject[] LoadingOverlays;
        public GameObject TopClanUI;
        public GameObject ClanInvitationUI;
        public Transform ClanInvitationsPanel;
        public Transform TopClansPanel;
        public GameObject MyClanWindow;
        public GameObject CreateClanWindow;
        public InputField InviteInputField;
        public Text InviteLogText;
        public GameObject ClanInfoOverlay;
        public Text ClanNameText;
        public Text ScoreText;
        public Text MembersText;
        public Text DateText;
        public Text StatusText;
        public Text DescriptionText;
        public Text ClanInfoLogText;
        public GameObject InvitationsButtons;
        public GameObject JoinButton;
        public GameObject RequestJoinButton;
        public GameObject AskComfirmationUI;
        public Animator WindowAnim;
        public CanvasGroup FadeAlpha;

        public bl_ClanInfo ClanInfo = new bl_ClanInfo();
        private bl_ClanInfo DisplayingClanInfo = null;
        private List<GameObject> cacheInvitations = new List<GameObject>();
        private List<GameObject> cacheTopClans = new List<GameObject>();
        private Action CurrentAskAction = null;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
#if ULSP
            if (bl_DataBase.Instance == null && bl_LoginProDataBase.Instance.ForceLoginScene)
            {
                bl_UtilityHelper.LoadLevel("Login");
                return;
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            if (bl_DataBase.Instance == null || !bl_DataBase.Instance.isLogged)
            {
                Debug.Log("You need logged before open this scene.");
                bl_UtilityHelper.LoadLevel("Login");
                return;
            }
            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize()
        {
#if CLANS
        if (bl_DataBase.Instance.LocalUser.Clan == null || !bl_DataBase.Instance.LocalUser.Clan.haveClan)
        {
            CreateClanWindow.SetActive(true);
            MyClanWindow.SetActive(false);
        }
        else
        {
            CreateClanWindow.SetActive(false);
            MyClanWindow.SetActive(true);
        }
#endif
            ClearCacheUI();
            ClanInfoOverlay.SetActive(false);
            StartCoroutine(GetTopClans(false));
            foreach (var item in Windows)
            {
                item.SetActive(false);
            }
            Windows[0].SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CheckTops()
        {
            if (cacheTopClans.Count <= 0) { StartCoroutine(GetTopClans(true)); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OpenWindow(int id)
        {
            StopCoroutine("ChangeWindowAnimated");
            StartCoroutine("ChangeWindowAnimated", id);
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator ChangeWindowAnimated(int id)
        {
            WindowAnim.Play("play", 0, 0);
            float t = WindowAnim.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(t * 0.5f);
            foreach (var item in Windows)
            {
                item.SetActive(false);
            }
            Windows[id].SetActive(true);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerator GetTopClans(bool justTopClans)
        {
            LoadingOverlays[4].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 0);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("\n"[0]);
                    if (split.Length > 0 && !split[0].Contains("empty"))
                    {
                        for (int i = 0; i < split.Length; i++)
                        {
                            if (string.IsNullOrEmpty(split[i]) || !split[i].Contains("|")) continue;
                            string[] info = split[i].Split("|"[0]);
                            GameObject g = Instantiate(TopClanUI) as GameObject;
                            g.GetComponent<bl_TopClanUI>().Set(info[0], info[2], info[1]);
                            g.transform.SetParent(TopClansPanel, false);
                            cacheTopClans.Add(g);
                        }
                    }
#if CLANS
                if (!justTopClans)
                {
                    bl_ClanInfo playerClan = bl_DataBase.Instance.LocalUser.Clan;
                    if ((playerClan == null || !playerClan.haveClan) && playerClan.MyInvitations.Count > 0)
                    {
                        StartCoroutine(GetUserInvitations());
                    }
                    else { LoadingOverlays[4].SetActive(false); }
                }
                else
                {
                    LoadingOverlays[4].SetActive(false);
                }
#endif
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
        IEnumerator GetUserInvitations()
        {
#if CLANS
        string[] ids = bl_DataBase.Instance.LocalUser.Clan.MyInvitations.Select(x => x.ToString()).ToArray();
        string line = string.Join(",", ids);
        if(line.Length > 0) { line += ","; }

        WWWForm wf = new WWWForm();
        wf.AddField("type", 15);
        wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
        wf.AddField("clanID", line);

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
                        GameObject g = Instantiate(ClanInvitationUI) as GameObject;
                        g.GetComponent<bl_ClanJoinRequestUI>().SetClanRequest(info[0], info[1], info[2]);
                        g.transform.SetParent(ClanInvitationsPanel, false);
                        cacheInvitations.Add(g);
                    }
                }
                else
                {
                    if(!string.IsNullOrEmpty(t))
                    Debug.Log(t);
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        LoadingOverlays[4].SetActive(false);
#else
            yield break;
#endif
        }

        public void InvitePlayer()
        {
            string pn = InviteInputField.text;
            if (string.IsNullOrEmpty(pn))
            {
                InviteLogText.text = "Player Nick Name can't be empty";
                return;
            }
            StartCoroutine(SendInvitation(pn));
        }

        IEnumerator SendInvitation(string nickName)
        {
            LoadingOverlays[0].SetActive(true);
#if CLANS
        int clanID = bl_DataBase.Instance.LocalUser.Clan.ID;
#else
            int clanID = 0;
#endif
            WWWForm wf = new WWWForm();
            wf.AddField("type", 5);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanID);
            wf.AddField("userID", nickName);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("|"[0]);
                    if (split[0].Contains("done"))
                    {
                        InviteLogText.text = "<color=green>Player Invited</color>";
                    }
                    else
                    {
                        InviteLogText.text = t;
                        Debug.Log(t);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            LoadingOverlays[0].SetActive(false);
        }

        public void AcceptJoinRequest(int id, Action callback)
        {
            int[] array = ClanInfo.ClanJoinRequests.Where(x => x != id).ToArray();
            string[] str = array.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", str);
            if (line.Length > 0) { line += ","; }
            StartCoroutine(AcceptJoinRequest(line, id, array, callback));
        }

        IEnumerator AcceptJoinRequest(string line, int id, int[] newArray, Action callBack)
        {
            LoadingOverlays[2].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 7);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", ClanInfo.ID);
            wf.AddField("userID", id);
            wf.AddField("settings", line);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
                        ClanInfo.ClanJoinRequests.Clear();
                        ClanInfo.ClanJoinRequests.AddRange(newArray);
                        bl_MyClan mc = FindObjectOfType<bl_MyClan>();
                        if (mc != null) { mc.Refresh(); }
                        callBack.Invoke();
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
            LoadingOverlays[2].SetActive(false);
        }

        public void DenyUserJoinRequest(int id, Action callBack)
        {
            int[] array = ClanInfo.ClanJoinRequests.Where(x => x != id).ToArray();
            string[] str = array.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", str);
            if (line.Length > 0) { line += ","; }
            StartCoroutine(DenyUserJoinRequest(line, array, callBack));
        }

        IEnumerator DenyUserJoinRequest(string line, int[] newArray, Action callBack)
        {
            LoadingOverlays[2].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 8);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", ClanInfo.ID);
            wf.AddField("userID", line);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
                        ClanInfo.ClanJoinRequests.Clear();
                        ClanInfo.ClanJoinRequests.AddRange(newArray);
                        Debug.Log("Request update.");
                        callBack.Invoke();
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
            LoadingOverlays[2].SetActive(false);
        }

        public void AcceptClanInvitation()
        {
            if (DisplayingClanInfo == null) return;
            StartCoroutine(JoinToClan(DisplayingClanInfo, false));
        }

        public void AcceptClanInvitation(bl_ClanInfo info, bool updateClanInfo)
        {
            StartCoroutine(JoinToClan(info, updateClanInfo));
        }

        IEnumerator JoinToClan(bl_ClanInfo info, bool updateClanInfo)
        {
            LoadingOverlays[4].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 10);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", info.ID);
            wf.AddField("userID", bl_DataBase.Instance.LocalUser.ID);
            wf.AddField("settings", bl_DataBase.Instance.LocalUser.Score);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
                        if (updateClanInfo)
                        {
                            bl_ClanSearch cs = FindObjectOfType<bl_ClanSearch>();
                            yield return StartCoroutine(cs.Search(info.Name, false));
                            info = cs.LastSearchInfo;
                        }
#if CLANS
                    bl_DataBase.Instance.LocalUser.Clan = info;
#endif
                        if (!updateClanInfo)
                        {
#if CLANS
                        bl_ClanInfo.ClanMember me = new bl_ClanInfo.ClanMember();
                        me.ID = bl_DataBase.Instance.LocalUser.ID;
                        me.Name = bl_DataBase.Instance.LocalUser.NickName;
                        me.Role = ClanMemberRole.Member;
                        bl_DataBase.Instance.LocalUser.Clan.Members.Add(me);
#endif
                        }
                        ClearCacheUI();
                        ClanInfoOverlay.SetActive(false);
                        CreateClanWindow.SetActive(false);
                        MyClanWindow.SetActive(true);
                        OpenWindow(1);
                        ClanInfoLogText.text = "Joined to the clan: " + info.Name;
                        Debug.Log("Joined to the clan");
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
            LoadingOverlays[4].SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void DenyClanInvitation()
        {
            if (DisplayingClanInfo == null) return;

            List<int> ids = new List<int>();
#if CLANS
        ids.AddRange(bl_DataBase.Instance.LocalUser.Clan.MyInvitations.ToArray());
#endif
            ids.Remove(DisplayingClanInfo.ID);
            string[] all = ids.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", all);
            if (line.Length > 0) { line += ","; }
            StartCoroutine(DeclineJoinToClan(line, ids));
        }

        public void DenyClanInvitation(int clanID, Action callback)
        {
            List<int> ids = new List<int>();
#if CLANS
        ids.AddRange(bl_DataBase.Instance.LocalUser.Clan.MyInvitations.ToArray());
#endif
            ids.Remove(clanID);
            string[] all = ids.Select(x => x.ToString()).ToArray();
            string line = string.Join(",", all);
            if (line.Length > 0) { line += ","; }
            StartCoroutine(DeclineJoinToClan(line, ids, callback));
        }

        IEnumerator DeclineJoinToClan(string line, List<int> newList, Action callback = null)
        {
            LoadingOverlays[4].SetActive(false);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 11);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("settings", line);
            wf.AddField("userID", bl_DataBase.Instance.LocalUser.ID);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
#if CLANS
                    bl_DataBase.Instance.LocalUser.Clan.MyInvitations = newList;
#endif
                        if (callback == null)
                        {
                            InvitationsButtons.SetActive(false);
                            ClanInfoLogText.text = "Invitation Declined";
                        }
                        else
                        {
                            callback.Invoke();
                        }
                        Debug.Log("Invitation Declined");
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
            LoadingOverlays[4].SetActive(false);
        }

        public void RequestJoinToClan()
        {
            if (DisplayingClanInfo == null) return;
            StartCoroutine(RequestJoinToClan(DisplayingClanInfo));
        }

        IEnumerator RequestJoinToClan(bl_ClanInfo info)
        {
            LoadingOverlays[4].SetActive(false);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 12);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", info.ID);
            wf.AddField("userID", bl_DataBase.Instance.LocalUser.ID);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
                        InvitationsButtons.SetActive(false);
                        RequestJoinButton.SetActive(false);
                        ClanInfoLogText.text = "Join request send";
                        Debug.Log("Join request send");
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
            LoadingOverlays[4].SetActive(false);
        }

        public void AskComfirmationFor(Action action)
        {
            CurrentAskAction = action;
            AskComfirmationUI.SetActive(true);
        }

        public void ComfirmAction()
        {
            if (CurrentAskAction == null) return;
            AskComfirmationUI.SetActive(false);
            CurrentAskAction.Invoke();
        }

        public void CancelAction()
        {
            CurrentAskAction = null;
            AskComfirmationUI.SetActive(false);
        }

        public void DisplayClanInfo(bl_ClanInfo info)
        {
            DisplayingClanInfo = info;
            ClanNameText.text = info.Name;
            ScoreText.text = info.Score.ToString();
            MembersText.text = string.Format("{0} / 20", info.MembersCount);
            DateText.text = info.Date;
            DescriptionText.text = info.Description;
            StatusText.text = info.isPublic ? "PUBLIC" : "PRIVATE";
            bool isFull = (info.MembersCount >= 20);
            if (isFull)
            {
                StatusText.text += " [FULL]";
            }
#if CLANS
        if (bl_DataBase.Instance.LocalUser.Clan == null && !isFull || !bl_DataBase.Instance.LocalUser.Clan.haveClan && !isFull)
        {
            if (bl_DataBase.Instance.LocalUser.Clan != null && bl_DataBase.Instance.LocalUser.Clan.MyInvitations.Contains(info.ID))
            {
                InvitationsButtons.SetActive(true);
                JoinButton.SetActive(false);
                RequestJoinButton.SetActive(false);
            }
            else
            {
                InvitationsButtons.SetActive(false);
                JoinButton.SetActive(info.isPublic);
                RequestJoinButton.SetActive(!info.isPublic);
            }
        }
        else
        {
            InvitationsButtons.SetActive(false);
            JoinButton.SetActive(false);
            RequestJoinButton.SetActive(false);
        }
#endif
            ClanInfoOverlay.SetActive(true);
        }

        public void ToLobby()
        {
            StartCoroutine(DofadeOut());
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearCacheUI()
        {
            foreach (var item in cacheInvitations)
            {
                if (item != null)
                    Destroy(item);
            }
            cacheTopClans.ForEach(x => Destroy(x));
            cacheTopClans.Clear();
            cacheInvitations.Clear();
        }

        IEnumerator DofadeOut()
        {
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime;
                FadeAlpha.alpha = d;
                yield return null;
            }
            bl_UtilityHelper.LoadLevel("MainMenu");
        }
        private static bl_ClanManager _instance;
        public static bl_ClanManager Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ClanManager>(); }
                return _instance;
            }
        }
    }
}