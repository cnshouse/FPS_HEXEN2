using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using MFPS.ULogin;

public class bl_RankingPro : MonoBehaviour
{
    [Header("Settings")]
    public bool FetchOnStart = true;
    [SerializeField]private int Top = 100;
    [Header("References")]
    public GameObject Content;
    [SerializeField]private GameObject RankingUIPrefab = null;
    [SerializeField]private Transform RankingPanel = null;

    private bl_LoginProDataBase LoginDataBase;
    private List<GameObject> currentList = new List<GameObject>();
    private bool Requesting = false;

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        LoginDataBase = bl_LoginProDataBase.Instance;
        if(FetchOnStart)
        StartCoroutine(GetRanking());
    }

    public void Open()
    {
        if (Content != null) Content.SetActive(true);
        StartCoroutine(GetRanking());
    }

    public void Refresh()
    {
        if (Requesting)
            return;

        Clean();
        StartCoroutine(GetRanking());
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator GetRanking()
    {
        Requesting = true;
        Dictionary<string,string> wf = new Dictionary<string, string>();
        wf.Add("top", Top.ToString());

        UnityWebRequest w = UnityWebRequest.Post(LoginDataBase.GetUrl(bl_LoginProDataBase.URLType.Ranking), wf);
        yield return w.SendWebRequest();

        if(w.isNetworkError == false)
        {
            string result = w.downloadHandler.text;
            if (!result.Contains("009"))
            {
                string[] split = result.Split("\n"[0]);
                List<LoginUserInfo> alluser = new List<LoginUserInfo>();
                for(int i = 0; i < split.Length; i++)
                {
                    if (!string.IsNullOrEmpty(split[i]))
                    {
                        string[] info = split[i].Split("|"[0]);
                        LoginUserInfo user = new LoginUserInfo();
                        user.LoginName = info[0];
                        user.NickName = info[1];
                        user.Kills = info[2].ToInt();
                        user.Deaths = info[3].ToInt();
                        user.Score = info[4].ToInt();
                        user.UserStatus = (LoginUserInfo.Status)info[5].ToInt();
                        alluser.Add(user);
                    }
                }
                InstanceUI(alluser);
            }
            else
            {
                Debug.Log("Any user register yet.");
            }
        }
        else
        {
            Debug.LogError(w.error);
        }
        w.Dispose();
        Requesting = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void InstanceUI(List<LoginUserInfo> users)
    {
        Clean();
        GameObject g = null;
        for(int i = 0; i < users.Count; i++)
        {
            g = Instantiate(RankingUIPrefab) as GameObject;
            g.GetComponent<bl_RankingUIPro>().SetInfo(users[i], i + 1);
            g.transform.SetParent(RankingPanel, false);
            currentList.Add(g);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void Clean()
    {
        for (int i = 0; i < currentList.Count; i++)
        {
            Destroy(currentList[i]);
        }
        currentList.Clear();
    }
}