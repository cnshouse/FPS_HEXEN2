using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using MFPS.ULogin;

public class bl_DataBase : MonoBehaviour
{
    [Header("Local User Info")]
    public LoginUserInfo LocalUser;

    private float StartPlayTime = 0;
    public bool isLogged { get; set; }
    private bool isSavingData = false;
    public string CacheAccessToken { get; set; }
    private bl_LoginProDataBase Data;
    public bool isGuest { get; set; }

    public delegate void OnUpdateDataEvent(LoginUserInfo userInfo);
    public static OnUpdateDataEvent OnUpdateData;

    private int TasksRunning = 0;
    private bl_ULoginWebRequest _webRequest;
    public bl_ULoginWebRequest WebRequest { get { if (_webRequest == null) { _webRequest = new bl_ULoginWebRequest(this); } return _webRequest; } }

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Data = bl_LoginProDataBase.Instance;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLogin(LoginUserInfo info)
    {
        LocalUser = info;
        isLogged = true;
        isGuest = false;
        if (Data.BanComprobationInMid)
        {
            float t = Data.CheckBanEach;
            InvokeRepeating("BanComprobation", t, t);
        }
        if(info.UserStatus == LoginUserInfo.Status.Admin || info.UserStatus == LoginUserInfo.Status.Moderator)
        {
            if (!bl_GameData.Instance.GameTeam.Exists(x => x.UserName == info.NickName))
            {
                bl_GameData.GameTeamInfo ti = new bl_GameData.GameTeamInfo();
                ti.UserName = info.NickName;
                if (info.UserStatus == LoginUserInfo.Status.Admin)
                {
                    ti.m_Role = bl_GameData.GameTeamInfo.Role.Admin;
                    ti.m_Color = Color.red;
                }
                else
                {
                    ti.m_Role = bl_GameData.GameTeamInfo.Role.Moderator;
                    ti.m_Color = Color.blue;
                }
                bl_GameData.Instance.GameTeam.Add(ti);
                bl_GameData.Instance.CurrentTeamUser = bl_GameData.Instance.GameTeam[bl_GameData.Instance.GameTeam.Count - 1];
            }
        }
    }

