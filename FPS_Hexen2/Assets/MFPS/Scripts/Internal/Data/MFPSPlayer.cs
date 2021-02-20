using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[Serializable]
public class MFPSPlayer
{
    public string Name;
    public Transform Actor;
    public PhotonView ActorView;
    public bool isRealPlayer = true;
    public bool isAlive = true;
    public Team Team = Team.None;
    public Transform AimPosition;

    public int ActorNumber
    {
        get
        {
            if (ActorView == null || ActorView.Owner == null) return -1;
            return ActorView.Owner.ActorNumber;
        }
    }

    public MFPSPlayer() { }

    public MFPSPlayer(PhotonView view, bool realPlayer = true, bool alive = true)
    {
        isRealPlayer = realPlayer;
        isAlive = alive;
        if (view == null) return;

        Actor = view.transform;
        ActorView = view;
        AimPosition = Actor;
        if (view.InstantiationData != null)
        {
            if (!isRealPlayer)
            {
                Name = (string)view.InstantiationData[0];
                Team = (Team)view.InstantiationData[1];
            }
            else
            {
                Name = view.Owner.NickName;
                Team = (Team)view.InstantiationData[0];
            }
        }
    }
}