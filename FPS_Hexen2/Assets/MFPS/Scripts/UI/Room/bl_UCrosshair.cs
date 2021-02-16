using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MFPS.Internal.Structures;

public class bl_UCrosshair : bl_MonoBehaviour
{

    [Header("Settings")]
    [LovattoToogle] public bool fadeOnAim = true;
    [Range(1, 10)] public float ScaleLerp = 5;
    [Range(0.1f, 5)] public float RotationSpeed = 2;
    [Range(0.01f, 1)] public float OnFireScaleRate = 0.1f;
    [Range(0.01f, 1)] public float OnCrouchReduce = 8;

    [Header("Hit Marker")]
    [Range(0.1f, 3)] public float Duration = 1;
    [Range(5, 50)] public float IncreaseAmount = 25;
    public Color HitMarkerColor = Color.white;

    [Header("References")]
    [SerializeField] private bl_UCrosshairInfo genericCrosshair = null;
    [SerializeField] private WeaponCrosshair[] weaponCrosshairs = null;

    [SerializeField] private RectTransform RootContent;
    [SerializeField] private RectTransform HitMarkerRoot;

    public bool Block { get; set; }
    private Vector2 InitSizeDelta;
    private bool isAim = false;
    public bool isCrouch { get; set; }
    private Canvas m_Canvas;
    private Vector3 InitialRotation;
    private float lastTimeFire;
    private CanvasGroup m_HitAlpha;
    private Vector2 defaultHitSize;
    private float hitDuration;
    private CanvasGroup CrossAlpha;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (RootContent != null)
        {
            InitSizeDelta = RootContent.sizeDelta;
            InitialRotation = RootContent.eulerAngles;
            CrossAlpha = RootContent.GetComponent<CanvasGroup>();
        }
        if (HitMarkerRoot != null)
        {
            m_HitAlpha = HitMarkerRoot.GetComponent<CanvasGroup>();
            defaultHitSize = HitMarkerRoot.sizeDelta;
            if (m_HitAlpha != null) { m_HitAlpha.alpha = 0; }
            Graphic[] hmg = HitMarkerRoot.GetComponentsInChildren<Graphic>();
            foreach (Graphic g in hmg) { g.color = HitMarkerColor; }
        }
        m_Canvas = transform.root.GetComponent<Canvas>();
        if (m_Canvas == null) { m_Canvas = transform.root.GetComponentInChildren<Canvas>(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (GetCrosshair.isStatic)
            return;

        ScaleContent();
        FadeControll();
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        if (m_HitAlpha != null)
        {
            m_HitAlpha.alpha = 0;
        }
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
        RootContent.gameObject.SetActive(bl_GameData.Instance.showCrosshair);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        isCrouch = false;
        isAim = false;
    }

    /// <summary>
    /// 
    /// </summary>
    void ScaleContent()
    {
        if (RootContent == null)
            return;

        Vector2 target = (isAim) ? new Vector2(GetCrosshair.OnAimScaleAmount, GetCrosshair.OnAimScaleAmount) : InitSizeDelta;
        Vector2 size = (isCrouch) ? target - CrouchVector : target;
        RootContent.sizeDelta = Vector2.Lerp(RootContent.sizeDelta, size, Time.unscaledDeltaTime * ScaleLerp);
    }

    /// <summary>
    /// 
    /// </summary>
    void FadeControll()
    {
        if (CrossAlpha == null)
            return;

        float at = (isAim && fadeOnAim) ? 0 : 1;
        CrossAlpha.alpha = Mathf.Lerp(CrossAlpha.alpha, at, Time.deltaTime * 8);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gun"></param>
    public void ActiveCrosshairForWeapon(bl_Gun gun)
    {
        if (gun == null) return;
        ActiveCrosshairForWeapon(gun.WeaponType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gun"></param>
    public void ActiveCrosshairForWeapon(GunType gun)
    {
        bool found = false;
        for (int i = 0; i < weaponCrosshairs.Length; i++)
        {
            if (weaponCrosshairs[i].gunType == gun)
            {
                weaponCrosshairs[i].crosshair?.SetActive(true);
                m_currentCrosshair = weaponCrosshairs[i].crosshair;
                found = true;
            }
            else
            {
                weaponCrosshairs[i].crosshair?.SetActive(false);
            }
        }
        if (!found)
        {
            ActiveGenericCrosshair();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ActiveGenericCrosshair()
    {
        genericCrosshair.SetActive(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="show"></param>
    public void Show(bool show)
    {
        if (Block || !bl_GameData.Instance.showCrosshair)
            return;

        RootContent.gameObject.SetActive(show);
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnFire()
    {
        if (RootContent == null)
            return;
        if (GetCrosshair.isStatic)
            return;
        if (Time.time < lastTimeFire)
            return;

        Vector2 size = (isCrouch) ? new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount) - CrouchVector : new Vector2(GetCrosshair.OnFireScaleAmount, GetCrosshair.OnFireScaleAmount);
        RootContent.sizeDelta = size;
        lastTimeFire = Time.time + OnFireScaleRate;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnAim(bool aim)
    {
        if (RootContent == null)
            return;
        if (GetCrosshair.isStatic)
            return;

        isAim = aim;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnCrouch(bool crouch)
    {
        if (RootContent == null)
            return;
        if (GetCrosshair.isStatic)
            return;

        isCrouch = crouch;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnHit()
    {
        if (HitMarkerRoot == null)
            return;

        StopCoroutine("OnHitMarker");
        StartCoroutine("OnHitMarker");
    }


    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
        RootContent.eulerAngles = InitialRotation;
    }

    /// <summary>
    /// 
    /// </summary>
    private bl_UCrosshairInfo m_currentCrosshair;
    public bl_UCrosshairInfo GetCrosshair
    {
        get
        {
            if (m_currentCrosshair == null) m_currentCrosshair = genericCrosshair;
            return m_currentCrosshair;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator OnHitMarker()
    {
        hitDuration = 0;
        HitMarkerRoot.sizeDelta = defaultHitSize;
        if (m_HitAlpha != null) { m_HitAlpha.alpha = 1; }
        Vector2 sizeTarget = new Vector2(IncreaseAmount, IncreaseAmount);
        while (hitDuration < 1)
        {
            HitMarkerRoot.sizeDelta = Vector2.Lerp(HitMarkerRoot.sizeDelta, sizeTarget, hitDuration);
            if (m_HitAlpha != null) { m_HitAlpha.alpha = Mathf.Lerp(m_HitAlpha.alpha, 0, hitDuration); }
            hitDuration += Time.unscaledDeltaTime / Duration;
            yield return null;
        }
    }

    private Vector2 CrouchVector
    {
        get
        {
            return isCrouch ? Vector2.one * OnCrouchReduce : Vector2.zero;
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (RootContent == null)
            return;

        RootContent.sizeDelta = new Vector2(GetCrosshair.NormalScaleAmount, GetCrosshair.NormalScaleAmount);
    }
#endif

    [System.Serializable]
    public class WeaponCrosshair
    {
        public GunType gunType;
        public bl_UCrosshairInfo crosshair;
    }

    private static bl_UCrosshair m_instance;
    public static bl_UCrosshair Instance
    {
        get
        {
            if (m_instance == null)
            {
                bl_UCrosshair[] all = FindObjectsOfType<bl_UCrosshair>();
                if(all.Length <= 0) { Debug.LogWarning("There are not an cross hair in this scene!"); return null; }
                else
                {
                    if(all.Length > 1) { Debug.LogWarning("There are 2 or more cross hair in this scene, if you use multiple cross hair, get the reference manually instead of by singleton!"); }
                    m_instance = all[0];
                }
            }
            return m_instance;
        }
    }
}