    /// <summary>
    /// Save a key pair value
    /// </summary>
    /// <param name="key">database colum name</param>
    /// <param name="value">value to save (string or int)</param>
    public void SaveValue(string key, object value, Action callBack = null)
    {
        var wf = bl_DataBaseUtils.CreateWWWForm();
        wf.AddField("typ", 8);
        wf.AddField("name", LocalUser.ID);
        wf.AddField("key", key);
        wf.AddField("data", (string)value);

        WebRequest.POST(DataBaseURL, wf, (result) =>
         {
             if (!result.isError)
             {
                 if (result.Text.Contains("save"))
                 {
                     Debug.Log($"{key} value saved!");
                 }else
                 result.Print();
             }
             else
             {
                 result.PrintError();
             }
             callBack?.Invoke();
         });
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveUserMetaData(Action callBack = null)
    {
        SaveValue("meta", LocalUser.metaData.ToString(), () => { callBack?.Invoke(); });
    }

    /// <summary>
    /// Start record the play time from this exactly moment
    /// </summary>
    public void RecordTime()
    {
        StartPlayTime = Time.time;
        //Debug.Log("Start track play time on : " + StartPlayTime);
    }

    /// <summary>
    /// stop record time and save and sum this play time to the store 
    /// in database
    /// </summary>
    public void StopAndSaveTime()
    {
        if (LocalUser == null || isGuest)
            return;
        if (StartPlayTime <= 0)
            return;

        float total = Time.time - StartPlayTime;
        int seconds = Mathf.FloorToInt(total);
        StartCoroutine(SetPlayTime(seconds));
    }

    /// <summary>
    /// Set local data to data base
    /// </summary>
    public void SaveData()
    {
        if (!isLogged || LocalUser == null || isGuest)
            return;

        StartCoroutine(IESaveData());
    }

    /// <summary>
    /// Set local data to data base
    /// </summary>
    public Coroutine SaveData(int score, int kills, int deaths)
    {
        if (!isLogged || LocalUser == null || isGuest)
            return null;

        LocalUser.SetNewData(score, kills, deaths);
        return StartCoroutine(IESaveData());
    }

    /// <summary>
    /// Add NEW coins to the user wallet
    /// </summary>
    /// <param name="newCoins"></param>
    public void SaveNewCoins(int newCoins)
    {
        if (newCoins <= 0)
            return;
        if (!isLogged || LocalUser == null || isGuest)
            return;

        int allCoins = LocalUser.Coins + newCoins;
        StartCoroutine(SetGameCoins(allCoins));
    }

    /// <summary>
    /// Subtract coins from the user wallet
    /// </summary>
    /// <param name="coins"></param>
    public bool SubtractCoins(int coins)
    {
        if (!isLogged || LocalUser == null || isGuest)
            return false;

        int userCoins = LocalUser.Coins;
        int result = userCoins - coins;
        if(result > 0)
        {
            StartCoroutine(SetGameCoins(result));
        }
        else
        {
            Debug.LogWarning("User wallet can't be negative, coins will not be saved!.");
        }
        return result > 0;
    }

    /// <summary>
    /// Save the user coins
    /// </summary>
    /// <param name="Coins">the user coins to save</param>
    public void SavenAllCoins(int Coins)
    {
        if (!isLogged || LocalUser == null || isGuest)
            return;

        StartCoroutine(SetGameCoins(Coins));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator IESaveData()
    {
        TasksRunning++;
        if (isSavingData) { yield break; }
        isSavingData = true;
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(LocalUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf = LocalUser.AddData(wf);
        wf.AddField("name", LocalUser.LoginName);
        wf.AddField("typ", 1);
        wf.AddField("hash", hash);

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf))
        {

            yield return www.SendWebRequest();

            if (www.error == null && !www.isNetworkError)
            {
                if (www.downloadHandler.text.Contains("success"))
                {
                    Debug.Log("<color=green>User data save!</color>");
#if LM
             bl_LevelManager.Instance.Check(LocalUser.Score);
#endif
                    FireUpdateData();
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
        TasksRunning--;
        isSavingData = false;
    }

#if CLANS
    public void SetClanScore(int newScore)
    {
        if (LocalUser.Clan == null || !LocalUser.Clan.haveClan) return;

        StartCoroutine(SetScoreToClan(newScore));
    }

    IEnumerator SetScoreToClan(int newScore)
    {
        WWWForm wf = new WWWForm();
        wf.AddField("type", 99);
        wf.AddField("hash", GetUserToken());
        wf.AddField("clanID", LocalUser.Clan.ID);
        wf.AddField("settings", newScore);

        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
        {
            yield return w.SendWebRequest();

            if (!w.isNetworkError)
            {
                string t = w.downloadHandler.text;
                if (t.Contains("done"))
                {
                    Debug.Log("Clan Score update");
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
#endif

    IEnumerator SetGameCoins(int coins)
    {
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(LocalUser.ID + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("coins", coins);
        wf.AddField("name", LocalUser.ID);
        wf.AddField("typ", 6);
        wf.AddField("hash", hash);

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf))
        {
            yield return www.SendWebRequest();

            if (!www.isNetworkError)
            {
                if (www.downloadHandler.text.Contains("save"))
                {
                    LocalUser.Coins = coins;
                    Debug.Log("Player coins store successfully!");
                    FireUpdateData();
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

    /// <summary>
    /// Store virtual coins that has been acquired by purchase
    /// if the coins was NOT acquired real purchase use SaveNewCoins() function instead
    /// </summary>
    public void SetCoinPurchase(CoinPurchaseData data, Action<bool> callback)
    {
        var wf = bl_DataBaseUtils.CreateWWWForm();
        wf.AddField("typ", 7);
        wf.AddField("name", LocalUser.ID);
        wf.AddField("data", JsonUtility.ToJson(data));

        WebRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            result.Print();
            if (result.resultState == ULoginResult.Status.Success)
            {
                LocalUser.Coins = int.Parse(result.Text.Trim());
            }
            if (callback != null) { callback.Invoke(result.resultState == ULoginResult.Status.Success); }
        });
    }

    IEnumerator SetPlayTime(int plusSeconds)
    {
        TasksRunning++;
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(LocalUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("playTime", plusSeconds);
        wf.AddField("name", LocalUser.LoginName);
        wf.AddField("typ", 3);
        wf.AddField("hash", hash);

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf))
        {
            yield return www.SendWebRequest();

            if (www.error == null && !www.isNetworkError)
            {
                if (www.downloadHandler.text.Contains("successpt"))
                {
                   // Debug.Log(www.downloadHandler.text);
                    LocalUser.PlayTime += plusSeconds;
                    TimeSpan t = TimeSpan.FromSeconds((double)LocalUser.PlayTime);
                    string answer = string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);
                    Debug.Log("Save Time: " + plusSeconds + " Total Play Time: " + answer);
                    FireUpdateData();
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
        TasksRunning--;
    }

    /// <summary>
    /// Check every certain time if the player has not been banned
    /// </summary>
    void BanComprobation()
    {
        StartCoroutine(CheckBan());
    }

    /// <summary>
    /// 
    /// </summary>
    void FireUpdateData()
    {
        if (OnUpdateData != null)
            OnUpdateData.Invoke(LocalUser);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckBan()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("typ", 1);
        wf.AddField("name", LocalUser.LoginName);

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.BanList), wf))
        {
            yield return www.SendWebRequest();

            if (www.error == null && !www.isNetworkError)
            {
                if (www.downloadHandler.text.Contains("yes"))
                {
                    Debug.Log("You're banned");
                    bl_DataBaseUtils.LoadLevel(0);
                }
            }
            else
            {
                Debug.LogError(www.error);
                CancelInvoke("BanComprobation");
            }
        }
    }

    public void GetUserInfo(string userName)
    {
        StartCoroutine(GetUserNameRequest(userName));
    }

    IEnumerator GetUserNameRequest(string userName)
    {
        WWWForm wf = new WWWForm();
        wf.AddField("typ", 2);
        wf.AddField("name", LocalUser.LoginName);

        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.RequestUser), wf))
        {
            yield return www.SendWebRequest();

            if (www.error == null)
            {
                if (www.downloadHandler.text.Contains("yes"))
                {
                    Debug.Log("You banned");
                    bl_DataBaseUtils.LoadLevel(0);
                }
            }
            else
            {
                Debug.LogError(www.error);
                CancelInvoke("BanComprobation");
            }
        }
    }

    public void BanLocalUser(string reason)
    {
        StartCoroutine(BanUser(reason));
    }

    IEnumerator BanUser(string reason)
    {
        //Used for security check for authorization to modify database
        string hash = bl_DataBaseUtils.Md5Sum(LocalUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        WWWForm wf = new WWWForm();
        wf.AddField("name", LocalUser.LoginName);
        wf.AddField("reason", reason);
        wf.AddField("myIP", LocalUser.IP);
        wf.AddField("mBy", "Admin");
        wf.AddField("type", 1);
        wf.AddField("hash", hash);

        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Ban), wf))
        {
            yield return w.SendWebRequest();

            if (w.error == null && !w.isNetworkError)
            {
                string result = w.downloadHandler.text;
                if (result.Contains("success"))
                {
                    Debug.Log("Banned");
                    FireUpdateData();
                }
                else
                {
                    Debug.Log(result);
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        bl_UtilityHelper.LoadLevel(0);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SignOut()
    {
        LocalUser = new LoginUserInfo();
        isLogged = false;
        isGuest = false;
        CacheAccessToken = string.Empty;
    }

    public string GetUserToken()
    {
        return bl_DataBaseUtils.Md5Sum(bl_LoginProDataBase.Instance.SecretKey).ToLower();
    }

    public string GetUserTokenComplete()
    {
        return bl_DataBaseUtils.Md5Sum(LocalUser.LoginName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
    }

    public bool IsRunningTask { get { return TasksRunning > 0; } }
    public string DataBaseURL => bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase);

    private static bl_DataBase _Instance;
    public static bl_DataBase Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<bl_DataBase>();
            }
            return _Instance;
        }
    }
}