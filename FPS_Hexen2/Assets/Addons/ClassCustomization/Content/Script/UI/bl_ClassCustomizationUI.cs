using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ClassCustomization
{
    public class bl_ClassCustomizationUI : MonoBehaviour
    {
        [Header("HUD")]
        public HudClassContent PrimaryHUD;
        public HudClassContent SecundaryHUD;
        public HudClassContent KnifeHUD;
        public HudClassContent GrenadeHUD;
        [Space(7)]
        public RectTransform[] ClassButtons;
        public Text ClassText = null;
        [Space(7)]
        public GameObject GunSelectPrefabs = null;
        public Transform PanelWeaponList = null;
        public GameObject SaveButton;
        public GameObject loadingUI;

        private void Awake()
        {
            SaveButton?.SetActive(false);
        }
    }
}