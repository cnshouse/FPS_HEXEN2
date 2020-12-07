using UnityEngine;
using UnityEngine.UI;

public class bl_CustomizerInfoButton : MonoBehaviour {

	[SerializeField]private Text m_Text;
    public Image weaponIcon;
    public Text lockedText;
    public Button button;
    public GameObject lockedUI;

    private bl_CustomizerInfoButton[] AllButtons;
    private bl_Customizer customizerWeapon;
    public int lockedStatus { get; set; }

    public void Init(bl_Customizer weapon)
    {
        lockedStatus = 0;
        customizerWeapon = weapon;
        bl_GunInfo info = customizerWeapon.GetWeaponInfo();
        m_Text.text = customizerWeapon.WeaponName;
        weaponIcon.sprite = info.GunIcon;

#if SHOP && ULSP
        if (info.Price > 0 && bl_DataBase.Instance != null)
        {
            if (bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(ShopItemType.Weapon, customizerWeapon.GunID()))
            {
                lockedStatus = bl_ShopData.Instance.purchaseOverrideLevelLock ? 3 : 0;
            }
            else
            {
                lockedStatus = 1;
                lockedText.text = "PRICE: $" + info.Price;
                button.interactable = false;
            }
        }
#endif
#if LM
        if (bl_GameData.Instance.LockWeaponsByLevel && (lockedStatus == 0 || lockedStatus == 3))
        {
            int al = bl_LevelManager.Instance.GetLevelID();
            bool UnLock = (al >= info.LevelUnlock);
            lockedStatus = UnLock ? lockedStatus : 2;
            if (!UnLock)
            {
                lockedText.text = "REQUIRES LEVEL: " + info.LevelUnlock;
                button.interactable = false;
            }
        }
#endif
        
        lockedUI.SetActive(lockedStatus != 0 && lockedStatus != 3);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnSelect()
    {
        if (lockedStatus != 0 && lockedStatus != 3) return;
        if (AllButtons == null || AllButtons.Length <= 0) { AllButtons = transform.parent.GetComponentsInChildren<bl_CustomizerInfoButton>(); }

        bl_CustomizerManager c = FindObjectOfType<bl_CustomizerManager>();
        c.showCustomizerWeapon(customizerWeapon);

        foreach(bl_CustomizerInfoButton b in AllButtons)
        {
            b.Deselect();
        }
        button.interactable = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Deselect()
    {
        if (lockedStatus != 0 && lockedStatus != 3) return;
        button.interactable = true;
    }
}