using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_WeaponLoadoutUI : MonoBehaviour
{
    [SerializeField]private LayoutShowMode showMode = LayoutShowMode.AutoHide;
    [SerializeField] private RectTransform BackRect = null;
    [SerializeField] private RectTransform[] SlotsGroups = null;
    private Image[] IconsImg = null;
    [SerializeField] private CanvasGroup Alpha = null;

    private int current = 0;
    public bool CanShow { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        Alpha.alpha = 0;
        Alpha.gameObject.SetActive(false);
        IconsImg = new Image[SlotsGroups.Length];
        for (int i = 0; i < SlotsGroups.Length; i++)
        {
            IconsImg[i] = SlotsGroups[i].GetComponentInChildren<Image>();
        }
        IconsImg[0].CrossFadeColor(Color.black, 0.1f, true, true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetInitLoadout(List<bl_Gun> guns)
    {
        for (int i = 0; i < SlotsGroups.Length; i++)
        {
            IconsImg[i].canvasRenderer.SetColor(Color.white);
            if(guns[i] == null || guns[i].Info == null) { SlotsGroups[i].gameObject.SetActive(false); continue; }
            
            Image img = SlotsGroups[i].GetComponentInChildren<Image>(false);
            img.sprite = guns[i].Info.GunIcon;
        }
        BackRect.position = SlotsGroups[0].position;
        current = 0;
        IconsImg[0].canvasRenderer.SetColor(Color.black);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ReplaceSlot(int slot, bl_Gun newGun)
    {
        if(IconsImg[slot].sprite == null)
        {
            Color c = IconsImg[slot].color;
            c.a = 1;
            IconsImg[slot].color = c;
        }
        IconsImg[slot].sprite = newGun.Info.GunIcon;
        SlotsGroups[slot].gameObject.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    public void ChangeWeapon(int nextSlot)
    {
        if (!bl_GameData.Instance.ShowWeaponLoadout || !CanShow) return;

        StopAllCoroutines();
        StartCoroutine(ChangeSlot(nextSlot));
    }

    /// <summary>
    /// 
    /// </summary>
    public void ClearAllIcons(bool playTransition = false)
    {
        for (int i = 0; i < IconsImg.Length; i++)
        {
            IconsImg[i].sprite = null;
            Color c = IconsImg[i].color;
            c.a = 0;
            IconsImg[i].color = c;
        }
        if (playTransition)
        {
            StopAllCoroutines();
            StartCoroutine(ChangeSlot(current));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeSlot(int nextSlot)
    {
        int cacheActual = current;
        current = nextSlot;
        Alpha.gameObject.SetActive(true);
        while (Alpha.alpha < 1)
        {
            Alpha.alpha += Time.deltaTime * 4;
            yield return null;
        }
        float d = 0;
        IconsImg[cacheActual].CrossFadeColor(Color.white, 0.3f, true, true);
        IconsImg[nextSlot].CrossFadeColor(Color.black, 0.3f, true, true);
        while (d < 1)
        {
            d += Time.deltaTime * 7;
            BackRect.position = Vector3.Lerp(SlotsGroups[cacheActual].position, SlotsGroups[nextSlot].position, d);
            yield return null;
        }
        if (ShowMode == LayoutShowMode.AutoHide)
        {
            yield return new WaitForSeconds(2.5f);
            while (Alpha.alpha > 0)
            {
                Alpha.alpha -= Time.deltaTime * 4;
                yield return null;
            }
            Alpha.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActive(bool active)
    {
        Alpha.gameObject.SetActive(active);
    }

    public LayoutShowMode ShowMode
    {
        get { return showMode; }
        set
        {
            showMode = value;
            if(showMode == LayoutShowMode.AlwaysShow)
            {
                StopAllCoroutines();
                if(Alpha.alpha <= 0)
                {
                    StartCoroutine(ChangeSlot(current));
                }
            }
        }
    }

    [System.Serializable]
    public enum LayoutShowMode
    {
        AutoHide,
        AlwaysShow,
    }

    private static bl_WeaponLoadoutUI _instance;
    public static bl_WeaponLoadoutUI Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_WeaponLoadoutUI>(); }
            return _instance;
        }
    }
}