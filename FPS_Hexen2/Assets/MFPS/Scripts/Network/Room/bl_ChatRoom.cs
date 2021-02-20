using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class bl_ChatRoom : bl_MonoBehaviour
{
    public KeyCode globalChatKey = KeyCode.T;
    public KeyCode teamChatKey = KeyCode.Y;
    public int messageBufferLenght = 7;

    private List<string> messages = new List<string>();
    private MessageTarget messageTarget = MessageTarget.All;

    [Header("References")]
    public GameObject chatUIRoot;
    public InputField chatInputField;
    public Text chatText;

    public bool CanUseTheChat { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        bl_GameData.Instance.isChating = false;
        this.InvokeAfter(5, HideChat);
        if (chatInputField != null) chatInputField.gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.ChatEvent, OnNetworkMessageReceived);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        if (bl_PhotonNetwork.Instance != null)
            bl_PhotonNetwork.Instance.RemoveCallback(OnNetworkMessageReceived);
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
#if LOCALIZATION
        AddChatLocally(bl_Localization.Instance.GetText(29));
#else
        AddChatLocally(bl_GameTexts.OpenChatStart);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!CanUseTheChat) return;

        if (!bl_GameData.Instance.isChating)
        {
            if (Input.GetKeyDown(globalChatKey))
            {
                messageTarget = MessageTarget.All;
                OnSubmit();
            }
            if (Input.GetKeyDown(teamChatKey))
            {
                messageTarget = MessageTarget.Team;
                OnSubmit();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnSubmit();
            }
        }
    }

    /// <summary>
    /// Receive network message from chat event from the server
    /// </summary>
    /// <param name="data"></param>
    void OnNetworkMessageReceived(ExitGames.Client.Photon.Hashtable data)
    {
        var messageTarget = (MessageTarget)data["target"];
        var sender = (Player)data["sender"];
        var message = (string)data["chat"];

        if (messageTarget == MessageTarget.Team && sender.GetPlayerTeam() != PhotonNetwork.LocalPlayer.GetPlayerTeam())//check if team message and only team receive it
            return;

        string senderName = "anonymous";

        if (sender != null)
        {
            senderName = string.IsNullOrEmpty(sender.NickName) ? $"Player {sender.ActorNumber}" : sender.NickName;
        }

        string txt = string.Format("[{0}] [{1}]:{2}", messageTarget.ToString().ToUpper(), senderName, message);
        this.messages.Add(txt);
        if (messages.Count > messageBufferLenght)
        {
            messages.RemoveAt(0);
        }

        chatText.text = string.Empty;
        foreach (string m in messages)
        {
            chatText.text += m + "\n";
        }
        CancelInvoke(nameof(HideChat));
        chatText.CrossFadeAlpha(1, 0.3f, true);
        Invoke(nameof(HideChat), 5);
    }

    /// <summary>
    /// Send text message over network to all other clients
    /// </summary>
    /// <param name="txt"></param>
    public void SetChat(string txt)
    {
        if (!PhotonNetwork.IsConnected || !CanUseTheChat)
            return;
        if (string.IsNullOrEmpty(txt)) return;

        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("sender", PhotonNetwork.LocalPlayer);
        data.Add("target", messageTarget);
        data.Add("chat", txt);

        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.ChatEvent, data);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnSubmit()
    {
        bl_GameData.Instance.isChating = !bl_GameData.Instance.isChating;
        chatInputField.gameObject.SetActive(bl_GameData.Instance.isChating);

        if (bl_GameData.Instance.isChating)
        {
            chatInputField.text = string.Empty;
            chatInputField.ActivateInputField();
        }
        else
        {
            SetChat(chatInputField.text);
            chatInputField.DeactivateInputField();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void HideChat()
    {
        chatText.CrossFadeAlpha(0, 2, true);
    }

    /// <summary>
    /// Post a message in the chat locally only.
    /// </summary>
    /// <param name="newLine"></param>
    public void AddChatLocally(string newLine)
    {
        this.messages.Add(newLine);
        if (messages.Count > messageBufferLenght)
        {
            messages.RemoveAt(0);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Refresh()
    {
        chatText.text = string.Empty;
        foreach (string m in messages)
            chatText.text += m + "\n";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    public void SubmitInputField(InputField field)
    {

    }

    public enum MessageTarget
    {
        All,
        Team,
    }

    private static bl_ChatRoom _instance;
    public static bl_ChatRoom Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ChatRoom>(); }
            return _instance;
        }
    }
}