using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class bl_SupportTicketManager : MonoBehaviour
{

    [SerializeField] private GameObject TicketPrefab;
    [SerializeField] private Transform TicketsPanel;
    [SerializeField] private Text MessageText;
    [SerializeField] private Text TitleText;
    [SerializeField] private Text NameText;
    [SerializeField] private InputField ReplyInput;

    private bl_SupportTicket CurrentTicket;

    private void Start()
    {
        LoadTickets();
    }

    public void LoadTickets()
    {
        StartCoroutine(GetTickets());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator GetTickets()
    {
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum("dev" + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("hash", hash);
        wf.AddField("name", "dev");
        wf.AddField("type", 3);

        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
        {
            //Wait for response
            yield return w.SendWebRequest();
            if (!w.isHttpError && !w.isNetworkError)
            {
                string[] tickets = w.downloadHandler.text.Split("\n"[0]);
                List<bl_SupportTicket.Ticket> List = new List<bl_SupportTicket.Ticket>();
                for (int i = 0; i < tickets.Length; i++)
                {
                    if (!string.IsNullOrEmpty(tickets[i]) && tickets[i].Length > 2)
                    {
                        string[] info = tickets[i].Split("|"[0]);
                        bl_SupportTicket.Ticket t = new bl_SupportTicket.Ticket();
                        t.Title = info[0];
                        t.Message = info[1];
                        t.Reply = info[2];
                        t.ID = int.Parse(info[3]);
                        t.User = info[4];
                        List.Add(t);
                    }
                }
                InstanceTickets(List);
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
    }

    public void SelectTicket(bl_SupportTicket ticket)
    {
        CurrentTicket = ticket;
        MessageText.text = ticket.cacheInfo.Message;
        TitleText.text = ticket.cacheInfo.Title;
        NameText.text = ticket.cacheInfo.User;
        ReplyInput.text = (string.IsNullOrEmpty(ticket.cacheInfo.Reply)) ? "Reply..." : ticket.cacheInfo.Reply;
    }

    public void Reply()
    {
        if (CurrentTicket == null || ReplyInput.text == string.Empty)
            return;

        StartCoroutine(ReplyTicket());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ReplyTicket()
    {
        WWWForm wf = new WWWForm();
        string hash = bl_DataBaseUtils.Md5Sum("dev" + bl_LoginProDataBase.Instance.SecretKey).ToLower();
        wf.AddField("hash", hash);
        wf.AddField("name", "dev");
        wf.AddField("id", CurrentTicket.cacheInfo.ID);
        wf.AddField("reply", ReplyInput.text);
        wf.AddField("type", 4);

        //Request public IP to the server
        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Support), wf))
        {
            //Wait for response
            yield return w.SendWebRequest();
            if (!w.isHttpError && !w.isNetworkError)
            {
                if (w.downloadHandler.text.Contains("success"))
                {
                    Destroy(CurrentTicket.gameObject);
                    CurrentTicket = null;
                    ReplyInput.text = string.Empty;
                    MessageText.text = string.Empty;
                    TitleText.text = string.Empty;
                    NameText.text = string.Empty;
                }
                else { Debug.LogWarning(w.downloadHandler.text); }
            }
            else
            {
                Debug.LogError(w.error);
            }
        }
    }

    void InstanceTickets(List<bl_SupportTicket.Ticket> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            GameObject g = Instantiate(TicketPrefab) as GameObject;
            g.transform.SetParent(TicketsPanel, false);
            g.GetComponent<bl_SupportTicket>().GetInfo(list[i], this);
        }
    }
}