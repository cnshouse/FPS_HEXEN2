using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_ULoginUI : MonoBehaviour
    {
        public GameObject[] addonObjects;

        private static bl_ULoginUI _instance;
        public static bl_ULoginUI Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ULoginUI>(); }
                return _instance;
            }
        }
    }
}