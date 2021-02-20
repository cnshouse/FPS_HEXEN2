using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using HashTable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class bl_WaitingRoomChat : bl_PhotonHelper
{
    public Text ChatText;
    public InputField chatInput;

    static readonly RaiseEventOptions EventsAll = new RaiseEventOptions();
    private string team1Color, team2Color;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        ChatText.text = string.Empty;
        EventsAll.Receivers = ReceiverGroup.All;
        PhotonNetwork.NetworkingClient.EventReceived += OnEventCustom;
        if (bl_GameData.isDataCached)
        {
            team1Color = ColorUtility.ToHtmlStringRGBA(bl_GameData.Instance.Team1Color);
            team2Color = ColorUtility.ToHtmlStringRGBA(bl_GameData.Instance.Team2Color);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEventCustom;
    }

    /// <summary>
    /// From Server
    /// </summary>
    public void OnChatReceive(HashTable data)
    {
        string msg = (string)data["chat"];
        Player sender = (Player)data["player"];
        string txt = "";
        if (isOneTeamModeUpdate)
        {
            txt = string.Format("{0}: {1}", sender.NickNameAndRole(), msg);
        }
        else
        {
            string tc = sender.GetPlayerTeam() == Team.Team1 ? team1Color : team2Color;
            txt = string.Format("<color=#{2}>{0}</color>: {1}", sender.NickNameAndRole(), msg, tc);
        }
        AddChat(txt);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public void SendMessage(InputField field)
    {
        if (!PhotonNetwork.InRoom) return;

        string str = field.text;
        if (string.IsNullOrEmpty(str)) return;

        HashTable table = new HashTable();
        table.Add("chat", str);
        table.Add("player", PhotonNetwork.LocalPlayer);
        PhotonNetwork.RaiseEvent(PropertiesKeys.ChatEvent, table, EventsAll, SendOptions.SendUnreliable);
        field.text = string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddChat(string txt)
    {
        ChatText.text += "\n" + txt;
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update()
    {
        if (chatInput == null) return;
        if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
        {
            if (chatInput.isFocused || !string.IsNullOrEmpty(chatInput.text))
            {
                SendMessage(chatInput);
            }
        }
    }

    /// <summary>
    /// RaiseEvent = RPC, I just used this cuz I like it more :)
    /// </summary>
    public void OnEventCustom(EventData data)
    {
        HashTable t = (HashTable)data.CustomData;
        switch (data.Code)
        {
            case PropertiesKeys.ChatEvent:
                OnChatReceive(t);
                break;
        }
    }
}