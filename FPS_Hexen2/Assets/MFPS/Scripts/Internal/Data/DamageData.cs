using UnityEngine;
using Photon.Realtime;

public class DamageData
{
    public int Damage = 10;
    public string From = "";
    public DamageCause Cause = DamageCause.Player;
    public Vector3 Direction { get; set; } = Vector3.zero;
    public bool isHeadShot { get; set; } = false;
    public int GunID { get; set; } = 0;
    public Player Actor { get; set; }
    public MFPSPlayer MFPSActor { get; set; }
    public int ActorViewID { get; set; }

    /// <summary>
    /// Create a hashtable with the this damage data
    /// </summary>
    /// <returns></returns>
    public ExitGames.Client.Photon.Hashtable GetAsHashtable()
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add("d", Damage);
        data.Add("gi", GunID);
        data.Add("vi", ActorViewID);
        data.Add("c", Cause);
        data.Add("f", From);
        if (Direction != Vector3.zero)
            data.Add("dr", Direction);

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    public DamageData(ExitGames.Client.Photon.Hashtable data)
    {
        Damage = (int)data["d"];
        GunID = (int)data["gi"];
        ActorViewID = (int)data["vi"];
        Cause = (DamageCause)data["c"];
        From = (string)data["f"];
        if (data.ContainsKey("dr")) Direction = (Vector3)data["dr"];

        MFPSActor = bl_GameManager.Instance.FindMFPSActor(ActorViewID);
    }

    public DamageData()
    {
    }
}