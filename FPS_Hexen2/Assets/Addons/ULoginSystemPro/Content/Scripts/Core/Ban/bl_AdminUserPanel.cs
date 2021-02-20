using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

namespace MFPS.ULogin
{
    public class bl_AdminUserPanel : bl_LoginProBase
    {
        [SerializeField] private InputField SearchInput = null;
        [SerializeField] private InputField ReasonText = null;
        [SerializeField] private Text UserNameText = null;
        [SerializeField] private Text KillsText = null;
        [SerializeField] private Text ScoreText = null;
        [SerializeField] private Text DeathsText = null;
        [SerializeField] private Text PlayTimeText = null;
        [SerializeField] private Text IPText = null;
        [SerializeField] private Text StatusText = null;
        [SerializeField] private Text LogText = null;
        [SerializeField] private GameObject[] BanUI = null;
        [SerializeField] private Button[] StatusButtons = null;
        [SerializeField] private GameObject LoadingUI = null;
        public bl_ConfirmationWindow confirmationWindow;

        private LoginUserInfo CurrentUser;
        private bool isRequesting = false;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            foreach (GameObject g in BanUI) { g.SetActive(false); }
            foreach (Button g in StatusButtons) { g.gameObject.SetActive(false); }
            LoadingUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Search()
        {
            string user = SearchInput.text;
            if (string.IsNullOrEmpty(user))
                return;
            if (isRequesting)
                return;

            SetBusy(true);
            WWWForm wf = CreateForm(false, true);
            wf.AddSecureField("name", user);
            wf.AddSecureField("type", 4);
            wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(user));

            var url = GetURL(bl_LoginProDataBase.URLType.Admin);
            WebRequest.POST(url, wf, (result) =>
             {
                 if (result.isError)
                 {
                     result.PrintError();
                     return;
                 }

                 string raw = result.RawText;
                 string[] split = raw.Split("|"[0]);
                 if (split[0].Contains("success"))
                 {
                     LoginUserInfo info = new LoginUserInfo();
                     info.LoginName = split[1];
                     info.Kills = split[2].ToInt();
                     info.Deaths = split[3].ToInt();
                     info.Score = split[4].ToInt();
                     info.IP = split[5];
                     info.UserStatus = (LoginUserInfo.Status)split[6].ToInt();
                     info.PlayTime = split[7].ToInt();
                     info.NickName = split[8];
                     info.ID = split[9].ToInt();
                     ShowUserInfo(info);
                     CurrentUser = info;
                     LogText.text = "information obtained.";
                     foreach (GameObject g in BanUI) { g.SetActive(true); }
                     if (info.UserStatus == LoginUserInfo.Status.Banned)
                     {
                         BanUI[0].SetActive(false);
                         BanUI[1].SetActive(false);
                     }
                     else
                     {
                         BanUI[2].SetActive(false);
                     }
                     if (info.UserStatus != LoginUserInfo.Status.Banned)
                     {
                         foreach (Button g in StatusButtons) { g.gameObject.SetActive(true); g.interactable = true; }
                         StatusButtons[(int)info.UserStatus].interactable = false;
                     }
                     else { foreach (Button g in StatusButtons) { g.gameObject.SetActive(false); } }
                 }
                 else
                 {
                     Debug.Log(raw);
                     LogText.text = raw;
                 }
                 SetBusy(false);
             });
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetStatsOfCurrentUser()
        {
            if(CurrentUser == null || string.IsNullOrEmpty(CurrentUser.NickName))
            {
                Debug.LogWarning("There's not user selected.");
                return;
            }

            confirmationWindow.AskConfirmation("Reset user statistics?", () =>
            {
                SetBusy(true);
                string data = "kills='0',deaths='0',score='0',playtime='0'";
                WWWForm wf = CreateForm(false, false);
                wf.AddSecureField("name", CurrentUser.ID);
                wf.AddSecureField("type", 5);
                wf.AddSecureField("unsafe", data);
                wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.ID.ToString()));

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                 {
                     if (result.isError)
                     {
                         result.PrintError();
                         return;
                     }

                     if (result.Text.Contains("done"))
                     {
                         CurrentUser.Kills = 0;
                         CurrentUser.Deaths = 0;
                         CurrentUser.Score = 0;
                         CurrentUser.PlayTime = 0;
                         ShowUserInfo(CurrentUser);
                         LogText.text = "Player stats updated.";
                     }
                     else
                     {
                         result.Print(true);
                     }
                     SetBusy(false);
                 });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Ban()
        {
            if (CurrentUser == null)
            {
                LogText.text = "Don't have any user to ban";
                return;
            }
            if(CurrentUser.UserStatus == LoginUserInfo.Status.Admin)
            {
                LogText.text = "Admins can't be banned.";
                return;
            }
            if(CurrentUser.UserStatus == LoginUserInfo.Status.Moderator)
            {
                if (bl_DataBase.Instance == null || bl_DataBase.Instance.LocalUser.UserStatus != LoginUserInfo.Status.Admin)
                {
                    LogText.text = "You have to be logged as Admin to ban a Moderator."; return;
                }
            }
            string reason = ReasonText.text;
            if (string.IsNullOrEmpty(reason))
            {
                Debug.Log("Need have an reason to ban, no?");
                LogText.text = "Write an reason to ban this user.";
                return;
            }
            if (isRequesting)
                return;

            confirmationWindow.AskConfirmation($"Ban {CurrentUser.NickName}?", () =>
            {
                SetBusy(true);
                WWWForm wf = new WWWForm();
                wf.AddField("name", CurrentUser.LoginName);
                wf.AddField("data", reason);
                wf.AddField("ip", CurrentUser.IP);
                var author = (!bl_DataBase.IsUserLogged || string.IsNullOrEmpty(bl_DataBase.Instance.LocalUser.NickName))
                ? "Admin" : bl_DataBase.Instance.LocalUser.NickName;
                wf.AddField("author", author);
                wf.AddField("type", 1);
                wf.AddField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.LoginName));

                Debug.Log($"Set author: {author}");
                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        CurrentUser.UserStatus = LoginUserInfo.Status.Banned;
                        ShowUserInfo(CurrentUser);
                        LogText.text = "Player banned.";
                        BanUI[0].SetActive(false);
                        BanUI[1].SetActive(false);
                        BanUI[2].SetActive(true);
                    }
                    else
                    {
                        result.Print(true);
                    }
                    SetBusy(false);
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void UnBan()
        {
            if (CurrentUser == null || CurrentUser.UserStatus != LoginUserInfo.Status.Banned) return;
            if (isRequesting)
                return;

            confirmationWindow.AskConfirmation($"Restore {CurrentUser.NickName} account?", () =>
            {
                SetBusy(true);
                WWWForm wf = new WWWForm();
                wf.AddField("name", CurrentUser.LoginName);
                wf.AddField("type", 2);
                wf.AddField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.LoginName));

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        CurrentUser.UserStatus = LoginUserInfo.Status.NormalUser;
                        ShowUserInfo(CurrentUser);
                        LogText.text = "Player account restored";
                        BanUI[0].SetActive(true);
                        BanUI[1].SetActive(true);
                        BanUI[2].SetActive(false);
                    }
                    else
                    {
                        result.Print(true);
                    }
                    SetBusy(false);
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeStatus(int newstatus)
        {
            if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.LoginName)) return;
            if((LoginUserInfo.Status)newstatus == LoginUserInfo.Status.Admin || (LoginUserInfo.Status)newstatus == LoginUserInfo.Status.Moderator)
            {
#if !UNITY_EDITOR
                if(bl_DataBase.Instance == null || bl_DataBase.Instance.LocalUser.UserStatus != LoginUserInfo.Status.Admin)
                {
                    LogText.text = "Only Admins can rise other Admins or Moderators.";
                    return;
                }
#endif
            }
            if (isRequesting)
                return;

