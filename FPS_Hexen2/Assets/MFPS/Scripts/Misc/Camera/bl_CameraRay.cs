using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class bl_CameraRay : bl_MonoBehaviour
{
    public int CheckFrameRate = 5;
    public CastMethod castMethod = CastMethod.Box;
    [Range(0.1f, 10)] public float DistanceCheck = 2;
    public LayerMask DetectLayers;
    public bool Checking { get; set; }

    #region Private members
    private int currentFrame = 0;
    private RaycastHit RayHit;
    private bl_GunPickUp gunPickup = null;
    private List<byte> activers = new List<byte>();
    private Dictionary<string, Action<bool>> triggers = new Dictionary<string, Action<bool>>();
    bool hasDectected = false;
    public float ExtraRayDistance { get; set; } = 0;
    private byte increaseCounter = 0;
    public Vector3 boxDimesion = new Vector3(0.15f, 0.15f, 0.1f);
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!Checking) { currentFrame = 0; return; }

        if(currentFrame == 0)
        {
            Fire();
        }
        currentFrame = (currentFrame + 1) % CheckFrameRate;
    }

    /// <summary>
    /// 
    /// </summary>
    void Fire()
    {
        bool detected = false;
        if (castMethod == CastMethod.Box || castMethod == CastMethod.Both)
        {
            detected = Physics.BoxCast(CachedTransform.position, boxDimesion, CachedTransform.forward,
                out RayHit, CachedTransform.rotation, RayDistance, DetectLayers, QueryTriggerInteraction.Ignore);
        }
        if((castMethod == CastMethod.Ray || castMethod == CastMethod.Both) && !detected)
        {
            Ray r = new Ray(CachedTransform.position, CachedTransform.forward);
            detected = Physics.Raycast(r, out RayHit, RayDistance, DetectLayers, QueryTriggerInteraction.Ignore);
        }

        if (detected)
        {
            hasDectected = true;
            OnHit();
            //check in each register trigger
            if (triggers.Count > 0)
            {
                foreach (var item in triggers.Keys)
                {
                    //if the object that is in front have the same name that the register trigger -> call their callback
                    if (RayHit.transform.name == item)
                    {
                        triggers[item].Invoke(true);
                    }
                }
            }
        }
        else
        {
            if (gunPickup != null) { gunPickup.FocusThis(false); gunPickup = null; }
            if (triggers.Count > 0 && hasDectected)
            {
                foreach (var item in triggers.Values)
                {
                    item.Invoke(false);
                }
            }
            hasDectected = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnHit()
    {
        bl_GunPickUp gp = RayHit.transform.GetComponent<bl_GunPickUp>();
        if (gp != null)
        {
            if (gunPickup != null && gunPickup != gp)
            {
                gunPickup.FocusThis(false);
            }
            gunPickup = gp;
            gunPickup.FocusThis(true);
        }
        else
        {
            if (gunPickup != null) { gunPickup.FocusThis(false); gunPickup = null; }
        }
    }

    /// <summary>
    /// If you wanna detect when an object is in front of the local player view
    /// register a callback in this function
    /// </summary>
    public byte AddTrigger(string objectName, Action<bool> callback, byte id)
    {
        if (triggers.ContainsKey(objectName)) { return 0; }

        triggers.Add(objectName, callback);
        return SetActiver(true, id);
    }


    /// <summary>
    /// Make sure of remove the trigger when you don't need to detect it anymore.
    /// </summary>
    public void RemoveTrigger(string objectName, byte id)
    {
        if (!triggers.ContainsKey(objectName)) return;
        triggers.Remove(objectName);
        SetActiver(false, id);
    }

    /// <summary>
    /// 
    /// </summary>
    public byte SetActiver(bool add, byte id)
    {
        if (add)
        {
            if (!activers.Contains(id))
            {
                activers.Add(id);
            }
            else
            {
                increaseCounter++;
                id = increaseCounter;
                activers.Add(id);
            }
            Checking = true;
        }
        else
        {
            if (activers.Contains(id))
            {
                activers.Remove(id);
            }
            if (activers.Count <= 0)
            {
                Checking = false;
            }
        }
        return id;
    }

    private float RayDistance => DistanceCheck + ExtraRayDistance;

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, transform.forward * RayDistance);
    }

    [System.Serializable]
    public enum CastMethod
    {
        Ray,
        Box,
        Both,
    }
}