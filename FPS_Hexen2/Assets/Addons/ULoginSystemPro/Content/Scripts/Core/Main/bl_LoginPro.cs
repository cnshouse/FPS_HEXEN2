using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections.Generic;
using MFPS.ULogin;

public class bl_LoginPro : MonoBehaviour
{

    [Header("References")]
    [SerializeField]private GameObject[] Panels = null;
    [SerializeField]private GameObject LoadingUI = null;
    [SerializeField]private GameObject DataBasePrefab = null;
    [SerializeField]private GameObject TermsAndConditions = null;
    [SerializeField] private GameObject AdminPanelButon = null;
    [SerializeField] private GameObject ClanButton = null;
    [SerializeField] private GameObject OutdatedUI = null;
    [SerializeField]private Animator PanelAnim = null;
    [SerializeField]private Text LogText = null;
    [SerializeField]private Text LoginSuccessText = null;
    [SerializeField]private CanvasGroup FadeAlpha = null;
    public Button GuestButton;
    
    public UnityEvent OnLogin;
    public UnityEvent OnLogOut;

    private bool isRequesting = false;
    private bl_DataBase DataBase;
    private bool isLogin = false;
    private LoginUserInfo LocalUserInfo;
    private string lastKey = string.Empty;
    private bl_BanSystem Ban;
    private string currentIP;
    private bl_SignIn SignIn;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        if (FindObjectOfType<bl_DataBase>() == null)
        {
            Instantiate(DataBasePrefab);
        }
        DataBase = FindObjectOfType<bl_DataBase>();
        Ban = FindObjectOfType<bl_BanSystem>();
        SignIn = transform.GetComponentInChildren<bl_SignIn>(true);
        if(GuestButton != null) { GuestButton.interactable = false; }
        AdminPanelButon.SetActive(false);
        ClanButton.SetActive(false);
        GuestButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        ChangePanel(0);
        LoadingUI.SetActive(false);
        StartCoroutine(DoStartSequence());
    }

