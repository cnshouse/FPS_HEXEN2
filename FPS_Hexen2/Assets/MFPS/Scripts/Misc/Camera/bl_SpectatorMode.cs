using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bl_SpectatorMode : bl_MonoBehaviour
{
    public bool isActive { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public void SetSpectatorMode(bool active)
    {
        gameObject.SetActive(true);
        bl_UtilityHelper.LockCursor(active);
        if (active)
        {
            bl_RoomCamera.Instance.cameraControl = true;
            bl_RoomCamera.Instance.SetActive(active);
            bl_UIReferences.Instance.ShowMenu(false);
        }
        else
        {
            bl_RoomCamera.Instance.BackToOriginal();
            if (!bl_GameManager.Joined)
            {
                bl_UIReferences.Instance.ShowMenu(true);
                bl_UIReferences.Instance.SetUpJoinButtons(true);
            }
        }
        isActive = active;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        ListenInput();
    }

    /// <summary>
    /// 
    /// </summary>
    void ListenInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetSpectatorMode(false);
        }
    }

    public void SetActive(bool active) => gameObject.SetActive(active);

    public static bool IsActive()
    {
        if (bl_UIReferences.Instance == null || bl_UIReferences.Instance.spectatorMode == null) return false;
        return bl_UIReferences.Instance.spectatorMode.isActive;
    }
}