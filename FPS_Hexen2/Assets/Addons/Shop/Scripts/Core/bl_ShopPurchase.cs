using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class bl_ShopPurchase 
{
    public int TypeID = 0;
    public int ID = 0;
}

[System.Serializable]
public enum ShopItemType
{
    Weapon = 0,
    WeaponCamo = 1,
    PlayerSkin = 2,
    PlayerAccesory = 3,
    None = 99,
}