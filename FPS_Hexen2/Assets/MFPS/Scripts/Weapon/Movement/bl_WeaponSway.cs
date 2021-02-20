using UnityEngine;
using System.Collections;

public class bl_WeaponSway : bl_MonoBehaviour
{
    #region Public members
    private float maxAmount = 0.05F;
    [Header("Movements")]
    [Range(0.2f, 5)] public float delayAmplitude = 2;
    [Range(0.01f, 0.5f)] public float pushAmplutide = 0.2f;
    [Range(1, 7)] public float sideAngleAmplitude = 4;
    public float Smoothness = 3.0F;

    [Header("FallEffect")]
    [Range(0.01f, 1.0f)]
    public float m_time = 0.2f;
    public float m_ReturnSpeed = 5;
    public float SliderAmount = 12;
    public float DownAmount = 13;
    public float Amount { get; set; }
    #endregion

    #region Private members
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Vector3 targetVector = Vector3.zero;
    private Transform m_Transform;
    private float factorX, factorY, factorZ = 0;
    private Vector3 defaultEuler;
    private float deltaTime;
    private bool isAiming = false;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_Transform = transform;
        defaultPosition = m_Transform.localPosition;
        defaultRotation = m_Transform.localRotation;
        Amount = delayAmplitude;
        defaultEuler = m_Transform.localEulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!bl_RoomMenu.Instance.isCursorLocked)
            return;

        deltaTime = Time.smoothDeltaTime;
        DelayMovement();
        SideMovement();

        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, defaultRotation, deltaTime * m_ReturnSpeed);
    }

    /// <summary>
    /// The delay effect movement when move the camera with the mouse.
    /// </summary>
    void DelayMovement()
    {
        if (bl_UtilityHelper.isMobile) return;

        factorX = -bl_GameInput.MouseX * deltaTime * Amount;
        factorY = -bl_GameInput.MouseY * deltaTime * Amount;
        factorZ = -bl_GameInput.Vertical * (isAiming ? pushAmplutide * 0.1f : pushAmplutide);
        factorX = Mathf.Clamp(factorX, -maxAmount, maxAmount);
        factorY = Mathf.Clamp(factorY, -maxAmount, maxAmount);
        targetVector = new Vector3(defaultPosition.x + factorX, defaultPosition.y + factorY, factorZ);
        m_Transform.localPosition = Vector3.Lerp(m_Transform.localPosition, targetVector, deltaTime * Smoothness);
    }

    /// <summary>
    /// The angle oscillation movement when the player move sideway
    /// </summary>
    void SideMovement()
    {
        factorX = bl_GameInput.Horizontal;
        defaultEuler.z = factorX * sideAngleAmplitude;
        defaultEuler.z = -defaultEuler.z;
        defaultRotation = Quaternion.Euler(defaultEuler);
        defaultRotation = Quaternion.Slerp(defaultRotation, Quaternion.identity, deltaTime * Smoothness);
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onPlayerLand += this.OnSmallImpact;
        bl_EventHandler.onLocalAimChanged += OnLocalAimChanged;
        bl_EventHandler.onChangeWeapon += OnLocalWeaponChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onPlayerLand -= this.OnSmallImpact;
        bl_EventHandler.onLocalAimChanged -= OnLocalAimChanged;
        bl_EventHandler.onChangeWeapon -= OnLocalWeaponChanged;
    }

    /// <summary>
    /// On Impact event
    /// </summary>
    void OnSmallImpact()
    {
        StartCoroutine(FallEffect());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aim"></param>
    void OnLocalAimChanged(bool aim)
    {
        isAiming = aim;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newWeapon"></param>
    void OnLocalWeaponChanged(int newWeapon)
    {
        isAiming = false;
    }

    /// <summary>
    /// create a soft impact effect
    /// </summary>
    /// <returns></returns>
    public IEnumerator FallEffect()
    {
        Quaternion m_default = m_Transform.localRotation;
        Quaternion m_finaly = m_Transform.localRotation * Quaternion.Euler(new Vector3(DownAmount, Random.Range(-SliderAmount, SliderAmount), 0));
        float t_rate = 1.0f / m_time;
        float t_time = 0.0f;
        while (t_time < 1.0f)
        {
            t_time += Time.deltaTime * t_rate;
            m_Transform.localRotation = Quaternion.Slerp(m_default, m_finaly, t_time);
            yield return t_rate;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetSettings()
    {
        Amount = delayAmplitude;
    }
}