            confirmationWindow.AskConfirmation($"Change account role?", () =>
            {
                foreach (Button g in StatusButtons) { g.interactable = false; }
                SetBusy(true);
                WWWForm wf = new WWWForm();
                wf.AddField("name", CurrentUser.LoginName);
                wf.AddField("type", 3);
                wf.AddField("data", newstatus);
                wf.AddField("hash", bl_DataBaseUtils.CreateSecretHash(CurrentUser.LoginName));

                var url = GetURL(bl_LoginProDataBase.URLType.Admin);
                WebRequest.POST(url, wf, (result) =>
                {
                    if (result.isError)
                    {
                        result.PrintError();
                        return;
                    }

                    if (result.resultState == ULoginResult.Status.Success)
                    {
                        CurrentUser.UserStatus = (LoginUserInfo.Status)newstatus;
                        ShowUserInfo(CurrentUser);
                        LogText.text = "Player role changed.";
                        foreach (Button g in StatusButtons) { g.interactable = true; }
                        StatusButtons[newstatus].interactable = false;
                    }
                    else
                    {
                        result.Print(true);
                    }
                    SetBusy(false);
                });
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isBusy"></param>
        public void SetBusy(bool isBusy)
        {
            isRequesting = isBusy;
            LoadingUI.SetActive(isBusy);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public void ShowUserInfo(LoginUserInfo info)
        {
            UserNameText.text = string.Format("<b>Name:</b> {0}", info.NickName);
            KillsText.text = string.Format("<b>Kills:</b> {0}", info.Kills);
            DeathsText.text = string.Format("<b>Deaths:</b> {0}", info.Deaths);
            ScoreText.text = string.Format("<b>Score:</b> {0}", info.Score);
            IPText.text = string.Format("<b>IP:</b> {0}", info.IP);
            StatusText.text = string.Format("<b>Status:</b> {0}", info.UserStatus.ToString());
            PlayTimeText.text = string.Format("<b>Play Time:</b> {0}", bl_DataBaseUtils.TimeFormat(info.PlayTime));
        }

        public void LoadLevel(string l)
        {
            bl_UtilityHelper.LoadLevel(l);
        }
    }
}