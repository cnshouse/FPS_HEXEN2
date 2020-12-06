using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MFPS.ULogin
{
    public class JsonULoginStructures
    {

    }

    [Serializable]
    public class CoinPurchaseData
    {
        public string productID;
        public int coins;
        public string receipt;
    }
}