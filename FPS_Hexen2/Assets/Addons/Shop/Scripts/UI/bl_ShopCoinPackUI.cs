using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Shop
{
    public class bl_ShopCoinPackUI : MonoBehaviour
    {

       public void Buy(int id)
        {
            bl_ShopManager.Instance.BuyCoinPack(id);
        }
    }
}