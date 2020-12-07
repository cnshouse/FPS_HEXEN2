using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class bl_CustomizerWeapon : MonoBehaviour
{
    public int WeaponID = 0;
    public string WeaponName;
    public bool ApplyOnStart = true;
    public bool isFPWeapon = true;

    public CustomizerAttachments Attachments;
    public CustomizerCamoRender CamoRender;
    private int[] AttachmentsIds = new int[] { 0, 0, 0, 0, 0 };
    private bool isSync = false;
    private bl_Gun Gun;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        if (isFPWeapon && !isSync)
        {
            LoadAttachments();
            ApplyAttachments();
            Gun = GetComponent<bl_Gun>();
            string line = bl_CustomizerData.Instance.CompileArray(AttachmentsIds);
            transform.root.GetComponent<PhotonView>().RPC("SyncCustomizer", RpcTarget.Others, Gun.GunID, line);
            isSync = true;
            bl_PhotonCallbacks.PlayerEnteredRoom += OnNewPlayerEnter;
        }
    }

    void OnNewPlayerEnter(Player player)
    {
        if (isFPWeapon)
        {
            string line = bl_CustomizerData.Instance.CompileArray(AttachmentsIds);
            transform.root.GetComponent<PhotonView>().RPC("SyncCustomizer", RpcTarget.Others, Gun.GunID, line);
        }
    }

    private void OnDestroy()
    {
        bl_PhotonCallbacks.PlayerEnteredRoom -= OnNewPlayerEnter;
    }

    /// <summary>
    /// 
    /// </summary>
    public void LoadAttachments()
    {
        AttachmentsIds = bl_CustomizerData.Instance.LoadAttachmentsForWeapon(WeaponName);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ApplyAttachments()
    {
        Attachments.Apply(AttachmentsIds);
        CamoRender.ApplyCamo(WeaponName, AttachmentsIds[(int)bl_AttachType.Camo]);
    }

    public void ApplyAttachments(int[] customIds)
    {
        Attachments.Apply(customIds);
        CamoRender.ApplyCamo(WeaponName, customIds[(int)bl_AttachType.Camo]);
    }

    public void ApplyAttachments(string line)
    {
        AttachmentsIds = bl_CustomizerData.Instance.DecompileLine(line);
        ApplyAttachments();
    }

#if UNITY_EDITOR

    public void BuildAttachments()
    {
        if(Attachments == null) { Attachments = new CustomizerAttachments(); }
        Attachments.Suppressers.Clear();
        Attachments.Sights.Clear();
        Attachments.Foregrips.Clear();
        Attachments.Magazines.Clear();
        CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(WeaponName);
    
        for (int i = 0; i < info.Attachments.Suppressers.Count; i++)
        {
            Attachments.Suppressers.Add(new CustomizerModelInfo());
            Attachments.Suppressers[i].SetInfo(info.Attachments.Suppressers[i]);
        }
        for (int i = 0; i < info.Attachments.Sights.Count; i++)
        {
            Attachments.Sights.Add(new CustomizerModelInfo());
            Attachments.Sights[i].SetInfo(info.Attachments.Sights[i]);
        }
        for (int i = 0; i < info.Attachments.Foregrips.Count; i++)
        {
            Attachments.Foregrips.Add(new CustomizerModelInfo());
            Attachments.Foregrips[i].SetInfo(info.Attachments.Foregrips[i]);
        }
        for (int i = 0; i < info.Attachments.Magazines.Count; i++)
        {
            Attachments.Magazines.Add(new CustomizerModelInfo());
            Attachments.Magazines[i].SetInfo(info.Attachments.Magazines[i]);
        }
    }

    public void RefreshAttachments()
    {
        CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(WeaponName);
        if (Attachments.Suppressers.Count != info.Attachments.Suppressers.Count)
        {
            for (int i = 0; i < info.Attachments.Suppressers.Count; i++)
            {
                if (Attachments.Suppressers.Count - 1 < i)
                {
                    Attachments.Suppressers.Add(new CustomizerModelInfo());
                }
                Attachments.Suppressers[i].SetInfo(info.Attachments.Suppressers[i]);
            }
        }
        if (Attachments.Sights.Count != info.Attachments.Sights.Count)
        {
            for (int i = 0; i < info.Attachments.Sights.Count; i++)
            {
                if (Attachments.Sights.Count - 1 < i)
                {
                    Attachments.Sights.Add(new CustomizerModelInfo());
                }
                Attachments.Sights[i].SetInfo(info.Attachments.Sights[i]);
            }
        }
        if (Attachments.Foregrips.Count != info.Attachments.Foregrips.Count)
        {
            for (int i = 0; i < info.Attachments.Foregrips.Count; i++)
            {
                if (Attachments.Foregrips.Count - 1 < i)
                {
                    Attachments.Foregrips.Add(new CustomizerModelInfo());
                }
                Attachments.Foregrips[i].SetInfo(info.Attachments.Foregrips[i]);
            }
        }
        if (Attachments.Magazines.Count != info.Attachments.Magazines.Count)
        {
            for (int i = 0; i < info.Attachments.Magazines.Count; i++)
            {
                if (Attachments.Magazines.Count - 1 < i)
                {
                    Attachments.Magazines.Add(new CustomizerModelInfo());
                }
                Attachments.Magazines[i].SetInfo(info.Attachments.Magazines[i]);
            }
        }
    }
#endif
}