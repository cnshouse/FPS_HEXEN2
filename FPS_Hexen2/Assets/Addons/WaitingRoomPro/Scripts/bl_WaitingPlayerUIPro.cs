using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPS.Runtime.FriendList;

public class bl_WaitingPlayerUIPro : MonoBehaviour
{
    public GameObject AddFriendButton;
    private bl_WaitingPlayerUI WaitingUI;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        WaitingUI = GetComponent<bl_WaitingPlayerUI>();
        AddFriendButton.SetActive(false);
    }

    private void Start()
    {
        bool isf = bl_FriendList.Instance.haveThisPlayerAsFriend(WaitingUI.ThisPlayer.NickName);
        AddFriendButton.SetActive(bl_FriendList.Instance.haveFriendsSlots && !WaitingUI.ThisPlayer.IsLocal && !isf);
    }

    /// <summary>
    /// 
    /// </summary>
    public void AddFriend()
    {
        string playerName = WaitingUI.ThisPlayer.NickName;
        bl_FriendList.Instance.AddFriend(playerName);
    }
}