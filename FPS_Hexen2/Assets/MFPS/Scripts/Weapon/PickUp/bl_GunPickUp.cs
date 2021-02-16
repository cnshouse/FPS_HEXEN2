using UnityEngine;
using System.Collections;
using Photon.Pun;

public class bl_GunPickUp : bl_MonoBehaviour
{
    [GunID] public int GunID = 0;
    public DetectMode m_DetectMode = DetectMode.Raycast;
    [HideInInspector]
    public bool PickupOnCollide = true;
    [Space(5)]
    public bool AutoDestroy = true;
    public float DestroyIn = 15f;

    public bl_EventHandler.UEvent onPickUp;

    private bool Into = false;
    private bl_GunPickUpManager pickupManager;

    //Cache info
    [System.Serializable]
    public class m_Info
    {
        public int Bullets = 0;
        public int Clips = 0;

        public int GetBullets
        {
            get
            {
                int b = Bullets;
                if (bl_GameData.Instance.AmmoType == AmmunitionType.Bullets)
                {
                    b = Bullets * Clips;
                }
                return b;
            }
        }
    }
    public m_Info Info;
    private bl_GunInfo CacheGun;
    private bool Equiped = false;
    private bool isFocus = false;
    private PhotonView localPlayerIn = null;

    public bool IsFocus { set { isFocus = value; } }
    private byte uniqueLocal = 0;

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;
        base.Awake();
        pickupManager = FindObjectOfType<bl_GunPickUpManager>();
        CacheGun = bl_GameData.Instance.GetWeapon(GunID);
        uniqueLocal = (byte)Random.Range(0, 9998);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Start()
    {
        if (AutoDestroy)
        {
            Destroy(gameObject, DestroyIn);
        }
        if (m_DetectMode == DetectMode.Trigger)
        {
            PickupOnCollide = false;
            yield return new WaitForSeconds(2f);
            PickupOnCollide = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Into)
        {
            bl_PickUpUI.Instance?.SetPickUp(false);
            if (localPlayerIn != null)
            {
                if (m_DetectMode == DetectMode.Raycast)
                {
                    bl_CameraRay cr = localPlayerIn.GetComponentInChildren<bl_CameraRay>();
                    if (cr != null)
                    {
                        cr.SetActiver(false, uniqueLocal);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="c"></param>
    void OnTriggerEnter(Collider c)
    {
        if (bl_GameManager.Instance.GameMatchState == MatchState.Waiting)
            return;
#if GR
        if(GetGameMode == GameMode.GR)
        {
            return;
        }
#endif
        // we only call Pickup() if "our" character collides with this PickupItem.
        // note: if you "position" remote characters by setting their translation, triggers won't be hit.

        PhotonView v = c.GetComponent<PhotonView>();
        if (PickupOnCollide && v != null && v.IsMine && c.CompareTag(bl_PlayerSettings.LocalTag))
        {
            Into = true;
            pickupManager.LastTrigger = this;
            localPlayerIn = v;
            bl_PlayerReferences playerReferences = v.GetComponent<bl_PlayerReferences>();
            if (CacheGun.Type == GunType.Knife)
            {
                if (playerReferences.gunManager.PlayerEquip.Exists(x => x != null && x.GunID == GunID))
                {
                    Equiped = true;
                }
                else
                {
                    Equiped = false;
                }
            }
            if (m_DetectMode == DetectMode.Raycast)
            {
                if (playerReferences.cameraRay != null)
                {
                    playerReferences.cameraRay.SetActiver(true, uniqueLocal);
                }
            }
            else if (m_DetectMode == DetectMode.Trigger)
            {
                bl_PickUpUI.Instance?.SetPickUp(true, GunID,this, Equiped);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnTriggerExit(Collider c)
    {
        if (c.transform.CompareTag(bl_PlayerSettings.LocalTag) && Into)
        {
            Into = false;
            bl_PickUpUI.Instance?.SetPickUp(false);
            if (localPlayerIn != null)
            {
                if (m_DetectMode == DetectMode.Raycast)
                {
                    bl_CameraRay cr = localPlayerIn.GetComponentInChildren<bl_CameraRay>();
                    if (cr != null)
                    {
                        cr.SetActiver(false, uniqueLocal);
                    }
                }
                localPlayerIn = null;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!Into || Equiped) return;

        if (m_DetectMode == DetectMode.Trigger)
        {
            if (PickUpInputPressed && pickupManager.LastTrigger == this)
            {
                Pickup();
            }
        }
        else if (m_DetectMode == DetectMode.Raycast)
        {
            if (!Into || !isFocus) return;

            if (PickUpInputPressed)
            {
                Pickup();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void Pickup()
    {
#if GR
        if (GetGameMode == GameMode.GR)
            return;
#endif

        pickupManager.SendPickUp(gameObject.name, GunID, Info);
        bl_PickUpUI.Instance?.SetPickUp(false);
        onPickUp?.Invoke();
    }

    public void FocusThis(bool focus)
    {
        isFocus = focus;
        bl_PickUpUI.Instance?.SetPickUp(focus, GunID, this , Equiped);
    }

    private SphereCollider SpheCollider;
    private void OnDrawGizmos()
    {
        if (SpheCollider != null)
        {
            Gizmos.color = new Color(0, 1, 0, 1f);
            bl_UtilityHelper.DrawWireArc(SpheCollider.bounds.center, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
        }
        else
        {
            SpheCollider = GetComponent<SphereCollider>();
        }
    }

    public bool PickUpInputPressed
    {
        get
        {
#if !INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.E);
#else
            return bl_Input.isButtonDown("Interact");
#endif
        }
    }

    [System.Serializable]
    public enum DetectMode
    {
        Raycast,
        Trigger,
    }
}