#region Login
    /// <summary>
    /// 
    /// </summary>
    public void Login(string user,string pass)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(LoginProcess(user, pass));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator LoginProcess(string user,string pass)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");

        //sets the mySQL query to the amount of rows to load
        Dictionary<string, string> formData = new Dictionary<string, string>();
        formData.Add("name",user);
        formData.Add("password", pass);

        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Login), formData))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;
            if (bl_LoginProDataBase.Instance.FullLogs)
            {
                Debug.Log("Login Result: " + result);
            }
            //check if we have some error
            if (www.isNetworkError == false && !www.isHttpError)
            {
                //decompile information
                string[] data = result.Split("|"[0]);

                if (data[0].Contains("success"))
                {
                    //'Decompile' information from response
                    LoginUserInfo info = new LoginUserInfo();
                    info.ParseFullData(data);
                    info.IP = currentIP;
#if CLANS
                    info.Clan = new bl_ClanInfo();
                    info.Clan.GetSplitInfo(data);
                    yield return StartCoroutine(info.Clan.GetClanBasicInfo());
                    ClanButton.SetActive(true);
#endif
                    //send information to local database
                    DataBase.OnLogin(info);
                    DataBase.CacheAccessToken = pass;
                    LocalUserInfo = info;
                    SignIn.OnLogin(info);
                    if(GuestButton != null) { GuestButton.gameObject.SetActive(false); }
                    SetLogText("Sign In success!");
                    //detect if this account is banned before load next level
                    if (bl_LoginProDataBase.Instance.DetectBan && Ban.CheckBanAccount(info.LoginName))
                    {
                        yield break;//just stop here due this account is banned
                    }
                    //if it's OK, load next level of show continue menu
                    isLogin = true;
                    if (OnLogin != null) { OnLogin.Invoke(); }
                    //check if session has to be remember
                    if(bl_LoginProDataBase.Instance.rememberMeBehave == RememberMeBehave.RememberSession)
                    {
                        if(SignIn != null && SignIn.RememberMe())
                        {
                            //if it's so, encrypt and save the authentication credentials that the user used.
                            bl_LoginProDataBase.Instance.RememberCredentials = $"{user}:{pass}";
                        }
                    }
                    yield return new WaitForSeconds(1f);
                    if (bl_LoginProDataBase.Instance.AutomaticallyLoadScene)//load the scene after login success (without continue menu)
                    {
                        Continue();
                    }
                    else
                    {
                        LoginSuccessText.text = string.Format("Welcome <b>{0}</b>", info.NickName);
                        if (info.UserStatus == LoginUserInfo.Status.Admin || info.UserStatus == LoginUserInfo.Status.Moderator)
                        {
                            AdminPanelButon.SetActive(true);
                        }
                        ChangePanel(3);
                    }
                }
                else//Wait, have a error?, please contact me for help with the result of next debug log.
                {
                    //Some error with the server setup.
                   // Debug.Log(result);
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
        }
        bl_ULoginLoadingWindow.Instance?.SetActive(false);
        LoadingUI.SetActive(false);
        isRequesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PlayAsGuest()
    {
        DataBase.isGuest = true;
        Continue();
    }
#endregion

#region SignUp

    /// <summary>
    /// 
    /// </summary>
    public void SinUp(string user, string nick, string pass,string email)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(RegisterProcess(user,nick, pass, email));
    }

    /// <summary>
    /// Connect with database
    /// </summary>
    /// <returns></returns>
    IEnumerator RegisterProcess(string user, string nick, string pass,string email)
    {
        isRequesting = true;
        SetLogText("");
        //Used for security check for authorization to modify database
        string hash = Md5Sum(user + pass + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        LoadingUI.SetActive(true);
        //Assigns the data we want to save
        //Where -> Form.AddField("name" = matching name of value in SQL database
        WWWForm mForm = new WWWForm();
        mForm.AddField("name", user); // adds the login name to the form
        mForm.AddField("nick", nick); // adds the nick name to the form
        mForm.AddField("password", pass); // adds the player password to the form
        mForm.AddField("kills", 0); // adds the kill total to the form
        mForm.AddField("deaths", 0); // adds the death Total to the form
        mForm.AddField("score", 0); // adds the score Total to the form
        mForm.AddField("coins", bl_GameData.Instance.VirtualCoins.InitialCoins);
        if (bl_LoginProDataBase.Instance.RequiredEmailVerification && !string.IsNullOrEmpty(email))
        {
            mForm.AddField("email", email);
        }
        else
        {
            mForm.AddField("email", "none");
        }
        mForm.AddField("multiemail", bl_LoginProDataBase.Instance.CanRegisterSameEmailInt());
        mForm.AddField("emailVerification", bl_LoginProDataBase.Instance.RequiereVerification());
        mForm.AddField("uIP", currentIP);
        mForm.AddField("hash", hash); // adds the security hash for Authorization

        //Creates instance of WWW to runs the PHP script to save data to mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Register), mForm))
        {
            yield return www.SendWebRequest();

            if (www.error == null)
            {
                string result = www.downloadHandler.text;
                if (bl_LoginProDataBase.Instance.FullLogs)
                {
                    Debug.Log("Register Result: " + result);
                }
                if (result.Contains("success") == true)
                {
                    //show success
                    ChangePanel(4);
                    SetLogText("Register success!");
                }
                else
                {
                    //Debug.Log(www.downloadHandler.text);
                    ErrorType(www.downloadHandler.text);
                }
            }
            else
            {
                Debug.Log("Error:" + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Continue()
    {
        StartCoroutine(LoadSceneSecuence(bl_LoginProDataBase.Instance.OnLoginLoadLevel));
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator LoadSceneSecuence(string scene)
    {
        while(FadeAlpha.alpha < 1)
        {
            FadeAlpha.alpha += Time.deltaTime * 2;
            yield return new WaitForEndOfFrame();
        }
        bl_DataBaseUtils.LoadLevel(scene);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator DoStartSequence()
    {
        FadeAlpha.alpha = 1;
        while(FadeAlpha.alpha > 0)
        {
            FadeAlpha.alpha -= Time.deltaTime;
            yield return null;
        }
        StartCoroutine(InitRequest());
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
#endregion

#region Change Password
    public void ChangePassword(string pass,string newpass)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;
        if (!isLogin) { SetLogText("Need login to request a password change."); return; }

        StartCoroutine(ChangePasswordRequest(pass, newpass));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangePasswordRequest(string pass, string newpass)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");
        //Used for security check for authorization to modify database
        string hash = Md5Sum(LocalUserInfo.ID + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        // Create instance of WWWForm
        WWWForm wf = new WWWForm();
        //sets the mySQL query to the amount of rows to load
        wf.AddField("id", LocalUserInfo.ID);
        wf.AddField("password", pass);
        wf.AddField("newpassword", newpass);
        wf.AddField("hash", hash);
        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.ChangePassword), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (www.error == null && !www.isNetworkError)
            {
                if (result.Contains("success"))
                {
                    SetLogText("your password has been changed successfully.");
                    ChangePanel(3);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug log.
                {
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }
#endregion

#region Reset Password
    public void RequestNewPassword(string user,string email)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(PasswordRequest(user, email));
    }

    public void ResetPassword(string user, string pass)
    {
        if (isRequesting)
            return;
        if (bl_LoginProDataBase.Instance == null)
            return;

        StartCoroutine(ResetPasswordCall(user, pass));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator PasswordRequest(string user, string email)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");
        //Used for security check for authorization to modify database
        string hash = Md5Sum(email + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        lastKey = string.Format("LS-{0}", bl_DataBaseUtils.GenerateKey(8));
        // Create instance of WWWForm
        WWWForm wf = new WWWForm();
        //sets the mySQL query to the amount of rows to load
        wf.AddField("name", user);
        wf.AddField("email", email);
        wf.AddField("key", lastKey);
        wf.AddField("step", 1);
        wf.AddField("hash", hash);
        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.ResetPassword), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (www.error == null && !www.isNetworkError)
            {
                if (result.Contains("success"))
                {
                    SetLogText("An email with your reset key has ben send to your email-address");
                    ChangePanel(7);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug logwarning.
                {
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ResetPasswordCall(string user, string pass)
    {
        isRequesting = true;
        LoadingUI.SetActive(true);
        SetLogText("");
        //Used for security check for authorization to modify database
        string hash = Md5Sum(user + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        // Create instance of WWWForm
        WWWForm wf = new WWWForm();
        //sets the mySQL query to the amount of rows to load
        wf.AddField("name", user);
        wf.AddField("password", pass);
        wf.AddField("step", 2);
        wf.AddField("hash", hash);
        //Creates instance to run the php script to access the mySQL database
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.ResetPassword), wf))
        {
            //Wait for server response...
            yield return www.SendWebRequest();
            string result = www.downloadHandler.text;

            //check if we have some error
            if (www.error == null)
            {
                if (result.Contains("success"))
                {
                    SetLogText("your password has been reset successfully,you can sign in now.");
                    ChangePanel(0);
                }
                else//Wait, have a error?, please contact me for help with the result of next debug logwarning.
                {
                    ErrorType(result);
                }
            }
            else
            {
                Debug.LogError("Error: " + www.error);
            }
            LoadingUI.SetActive(false);
        }
        isRequesting = false;
    }
    #endregion

    #region IP
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator InitRequest()
    {
        bl_ULoginLoadingWindow.Instance?.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        if (!bl_GameData.isDataCached)
        {
            yield return StartCoroutine(bl_GameData.AsyncLoadData());
        }
        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Get(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Init)))
        {
            //Wait for response
            yield return w.SendWebRequest();
            if (!w.isHttpError && !w.isNetworkError)
            {
                string result = w.downloadHandler.text;
                if (bl_LoginProDataBase.Instance.FullLogs)
                {
                    Debug.Log("IP Result: " + result);
                }
                string[] info = result.Split("|"[0]);
                currentIP = info[0];
                if (bl_LoginProDataBase.Instance.CheckGameVersion)
                {
                    if (bl_GameData.Instance.GameVersion != info[1])
                    {
                        OutdatedUI.SetActive(true);
                        bl_ULoginLoadingWindow.Instance?.SetActive(false);
                        Debug.LogWarning(string.Format("Outdated version: {0} - {1}", bl_GameData.Instance.GameVersion, info[1]));
                        yield break;
                    }
                }
                if (bl_LoginProDataBase.Instance.DetectBan)
                {
                    Ban.Process();
                }
                else
                {
                    if (!CheckSessionCredentials())
                    {
                        bl_ULoginLoadingWindow.Instance?.SetActive(false);
                        GuestButton.gameObject.SetActive(true);
                    }
                    else
                    {
                        bl_ULoginLoadingWindow.Instance.SetText("Authenticating previous session user...");
                    }
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
    }
#endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool CheckSessionCredentials()
    {
        if (bl_LoginProDataBase.Instance.rememberMeBehave == RememberMeBehave.RememberSession)
        {
            string credentials = bl_LoginProDataBase.Instance.RememberCredentials;
            if (!string.IsNullOrEmpty(credentials))
            {
                if (credentials.Contains(":"))
                {
                    string[] userCredentials = credentials.Split(':');
                    SignIn.SetCredentials(userCredentials[0], userCredentials[1]);
                    if (!SignIn.SignIn())
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    bl_LoginProDataBase.Instance.DeleteRememberCredentials();
                }
            }
            return false;
        }
        else return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        bl_DataBase.Instance.SignOut();
        bl_LoginProDataBase.Instance.DeleteRememberCredentials();
        SignIn.ResetFields();
        ChangePanel(0);
        OnLogOut?.Invoke();
        AdminPanelButon.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetLogText(string text)
    {
        LogText.text = text;
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangePanel(int id)
    {
        StartCoroutine(ChangePanelAnim(id));
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAdminPanel()
    {
        StartCoroutine(LoadSceneSecuence("AdminPanel"));
    }

    public void LoadClanMenu()
    {
        StartCoroutine(LoadSceneSecuence("ClanMenu"));
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowTerms(bool active)
    {
        TermsAndConditions.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator ChangePanelAnim(int id)
    {
        PanelAnim.Play("change", 0, 0);
        yield return new WaitForSeconds(PanelAnim.GetCurrentAnimatorStateInfo(0).length / 2);
        for(int i = 0; i < Panels.Length; i++) { Panels[i].SetActive(false); }
        Panels[id].SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    private void ErrorType(string error)
    {
        int code = 0;
        if (int.TryParse(error,out code))
        {
            switch (code)
            {
                case (int)LoginErrorCode.PasswordIncorrect:
                    SetLogText("Password is incorrect!");
                    break;
                case (int)LoginErrorCode.UserAlreadyRegister:
                    SetLogText("User with this name already exist");
                    break;
                case (int)LoginErrorCode.UserNotExist:
                    SetLogText("User with this name not exist");
                    break;
                case (int)LoginErrorCode.InvalidEmail:
                    SetLogText("Invalid email address please type a valid email.");
                    break;
                case (int)LoginErrorCode.EmailExist:
                    SetLogText("The email is already taken, please try new.");
                    break;
                case (int)LoginErrorCode.NotActive:
                    SetLogText("Your account is not active yet, verify your email to active.");
                    ChangePanel(2);
                    break;
                case (int)LoginErrorCode.UserNotFound:
                    SetLogText("User not found, have you sign out?");
                    break;
                case (int)LoginErrorCode.EmailNotSend:
                    SetLogText("Email can't be send.");
                    break;
                case (int)LoginErrorCode.UserAndEmailNotFound:
                    SetLogText("User or email is not found, or they are not of the same account.");
                    break;
                default:
                    SetLogText("Error code not define: " + error);
                    break;
            }
        }
        else
        {
            SetLogText("Unknown error: " + error);
        }
    }

    public string GetKey { get { return lastKey; } }
    public string CurrentIp { get { return currentIP; } }

    private static bl_LoginPro _instance;
    public static bl_LoginPro Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_LoginPro>(); }
            return _instance;
        }
    }
}