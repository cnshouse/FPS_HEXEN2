using UnityEngine;

public class bl_Recoil : bl_MonoBehaviour
{
    [Range(1, 25)] public float MaxRecoil = 5;
    public bool AutomaticallyComeBack = true;

    private Transform m_Transform;
    private Vector3 RecoilRot;
    private float Recoil = 0;
    private float RecoilSpeed = 2;
    private bool wasFiring = false;
    private float lerpRecoil = 0;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        GameObject g = new GameObject("Recoil");
        m_Transform = g.transform;
        m_Transform.parent = transform.parent;
        m_Transform.localPosition = Vector3.zero;
        m_Transform.localEulerAngles = Vector3.zero;
        transform.parent = m_Transform;
#if !MFPSTPV
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
#endif

        RecoilRot = m_Transform.localEulerAngles;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        RecoilControl();
    }

    /// <summary>
    /// 
    /// </summary>
    void RecoilControl()
    {
        if (GunManager == null)
            return;

        if (GunManager.CurrentGun != null)
        {
            if (GunManager.CurrentGun.isFiring)
            {
                if (AutomaticallyComeBack)
                {
                    Quaternion q = Quaternion.Euler(new Vector3(-Recoil, 0, 0));
                    m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, q, Time.deltaTime * RecoilSpeed);
                }
                else
                {
                    lerpRecoil = Mathf.Lerp(lerpRecoil, Recoil, Time.deltaTime * RecoilSpeed);
                    fpController.mouseLook.SetVerticalOffset(-lerpRecoil);
                }
                wasFiring = true;
            }
            else
            {
                if (AutomaticallyComeBack)
                {
                    BackToOrigin();
                }
                else
                {
                    if (wasFiring)
                    {
                        Recoil = 0;
                        lerpRecoil = 0;
                        fpController.mouseLook.CombineVerticalOffset();
                        wasFiring = false;
                    }
                }
            }
        }
        else
        {
            BackToOrigin();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void BackToOrigin()
    {
        if (m_Transform == null) return;

        Quaternion q = Quaternion.Euler(RecoilRot);
        m_Transform.localRotation = Quaternion.Slerp(m_Transform.localRotation, q, Time.deltaTime * RecoilSpeed);
        Recoil = m_Transform.localEulerAngles.x;
    }

    /// <summary>
    /// Add recoil
    /// </summary>
    public void SetRecoil(float amount, float speed = 2)
    {
        Recoil += amount;
        Recoil = Mathf.Clamp(Recoil, 0, MaxRecoil);
        RecoilSpeed = speed;
    }

    private bl_GunManager GunManager => PlayerReferences.gunManager;
    private bl_FirstPersonController fpController => PlayerReferences.firstPersonController;

    private bl_PlayerReferences playerReferences;
    public bl_PlayerReferences PlayerReferences
    {
        get
        {
            if (playerReferences == null) playerReferences = transform.root.GetComponent<bl_PlayerReferences>();
            return playerReferences;
        }
    }
}