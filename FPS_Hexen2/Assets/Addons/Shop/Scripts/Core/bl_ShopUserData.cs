using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class bl_ShopUserData 
{
    public string PurchasesSource;
    public List<bl_ShopPurchase> ShopPurchases = new List<bl_ShopPurchase>();

    public void GetInfo(string[] data)
    {
        PurchasesSource = data[14];
        //Debug.Log($"Purchases: {data[14]}");
        ShopPurchases = bl_ShopData.DecompilePurchases(PurchasesSource);
    }

    public bool isItemPurchase(ShopItemType typeID, int ID)
    {
        return ShopPurchases.Exists(x => x.TypeID == (int)typeID && x.ID == ID);
    }

    public bool isItemPurchase(bl_ShopItemData data)
    {
        return ShopPurchases.Exists(x => x.TypeID == (int)data.Type && x.ID == data.ID);
    }

    public void AddPurchase(bl_ShopPurchase purchase)
    {       
        ShopPurchases.Add(purchase);
    }
}