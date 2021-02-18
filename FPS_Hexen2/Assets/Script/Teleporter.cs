using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Teleporter : bl_MonoBehaviour
{
    public DetectMode m_DetectMode = DetectMode.Raycast;

    [HideInInspector]
    public bool TeleportOnCollide = true;
    [HideInInspector]
    public bool SentTeleport = false;

    private bool Into = false;

    private PhotonView localPlayerIn = null;

    private bl_UIReferences UIReferences;
    private byte uniqueLocal = 0;

    public bool AutoDestroy = false;
    public float DestroyIn = 300f;

    private bool isFocus = false;
    public bool IsFocus { set { isFocus = value; } }

    public SphereCollider SpheCollider;

    public GameObject TelePortDestination; 

    protected override void Awake()
    {
        if (!PhotonNetwork.IsConnected) return;
        base.Awake();
        UIReferences = bl_UIReferences.Instance;
        uniqueLocal = (byte)Random.Range(0, 9998);
    }

    IEnumerator Start()
    {
        if (AutoDestroy)
        {
            Destroy(gameObject, DestroyIn);
        }
        TeleportOnCollide = false;
        yield return new WaitForSeconds(2f);
        TeleportOnCollide = true;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (Into)
        {
            if (UIReferences != null)
            {
                //UIReferences.SetPickUp(false);
            }
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
        if (TeleportOnCollide && v != null && v.IsMine && c.CompareTag(bl_PlayerSettings.LocalTag))
        {
            //Debug.Log("In Range of the Teleporter!!!");
            Into = true;
            localPlayerIn = v;
            if (m_DetectMode == DetectMode.Raycast)
            {
                bl_CameraRay cr = v.GetComponentInChildren<bl_CameraRay>();
                if (cr != null)
                {
                    cr.SetActiver(true, uniqueLocal);
                }
            }
            else if (m_DetectMode == DetectMode.Trigger)
            {
                //on Trigger teleport...
                //Teleport();
                bl_PickUpUI.Instance?.Show("Press E to use Teleporter", null, "E");
                //UIReferences.ShowTeleportPromt(true);
            }
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.transform.CompareTag(bl_PlayerSettings.LocalTag) && Into)
        {
            Into = false;
            bl_PickUpUI.Instance?.Hide();
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

    public override void OnUpdate()
    {
        if (m_DetectMode == DetectMode.Trigger)
        {
            if (!Into) return;
#if !INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.E))
            {
                Teleport();
            }
#else
            if (bl_Input.isButtonDown("Interact"))
            {
                Teleport();
            }
#endif
        }
        else if (m_DetectMode == DetectMode.Raycast)
        {
            if (!Into || !isFocus) return;
#if !INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.E))
            {
                Teleport();
            }
#else
            if (bl_Input.isButtonDown("Interact"))
            {
                Teleport();
            }
#endif
        }
    }

    public void Teleport()
    {
        if (SentTeleport)
            return;
#if GR
        if (GetGameMode == GameMode.GR)
        {
            return;
        }
#endif

        SentTeleport = true;

        //Debug.Log("Attempt to teleport the player!!!!");
        //Add the Character transform movement here...
        localPlayerIn.GetComponent<bl_FirstPersonController>().OnTeleport(TelePortDestination.transform.position, TelePortDestination.transform.rotation);
        SentTeleport = false;
        bl_PickUpUI.Instance?.Hide();
        //UIReferences.SetPickUp(false);
    }

    public void FocusThis(bool focus)
    {
        isFocus = focus;
        //UIReferences.SetPickUp(focus, GunID, this, Equiped);
    }

    private void OnDrawGizmos()
    {
        if (SpheCollider != null)
        {
            Gizmos.color = new Color(1, 0, 0, 1f);
            bl_UtilityHelper.DrawWireArc(SpheCollider.bounds.center, SpheCollider.radius * transform.lossyScale.x, 360, 20, Quaternion.identity);
        }
        else
        {
            SpheCollider = GetComponent<SphereCollider>();
        }
    }

    [System.Serializable]
    public enum DetectMode
    {
        Raycast,
        Trigger,
    }
}
