using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_PlayerUIBank : MonoBehaviour
{
    [Header("REFERENCES")]
    public Canvas PlayerUICanvas;
    public GameObject KillZoneUI;
    public GameObject WeaponStatsUI;
    public GameObject playerStatsUI;
    public GameObject MaxKillsUI;
    public GameObject SpeakerIcon;
    public GameObject TimeUIRoot;
    public GameObject MaxKillsUIRoot;
    public Image PlayerStateIcon;
    public Image SniperScope;
    public Image HealthBar;

    public Text AmmoText;
    public Text ClipText;
    public Text HealthText;
    public Text TimeText;
    public Text FireTypeText;
    public CanvasGroup DamageAlpha;
    public bl_WeaponLoadoutUI LoadoutUI;
    public Gradient AmmoTextColorGradient;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        UpdateUIDisplay();
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateUIDisplay()
    {
        TimeUIRoot.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Time));
        WeaponStatsUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.WeaponData));
        playerStatsUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.PlayerStats));
        if (LoadoutUI != null) LoadoutUI.SetActive(bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.Loadout));
    }

    /// <summary>
    /// 
    /// </summary>
    public void UpdateWeaponState(bl_Gun gun)
    {
        int bullets = gun.bulletsLeft;
        int clips = gun.numberOfClips;
        float per = (float)bullets / (float)gun.bulletsPerClip;
        Color c = AmmoTextColorGradient.Evaluate(per);

        if (gun.Info.Type != GunType.Knife)
        {
            AmmoText.text = bullets.ToString();
            if (gun.HaveInfinityAmmo)
                ClipText.text = "∞";
            else
                ClipText.text = ClipText.text = clips.ToString("F0");
            AmmoText.color = c;
            ClipText.color = c;
        }
        else
        {
            AmmoText.text = "--";
            ClipText.text = ClipText.text = "--";
            AmmoText.color = Color.white;
            ClipText.color = Color.white;
        }
    }
}