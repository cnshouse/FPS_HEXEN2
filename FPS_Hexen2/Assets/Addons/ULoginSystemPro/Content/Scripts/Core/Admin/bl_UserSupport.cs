using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class bl_UserSupport : MonoBehaviour
{
    public float WindowSize = 141;
    [Range(1,100)] public float ShowSpeed = 100;

    [Header("References")]
    [SerializeField] private GameObject LoginBlock;
    [SerializeField] private GameObject ReplyWindow;
    [SerializeField] private InputField TitleInput;
    [SerializeField] private InputField ContentInput;
    [SerializeField] private Button SummitButton;
    [SerializeField] private Button CloseButton;
    [SerializeField] private Text MessageText;
    [SerializeField] private Text ReplyText;
    [SerializeField] private GameObject Loading;
    [SerializeField] private RectTransform WindowTransform;

    private bl_LoginPro LoginPro;
    private bl_DataBase DataBase;
    private bool sending = false;
    private int PendingReplyID = 0;
    private bool ShowWindow = false;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        LoginPro = FindObjectOfType<bl_LoginPro>();
        DataBase = bl_DataBase.Instance;
        LoginBlock.SetActive(true);
        ReplyWindow.SetActive(false);
        Loading.SetActive(false);
        SummitButton.interactable = false;
        CloseButton.interactable = false;
    }

    public void OnLogin()
    {
        DataBase = bl_DataBase.Instance;
        LoginBlock.SetActive(false);
        StartCoroutine(CheckTickets());
    }

    public void OnLogOut()
    {
        LoginBlock.SetActive(true);
    }

    public void Send()
    {
        if (sending || DataBase == null || !DataBase.isLogged)
            return;
        if (string.IsNullOrEmpty(TitleInput.text) || string.IsNullOrEmpty(ContentInput.text))
            return;

        StartCoroutine(SummitTicket());
    }

    public void CheckTexts()
    {
        SummitButton.interactable = (TitleInput.text.Length > 2 && ContentInput.text.Length > 13);
    }

    public void Show()
    {
        ShowWindow = !ShowWindow;
        StopCoroutine("ShowWindowIE");
        StartCoroutine("ShowWindowIE", ShowWindow);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator SummitTicket()
    {
        Loading.SetActive(true);
        SummitButton.interactable = false;
        sending = true;
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.NickName  + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("hash", hash);
        wf.AddField("name", DataBase.LocalUser.NickName);
        wf.AddField("title", TitleInput.text);
        wf.AddField("content", ContentInput.text);
        wf.AddField("type", 1);

        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
        {
            //Wait for response
            yield return w.SendWebRequest();

            if (w.error == null && !w.isNetworkError)
            {
                if (w.downloadHandler.text.Contains("success"))
                {
                    LoginPro.SetLogText("Summited!");
                    MessageText.text = ContentInput.text;
                    ReplyText.text = "AWAITING FOR REPLY...";
                    CloseButton.interactable = false;
                    ReplyWindow.SetActive(true);
                }
                else { Debug.LogWarning(w.downloadHandler.text); }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        sending = false;
        Loading.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator CheckTickets()
    {
        Loading.SetActive(true);
        SummitButton.interactable = false;
        sending = true;
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.NickName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("hash", hash);
        wf.AddField("name", DataBase.LocalUser.NickName);
        wf.AddField("type", 2);

        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
        {
            //Wait for response
            yield return w.SendWebRequest();
            if (w.error == null)
            {
                string[] split = w.downloadHandler.text.Split("|"[0]);
                if (split[0].Contains("reply"))
                {
                    MessageText.text = split[1];
                    string reply = split[2];
                    if (string.IsNullOrEmpty(reply))
                    {
                        reply = "AWAITING FOR REPLY...";
                    }
                    else
                    {
                        CloseButton.interactable = true;
                    }
                    ReplyText.text = reply;
                    PendingReplyID = int.Parse(split[3]);
                    ReplyWindow.SetActive(true);
                }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        sending = false;
        Loading.SetActive(false);
    }

    public void CloseTicket()
    {
        CloseButton.interactable = false;
        StartCoroutine(CloseTicketIE());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator CloseTicketIE()
    {
        Loading.SetActive(true);
        SummitButton.interactable = false;
        sending = true;
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum(DataBase.LocalUser.NickName + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("hash", hash);
        wf.AddField("name", DataBase.LocalUser.NickName);
        wf.AddField("id", PendingReplyID);
        wf.AddField("type", 5);

        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
        {
            //Wait for response
            yield return w.SendWebRequest();
            if (w.error == null && !w.isNetworkError)
            {
                if (w.downloadHandler.text.Contains("success"))
                {
                    LoginPro.SetLogText("Close ticket!");
                    ReplyWindow.SetActive(false);
                }
                else { Debug.LogWarning(w.downloadHandler.text); }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
        sending = false;
        Loading.SetActive(false);
    }

    IEnumerator ShowWindowIE(bool show)
    {
        Vector2 v = WindowTransform.anchoredPosition;
        if (show)
        {
            while(v.x > 0)
            {
                v.x -= Time.deltaTime * (ShowSpeed * 10);
                WindowTransform.anchoredPosition = v;
                yield return null;
            }
            v.x = 0;
            WindowTransform.anchoredPosition = v;
        }
        else
        {
            while (v.x < WindowSize)
            {
                v.x += Time.deltaTime * (ShowSpeed * 10);
                WindowTransform.anchoredPosition = v;
                yield return null;
            }
            v.x = WindowSize;
            WindowTransform.anchoredPosition = v;
        }
    }
}