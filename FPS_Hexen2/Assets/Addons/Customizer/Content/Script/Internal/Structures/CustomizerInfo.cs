using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CustomizerInfo 
{
    public string WeaponName;
    [GunID] public int GunID = 0;
    public List<CamoInfo> Camos = new List<CamoInfo>();
    public ListCustomizer Attachments;
}

[System.Serializable]
public class ListCustomizer
{
    public List<AttachInfo> Suppressers = new List<AttachInfo>();
    public List<AttachInfo> Sights = new List<AttachInfo>();
    public List<AttachInfo> Foregrips = new List<AttachInfo>();
    public List<AttachInfo> Magazines = new List<AttachInfo>();
}

[System.Serializable]
public class CustomizerModelInfo
{
    public string Name = "";
    public int ID = 0;
    public GameObject Model;

    [HideInInspector] public AttachInfo Info;

    public void SetInfo(AttachInfo _info)
    {
        Name = _info.Name;
        ID = _info.ID;
        Info = _info;
    }
}

[System.Serializable]
public class CustomizerCamoRender
{
    public int MaterialID = 0;
    public Renderer Render;

    [HideInInspector] public CamoInfo Info;

    public void SetInfo(CamoInfo _info)
    {
        Info = _info;
    }

    public Material ApplyCamo(string weapon, int camoID)
    {
        if (Render == null) return null;

        Material m = bl_CustomizerData.Instance.GetWeapon(weapon).Camos.Find(x => x.ID == camoID).Camo;
        Material[] mats = Render.materials;
        mats[MaterialID] = m;
        Render.materials = mats;
        return m;
    }
}

[System.Serializable]
public class CustomizerAttachments
{
    public List<CustomizerModelInfo> Suppressers = new List<CustomizerModelInfo>();
    public List<CustomizerModelInfo> Sights = new List<CustomizerModelInfo>();
    public List<CustomizerModelInfo> Foregrips = new List<CustomizerModelInfo>();
    public List<CustomizerModelInfo> Magazines = new List<CustomizerModelInfo>();

    public void GetAttachmentInfo(string weapon)
    {
        CustomizerInfo info = bl_CustomizerData.Instance.GetWeapon(weapon);
        for (int i = 0; i < Suppressers.Count; i++)
        {
            Suppressers[i].Info = info.Attachments.Suppressers[Suppressers[i].ID];
        }
        for (int i = 0; i < Sights.Count; i++)
        {
            Sights[i].Info = info.Attachments.Sights[Sights[i].ID];
        }
        for (int i = 0; i < Foregrips.Count; i++)
        {
            Foregrips[i].Info = info.Attachments.Foregrips[Foregrips[i].ID];
        }
        for (int i = 0; i < Magazines.Count; i++)
        {
            Magazines[i].Info = info.Attachments.Magazines[Magazines[i].ID];
        }
    }

    public void Apply(int[] array)
    {
        Suppressers.ForEach(x => { if (x.Model != null) { x.Model.SetActive(false); } });
        Sights.ForEach(x => { if (x.Model != null) { x.Model.SetActive(false); } });
        Foregrips.ForEach(x => { if (x.Model != null) { x.Model.SetActive(false); } });
        Magazines.ForEach(x => { if (x.Model != null) { x.Model.SetActive(false); } });
        if (Suppressers.Count > 0 && Suppressers[array[0]].Model != null)
            Suppressers[array[0]].Model.SetActive(true);
        if (Sights.Count > 0 && Sights[array[1]].Model != null)
            Sights[array[1]].Model.SetActive(true);
        if (Foregrips.Count > 0 && Foregrips[array[2]].Model != null)
            Foregrips[array[2]].Model.SetActive(true);
        if (Magazines.Count > 0 && Magazines[array[3]].Model != null)
            Magazines[array[3]].Model.SetActive(true);
    }
}

[System.Serializable]
public class AttachInfo
{
    public string Name;
    public int ID;
    public string Description;
}

[System.Serializable]
public class CamoInfo
{
    public string Name;
    public int GlobalID = 0;
    public int ID;
    public Material Camo;
    public Texture2D OverridePreview;

    public int ofWeaponID = 0;

    public Texture2D Preview
    {
        get
        {
            if(OverridePreview != null) { return OverridePreview; }
            return bl_CustomizerData.Instance.GlobalCamos[GlobalID].Preview;
        }
    }
}

[System.Serializable]
public class GlobalCamo
{
    public string Name;
    public Texture2D Preview;
    public int Price = 0;
    public string Description;

    public bool isFree() { return Price <= 0; }
    [HideInInspector] public Sprite m_sprite = null;
    public Sprite spritePreview()
    {
        if(m_sprite == null && Preview != null)
        {
            m_sprite = Sprite.Create(Preview, new Rect(0, 0, Preview.width, Preview.height), new Vector2(0.5f, 0.5f));
        }
        return m_sprite;
    }
}