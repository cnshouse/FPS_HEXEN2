using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_PickUpUI : MonoBehaviour
{
    public GameObject content;
    public Text mainText;
    public Image iconImg;
    public Text keyText;

    private bl_GunPickUp CacheGunPickUp = null;

#if LOCALIZATION
    private int[] LocaleTextIDs = new int[] { 13, 12 };
    private string[] LocaleStrings;
#endif

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
#if LOCALIZATION
        LocaleStrings = bl_Localization.Instance.GetTextArray(LocaleTextIDs);
#endif
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        Hide();
    }

    public void Show(string text) => Show(text, null, string.Empty);

    public void Show(string text, Sprite icon, string pickKey = "")
    {
        mainText.text = text;
        iconImg.sprite = icon;
        iconImg.gameObject.SetActive(icon != null);
        if (!bl_UtilityHelper.isMobile)
            keyText.text = pickKey.ToUpper();
        else
            keyText.text = bl_GameTexts.Touch.ToUpper();

        content.SetActive(true);
    }

    public void SetPickUp(bool show, int id = 0, bl_GunPickUp gun = null, bool equiped = false)
    {
        if (show)
        {
            bl_GunInfo info = bl_GameData.Instance.GetWeapon(id);
#if LOCALIZATION
            string t = (equiped) ? string.Format(LocaleStrings[0], info.Name) : string.Format(LocaleStrings[1], bl_GameData.Instance.GetWeapon(id).Name);
#else
            string t = (equiped) ? string.Format(bl_GameTexts.PickUpWeaponEquipped, info.Name) : string.Format(bl_GameTexts.PickUpWeapon, bl_GameData.Instance.GetWeapon(id).Name);
#endif
            Show(t.ToUpper(), info.GunIcon, KeyCode.E.ToString());
            CacheGunPickUp = gun;
        }
        else Hide();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Hide()
    {
        content.SetActive(false);
    }

    public void OnPickUpClicked()
    {
        if (CacheGunPickUp != null)
        {
            CacheGunPickUp.Pickup();
        }
    }

    private static bl_PickUpUI _instance;
    public static bl_PickUpUI Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_PickUpUI>(); }
            return _instance;
        }
    }
}