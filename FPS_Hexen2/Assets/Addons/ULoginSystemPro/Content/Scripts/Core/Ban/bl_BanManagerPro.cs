using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using MFPS.ULogin;

public class bl_BanManagerPro : MonoBehaviour
{

    [SerializeField]private InputField SearchInput;
    [SerializeField]private InputField ReasonText;
    [SerializeField]private Text UserNameText;
    [SerializeField]private Text KillsText;
    [SerializeField]private Text ScoreText;
    [SerializeField]private Text DeathsText;
    [SerializeField]private Text PlayTimeText;
    [SerializeField]private Text IPText;
    [SerializeField]private Text StatusText;
    [SerializeField]private Text LogText;
    [SerializeField]private GameObject[] BanUI;
    [SerializeField] private Button[] StatusButtons;
    [SerializeField] private GameObject LoadingUI;

    private bl_LoginProDataBase DataBase;
    private LoginUserInfo CurrentUser;
    private bool isRequesting = false;
    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        DataBase = bl_LoginProDataBase.Instance;
        foreach(GameObject g in BanUI) { g.SetActive(false); }
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

        StartCoroutine(SearchUser(user));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator SearchUser(string user)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        WWWForm wf = new WWWForm();
        wf.AddField("name", user);
        wf.AddField("type", 2);
        using (UnityWebRequest w = UnityWebRequest.Post(DataBase.GetUrl(bl_LoginProDataBase.URLType.Account), wf))
        {
            yield return w.SendWebRequest();

            if (!w.isNetworkError && w.error == null)
            {
                string result = w.downloadHandler.text;
                string[] split = result.Split("|"[0]);
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
                    Debug.Log(result);
                    LogText.text = result;
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        isRequesting = false;
        LoadingUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Ban()
    {
        if(CurrentUser == null)
        {
            LogText.text = "Don't have any user to ban";
            return;
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

        StartCoroutine(BanUser());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator BanUser()
    {
        LoadingUI.SetActive(true);
        isRequesting = true;
        //Used for security check for authorization to modify database
        string hash = Md5Sum(CurrentUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        string reason = ReasonText.text;
        WWWForm wf = new WWWForm();
        wf.AddField("name", CurrentUser.LoginName);
        wf.AddField("reason", reason);
        wf.AddField("myIP", CurrentUser.IP);
        wf.AddField("mBy", "Admin");
        wf.AddField("type", 1);
        wf.AddField("hash", hash);

        using (UnityWebRequest w = UnityWebRequest.Post(DataBase.GetUrl(bl_LoginProDataBase.URLType.BanList), wf))
        {
            yield return w.SendWebRequest();

            if (w.error == null && !w.isNetworkError)
            {
                string result = w.downloadHandler.text;
                if (result.Contains("success"))
                {
                    StatusText.text = string.Format("<b>Status:</b> {0}", "Banned");
                    LogText.text = "User banned.";
                    BanUI[0].SetActive(false);
                    BanUI[1].SetActive(false);
                    BanUI[2].SetActive(true);
                }
                else
                {
                    Debug.Log(result);
                    LogText.text = result;
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        isRequesting = false;
        LoadingUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void UnBan()
    {
        if (CurrentUser == null)
        {
            LogText.text = "Don't have any user to ban";
            return;
        }
        if (isRequesting)
            return;

        StartCoroutine(UnBanUser());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator UnBanUser()
    {
        LoadingUI.SetActive(true);
        isRequesting = true;
        //Used for security check for authorization to modify database
        string hash = Md5Sum(CurrentUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        WWWForm wf = new WWWForm();
        wf.AddField("name", CurrentUser.LoginName);
        wf.AddField("mBy", "Admin");
        wf.AddField("type", 2);
        wf.AddField("hash", hash);

        using (UnityWebRequest w = UnityWebRequest.Post(DataBase.GetUrl(bl_LoginProDataBase.URLType.BanList), wf))
        {

            yield return w.SendWebRequest();

            if (w.error == null)
            {
                string result = w.downloadHandler.text;
                if (result.Contains("success"))
                {
                    StatusText.text = "<b>Status:</b> Normal User";
                    LogText.text = "User UnBanned.";
                    BanUI[0].SetActive(true);
                    BanUI[1].SetActive(true);
                    BanUI[2].SetActive(false);
                }
                else
                {
                    Debug.Log(result);
                    LogText.text = result;
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        isRequesting = false;
        LoadingUI.SetActive(false);
    }

    public void ChangeStatus(int newstatus)
    {
        StartCoroutine(IEChangeUserStatus(newstatus));
    }

    IEnumerator IEChangeUserStatus(int news)
    {
        foreach (Button g in StatusButtons) { g.interactable = false; }
        LoadingUI.SetActive(true);
        //Used for security check for authorization to modify database
        string hash = Md5Sum(CurrentUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        WWWForm wf = new WWWForm();
        wf.AddField("name", CurrentUser.LoginName);
        wf.AddField("status", news);
        wf.AddField("type", 3);
        wf.AddField("hash", hash);

        using (UnityWebRequest w = UnityWebRequest.Post(DataBase.GetUrl(bl_LoginProDataBase.URLType.BanList), wf))
        {
            yield return w.SendWebRequest();

            if (!w.isHttpError && !w.isNetworkError)
            {
                string result = w.downloadHandler.text;
                if (result.Contains("update"))
                {
                    foreach (Button g in StatusButtons) { g.interactable = true; }
                    StatusButtons[news].interactable = false;
                    StatusText.text = string.Format("<b>Status:</b> {0}", ((LoginUserInfo.Status)news).ToString());
                }
                else
                {
                    Debug.Log("Can't update status for: " + CurrentUser.LoginName + " error: " + result);
                    LogText.text = result;
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        LoadingUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    public void ShowUserInfo(MFPS.ULogin.LoginUserInfo info)
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

    /// <summary>
    /// Md5s Security Features
    /// </summary>
    public string Md5Sum(string input)
    {
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++) { sb.Append(hash[i].ToString("X2")); }
        return sb.ToString();
    }
}