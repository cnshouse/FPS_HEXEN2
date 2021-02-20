using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
using MFPS.ULogin;

public class bl_UserProfile : bl_LoginProBase
{

    [Header("References")]
    [SerializeField]private Text NameText = null;
    [SerializeField]private Text ProfileNameText = null;
    [SerializeField]private Text ScoreText = null;
    [SerializeField]private Text KillsText = null;
    [SerializeField]private Text DeathsText = null;
    [SerializeField]private Text PlayTimeText = null;
    [SerializeField]private Text CoinsText = null;
    [SerializeField] private Text BarCoinsText = null;
    public Text ClanText;
    [SerializeField]private Text LogText = null;
    [SerializeField]private GameObject ProfileWindow = null;
    [SerializeField]private GameObject SettingsWindow = null;
    [SerializeField]private GameObject ChangeNameWindow = null;
    [SerializeField]private GameObject SuccessWindow = null;
    [SerializeField]private GameObject ChangeNameButton = null;
    [SerializeField] private GameObject ChangePassButton = null;
    [SerializeField]private InputField CurrentPassNick = null;
    [SerializeField]private InputField NewNickInput = null;
    [SerializeField]private InputField CurrentPassInput = null;
    [SerializeField]private InputField NewPassInput = null;
    [SerializeField]private InputField RePassInput = null;

