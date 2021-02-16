using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Networking;
using MFPS.ULogin;

public class bl_DataBase : bl_LoginProBase
{
    [Header("Local User Info")]
    public LoginUserInfo LocalUser;

    #region Public properties
    public bool isLogged { get; set; }
    public string CacheAccessToken { get; set; }
    public bool isGuest { get; set; }
    public string RSAPublicKey { get; set; }
    public static LoginUserInfo LocalLoggedUser => Instance.LocalUser;
    #endregion

    #region Private members
    private int TasksRunning = 0;
    private bl_LoginProDataBase Data;
    private float StartPlayTime = 0;
    #endregion

    public delegate void OnUpdateDataEvent(LoginUserInfo userInfo);
    public static OnUpdateDataEvent OnUpdateData;

    public static LoginUserInfo LocalUserInstance => Instance.LocalUser;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        gameObject.name = "Database";
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
            InvokeRepeating(nameof(BanComprobation), t, t);
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
        var wf = bl_DataBaseUtils.CreateWWWForm(FormHashParm.Name, true);
        wf.AddSecureField("typ", 8);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("key", key);
        wf.AddSecureField("data", (string)value);

        WebRequest.POST(DataBaseURL, wf, (result) =>
         {
             if (!result.isError)
             {
                 if (result.resultState == ULoginResult.Status.Success)
                 {
                     Debug.Log($"{key} value saved!");
                 }
                 else
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
    /// Update the given fields of the logged user in the external database
    /// </summary>
    public void UpdateUserData(ULoginUpdateFields fieldsToUpdate, Action<bool> callback = null)
    {
        if (LocalUser == null || !isLogged || isGuest)
        {
            Debug.LogWarning("To save data user have to be log-in.");
            return;
        }
        if (fieldsToUpdate == null) return;

        var wf = CreateForm(false, true);
        wf.AddSecureField("hash", bl_DataBaseUtils.CreateSecretHash(LocalUser.LoginName));
        wf.AddSecureField("name", LocalUser.LoginName);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("typ", 0);
        wf = fieldsToUpdate.AddToWebForm(wf);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                result.PrintError();
                callback?.Invoke(false);
                return;
            }
            if (ULoginSettings.FullLogs) result.Print();

            if (result.resultState == ULoginResult.Status.Success)
            {
                Debug.LogFormat("Data update successfully for user: {0}", LocalUser.NickName);
                FireUpdateData();
            }
            else if (result.resultState == ULoginResult.Status.Fail)
            {
                Debug.Log("Update user data fail, that could be caused to the data sent were the same that the one already saved.");
            }
            callback?.Invoke(true);
        });
    }

    /// <summary>
    /// Add NEW coins to the user wallet
    /// </summary>
    /// <param name="newCoins"></param>
    public void SaveNewCoins(int newCoins)
    {
        UpdateUserCoins(newCoins, ULoginCoinsOp.Add);
    }

    /// <summary>
    /// Subtract coins from the user wallet
    /// </summary>
    /// <param name="coins"></param>
    public bool SubtractCoins(int coins)
    {
        UpdateUserCoins(coins, ULoginCoinsOp.Deduct);
        return true;
    }

    /// <summary>
    /// Update user coins.
    /// </summary>
    /// <param name="coins"></param>
    /// <param name="coinsOp">add or deduct coins?</param>
    public void UpdateUserCoins(int coins, ULoginCoinsOp coinsOp, Action callback = null)
    {
        if (!IsUserLogged) return;

        if(coinsOp == ULoginCoinsOp.Deduct && (LocalUser.Coins - coins) <= 0)
        {
            Debug.LogWarning("User wallet shouldn't be negatived.");
        }

        var wf = CreateForm(FormHashParm.Name, true);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("typ", 6);
        wf.AddSecureField("values", coins);
        wf.AddSecureField("key", (int)coinsOp);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                Debug.Log(result.RawText);
                result.PrintError();
                return;
            }
            if (ULoginSettings.FullLogs) result.Print();

