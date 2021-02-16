using UnityEngine;

public class BulletData 
{
    public string WeaponName;
    public float Damage;
    public Vector3 Position;
    public float ImpactForce;
    public float MaxSpread;
    public float Spread;
    public float Speed;
    public float Range;
    public int WeaponID;
    public bool isNetwork;
    public int ActorViewID { get; set; }
    public MFPSPlayer MFPSActor { get; set; }

    public BulletData()
    {
    }

    public BulletData(DamageData data)
    {
        MFPSActor = data.MFPSActor;
        Damage = data.Damage;
        ActorViewID = data.ActorViewID;
        WeaponID = data.GunID;
        Position = data.Direction;
    }
}