    private bool isSettingOpen = false;
    private bool isOpen = false;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if(bl_DataBase.Instance != null && bl_DataBase.Instance.isLogged)
        {
            OnLogin();
        }
        ChangeNameButton.SetActive(bl_LoginProDataBase.Instance.PlayerCanChangeNick);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_DataBase.OnUpdateData += OnUpdateData;
        SetupButtons();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_DataBase.OnUpdateData -= OnUpdateData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userInfo"></param>
    void OnUpdateData(LoginUserInfo userInfo)
    {
        OnLogin();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLogin()
    {
        if (bl_DataBase.Instance == null) return;

        if (ProfileNameText != null)
            ProfileNameText.text = bl_DataBase.Instance.LocalUser.NickName;
        if (NameText != null)
            NameText.text = bl_DataBase.Instance.LocalUser.NickName;
        ScoreText.text = bl_DataBase.Instance.LocalUser.Score.ToString();
        KillsText.text = bl_DataBase.Instance.LocalUser.Kills.ToString();
        DeathsText.text = bl_DataBase.Instance.LocalUser.Deaths.ToString();
        CoinsText.text = bl_DataBase.Instance.LocalUser.Coins.ToString();
        if (BarCoinsText != null)
            BarCoinsText.text = bl_DataBase.Instance.LocalUser.Coins.ToString();
#if CLANS
        if (bl_DataBase.Instance.LocalUser.Clan.haveClan)
        {
            ClanText.text = bl_DataBase.Instance.LocalUser.Clan.Name;
        }
        else
        {
            ClanText.transform.parent.gameObject.SetActive(false);
        }
#else
        ClanText.transform.parent.gameObject.SetActive(false);
#endif
        PlayTimeText.text = bl_DataBaseUtils.TimeFormat(bl_DataBase.Instance.LocalUser.PlayTime);
        gameObject.SetActive(true);
        SetupButtons();
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnSettings()
    {
        isSettingOpen = !isSettingOpen;
        SettingsWindow.SetActive(isSettingOpen);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnProfile()
    {
        isOpen = !isOpen;
        ProfileWindow.SetActive(isOpen);
        if (!isOpen)
        {
            SettingsWindow.SetActive(false);
            ChangeNameWindow.SetActive(false);
        }
        else
        {
            OnLogin();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeName()
    {
        if (bl_DataBase.Instance == null) return;

        string pass = CurrentPassNick.text;
        string nick = NewNickInput.text;
        if(pass != bl_DataBase.Instance.CacheAccessToken)
        {
            Debug.Log("Password doesn't match!");
            SetLog("Password doesn't match!");
            return;
        }
        if (string.IsNullOrEmpty(nick))
        {
            SetLog("Empty nick name");
            return;
        }
        if (nick.Length < 3)
        {
            SetLog("Nick name should have 3 or more characters");
            return;
        }
        StartCoroutine(SetChangeName(nick));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator SetChangeName(string nick)
    {
        WWWForm wf = CreateForm(false, true);
        wf.AddSecureField("id", bl_DataBase.Instance.LocalUser.LoginName);
        wf.AddSecureField("data", nick);
        wf.AddSecureField("type", 4);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(bl_DataBase.Instance.LocalUser.LoginName));

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Account), wf))
        {
            yield return www.SendWebRequest();

            if (www.error == null && !www.isNetworkError)
            {
                if (www.downloadHandler.text.Contains("success"))
                {
                    bl_DataBase.Instance.LocalUser.NickName = nick;
                    ProfileNameText.text = bl_DataBase.Instance.LocalUser.NickName;
                    NameText.text = bl_DataBase.Instance.LocalUser.NickName;
                    Debug.Log("Changed nick name!");
                    SuccessWindow.SetActive(true);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError(www.error);
            }
        }
    }

    public void ChangePass()
    {
        if (bl_DataBase.Instance == null) return;

        string cp = CurrentPassInput.text;
        string np = NewPassInput.text;
        string rp = RePassInput.text;

        if (cp != bl_DataBase.Instance.CacheAccessToken)
        {
            Debug.Log("Password doesn't match!");
            SetLog("Password doesn't match!");
            return;
        }
        if(np != rp)
        {
            Debug.Log("New password doesn't match!");
            SetLog("New password doesn't match!");
            return;
        }
        if(np.Length < bl_LoginProDataBase.Instance.MinPasswordLenght)
        {
            string t = string.Format("Password should have {0} or more character", bl_LoginProDataBase.Instance.MinPasswordLenght);
            Debug.Log(t);
            SetLog(t);
            return;
        }
        StartCoroutine(SetChangePass(cp, np));
    }

    IEnumerator SetChangePass(string pass, string newpass)
    {
        // Create instance of WWWForm
        WWWForm wf = CreateForm(FormHashParm.ID, true);
        //sets the mySQL query to the amount of rows to load
        wf.AddSecureField("id", bl_DataBase.Instance.LocalUser.ID);
        wf.AddSecureField("type", 1);
        wf.AddSecureField("password", pass);
        wf.AddSecureField("data", newpass);

        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Account), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (www.error == null && !www.isNetworkError)
            {
                if (result.Contains("success"))
                {
                    Debug.Log("Change password!");
                    bl_DataBase.Instance.CacheAccessToken = newpass;
                    SuccessWindow.SetActive(true);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug log.
                {
                    // ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
                SetLog(www.error);
            }
        }
    }

    void SetLog(string t)
    {
        if (LogText == null) return;
        LogText.text = t;
        Invoke("CleanLog", 5);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        if(bl_DataBase.Instance != null)
        bl_DataBase.Instance.LocalUser = new LoginUserInfo();
        if(bl_Lobby.Instance != null) { bl_Lobby.Instance.SignOut(); }
        if (bl_PhotonNetwork.Instance != null) bl_PhotonNetwork.LocalPlayer.NickName = string.Empty;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    /// <summary>
    /// 
    /// </summary>
    private void SetupButtons()
    {
        if (bl_DataBase.IsUserLogged)
        {
            ChangePassButton.SetActive(bl_DataBase.LocalUserInstance.authenticationType == AuthenticationType.ULogin);
        }
    }

    public void OpenRanking()
    {
        bl_RankingPro rp = FindObjectOfType<bl_RankingPro>();
        rp?.Open();
    }
    
    void CleanLog() { LogText.text = string.Empty; }

    private static bl_UserProfile _instance;
    public static bl_UserProfile Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<bl_UserProfile>();
            }
            return _instance;
        }
    }
}