            if (result.resultState == ULoginResult.Status.Success)
            {
                int newCoins = 0;
                if (int.TryParse(result.Text, out newCoins))
                {
                    LocalUser.Coins = newCoins;
                    Debug.LogFormat("User coins updated, the new total is: {0}", newCoins);
                    FireUpdateData();
                }
                else
                {
                    Debug.LogWarning("Unknown response: " + result.RawText);
                }
            }
            else result.Print(true);

            callback?.Invoke();
        });
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

    /// <summary>
    /// Store virtual coins that has been acquired by purchase
    /// if the coins was NOT acquired by a real purchase use SaveNewCoins() function instead
    /// </summary>
    public void SetCoinPurchase(CoinPurchaseData data, Action<bool> callback)
    {
        var wf = CreateForm(FormHashParm.Name, true);
        wf.AddSecureField("typ", 7);
        wf.AddSecureField("id", LocalUser.ID);
        var jsonData = JsonUtility.ToJson(data);
        wf.AddField("data", jsonData);
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

    /// <summary>
    /// Check every certain time if the player has not been banned
    /// </summary>
    void BanComprobation()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("typ", 3);
        wf.AddField("name", LocalUser.LoginName);
        wf.AddField("ip", LocalUser.IP);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.BanList), wf, (result) =>
          {
              if(result.isError)
              {
                  result.PrintError();
                  CancelInvoke();
                  return;
              }

              if(result.HTTPCode == 302)
              {
                  Debug.Log("You're banned");
                  bl_DataBaseUtils.LoadLevel(0);
              }
          });
    }

    /// <summary>
    /// Verify if user exist
    /// </summary>
    public static void CheckIfUserExist(MonoBehaviour reference, string where, string index, Action<bool> callback)
    {
        bl_ULoginWebRequest webRequest = new bl_ULoginWebRequest(reference);
        var wf = bl_DataBaseUtils.CreateWWWForm(FormHashParm.Name, true);
        wf.AddSecureField("typ", 4);
        wf.AddSecureField("key", where);
        wf.AddSecureField("values", index);

        webRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                result.PrintError();
                return;
            }

            callback?.Invoke(result.resultState == ULoginResult.Status.Success);
        });
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
        if (!IsUserLogged)
            return;
        if (StartPlayTime <= 1)
            return;

        float total = Time.time - StartPlayTime;
        int minutes = Mathf.CeilToInt(total);
        if (minutes <= 1)
            return;

        var wf = CreateForm(FormHashParm.Name, true);
        wf.AddSecureField("id", LocalUser.ID);
        wf.AddSecureField("values", minutes);
        wf.AddSecureField("typ", 3);

        WebRequest.POST(GetURL(bl_LoginProDataBase.URLType.DataBase), wf, (result) =>
        {
            if (result.isError)
            {
                result.PrintError();
                return;
            }
            if (ULoginSettings.FullLogs) result.Print();

            if (result.resultState == ULoginResult.Status.Success)
            {
                LocalUser.PlayTime += minutes;
                TimeSpan t = TimeSpan.FromMinutes((double)LocalUser.PlayTime);
                string answer = string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);
                Debug.Log("Save Time: " + minutes + " Total Play Time: " + answer);
                FireUpdateData();
            }
        });
    }

    /// <summary>
    /// 
    /// </summary>
    void FireUpdateData() => OnUpdateData?.Invoke(LocalUser);

    /// <summary>
    /// Set local data to data base
    /// </summary>
    [Obsolete("Use 'bl_ULoginMFPS.SaveLocalPlayerKDS' instead")]
    public Coroutine SaveData(int score, int kills, int deaths)
    {
        bl_ULoginMFPS.SaveLocalPlayerKDS();
        return null;
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

    public static bool IsUserLogged => Instance != null && Instance.LocalUser != null && Instance.isLogged && !Instance.isGuest;

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