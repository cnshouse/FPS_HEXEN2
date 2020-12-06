using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class bl_BanSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField]private GameObject BannedUI;
    [SerializeField]private Text BannedText;

    [SerializeField] private List<BanUserInfo> BanList = new List<BanUserInfo>();
    private bl_LoginPro Login;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        Login = FindObjectOfType<bl_LoginPro>();
        if (BannedUI) { BannedUI.transform.SetAsLastSibling(); }
    }

    public void Process()
    {
        StartCoroutine(GetBanList());
    }

    /// <summary>
    /// Get Ban List for sure that this IP is not ban
    /// </summary>
    /// <returns></returns>
    IEnumerator GetBanList()
    {
        // yield return new WaitForSeconds(1);
        bl_ULoginLoadingWindow.Instance.SetText("Checking client status with server...");
        Dictionary<string,string> wf = new Dictionary<string, string>();
        wf.Add("typ", "0");
        UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.BanList), wf);
     
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            print("Error: " + www.error);
        }
        else
        {
            string text = www.downloadHandler.text;
            if (text.Contains("result") == true)
            {
                string[] splitusers = text.Split("\n"[0]);
                foreach (string t in splitusers)
                {
                    if (!string.IsNullOrEmpty(t) && !t.Contains("result"))
                    {
                        string[] splitinfo = t.Split("|"[0]);
                        BanUserInfo bui = new BanUserInfo();
                        bui.User = splitinfo[0];
                        bui.Reason = splitinfo[1];
                        bui.IP = splitinfo[2];
                        bui.BannedBy = splitinfo[3];
                        BanList.Add(bui);
                    }
                }
                CheckBan();
            }
            else
            {
                if (text.Contains("empty") == true)
                {
                    Debug.Log("Don't have banned users.");
                    if (Login.GuestButton != null) { Login.GuestButton.interactable = true; Login.GuestButton.gameObject.SetActive(true); }
                }
                else
                {
                    Debug.Log(text);
                }
            }
        }
        if (!bl_GameData.isDataCached)
        {
          yield return StartCoroutine(bl_GameData.AsyncLoadData());
        }
        if (!Login.CheckSessionCredentials())
        {
            bl_ULoginLoadingWindow.Instance?.SetActive(false);
        }
        else
        {
            bl_ULoginLoadingWindow.Instance.SetText("Authenticating previous session user...");
        }
        www.Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    void CheckBan()
    {
        bool has = false;
        for (int i = 0; i < BanList.Count; i++)
        {
            if (BanList[i].IP == Login.CurrentIp)
            {
                OnBan(i);
                has = true;
            }
        }

        if (Login.GuestButton != null && !has) { Login.GuestButton.interactable = true; Login.GuestButton.gameObject.SetActive(true); }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool CheckBanAccount(string username)
    {
        for (int i = 0; i < BanList.Count; i++)
        {
            if (BanList[i].User == username)
            {
                Debug.Log("Ban detected!");
                OnBan(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnBan(int id)
    {
        BannedText.text = string.Format("You has been permanently <b>banned</b> from the game due {0} by {1}.", BanList[id].Reason, BanList[id].BannedBy);
        BannedUI.SetActive(true);
    }

    [System.Serializable]
    public class BanUserInfo
    {
        public string User = "";
        public string Reason = "";
        public string IP = "";
        public string BannedBy = "None";
    }
}