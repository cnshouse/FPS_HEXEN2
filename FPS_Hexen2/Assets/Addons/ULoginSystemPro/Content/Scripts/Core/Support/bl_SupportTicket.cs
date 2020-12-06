using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_SupportTicket : MonoBehaviour
{
    [SerializeField] private Text TitleText;
    public Ticket cacheInfo { get; set; }
    private bl_SupportTicketManager Manager;

    public void GetInfo(Ticket info, bl_SupportTicketManager ma)
    {
        Manager = ma;
        cacheInfo = info;
        TitleText.text = info.Title;
    }

    public void Select()
    {
        Manager.SelectTicket(this);
    }

    [System.Serializable]
   public class Ticket
    {
        public string Title;
        public string Message;
        public string Reply;
        public bool isClose;
        public string User;
        public int ID;
    }
}