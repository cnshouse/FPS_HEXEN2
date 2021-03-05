using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClassPanel_UI : MonoBehaviour
{
    public MFPS.ClassCustomization.HudClassContent Active_HUD;

    //Inventory windows
    public MFPS.ClassCustomization.HudClassContent Primary_HUD;
    public MFPS.ClassCustomization.HudClassContent Secondary_HUD;
    public MFPS.ClassCustomization.HudClassContent Perk_HUD;
    public MFPS.ClassCustomization.HudClassContent Lethal_HUD;
    //public MFPS.ClassCustomization.HudClassContent Kit_HUD;

    public Text Slot_Name;

    //[Space(7)]
    //public RectTransform[] ClassButtons;
    //public Text ClassText = null;
    [Space(7)]
    public GameObject GunSelectPrefabs = null;
    public Transform PanelWeaponList = null;
    //public GameObject SaveButton;
    //public GameObject loadingUI;

    private void Awake()
    {
    //    SaveButton?.SetActive(false);
    }

	private void Start()
	{
        LobbyPlayerManager lp = FindObjectOfType<LobbyPlayerManager>();
        MFPS.ClassCustomization.bl_ClassCustomize.Instance.TakeCurrentClass(lp.mClass);
    }
}
