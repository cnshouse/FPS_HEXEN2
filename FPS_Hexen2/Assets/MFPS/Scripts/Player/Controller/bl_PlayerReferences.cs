using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using MFPS.Internal.Interfaces;

public class bl_PlayerReferences : MonoBehaviour, IMFPSPlayerReferences
{
    public bl_PlayerNetwork playerNetwork;
    public bl_FirstPersonController firstPersonController;
    public bl_PlayerHealthManager playerHealthManager;
    public bl_PlayerSettings playerSettings;
    public bl_GunManager gunManager;
    public bl_PlayerAnimations playerAnimations;
    public bl_BodyPartManager bodyPartManager;
    public bl_Recoil recoil;
    public bl_CameraShaker cameraShaker;
    public bl_CameraRay cameraRay;
    public bl_WeaponBob weaponBob;
    public bl_WeaponSway weaponSway;
    public CharacterController characterController;
    public PhotonView photonView;
    public Camera playerCamera;
    public Camera weaponCamera;
    public ParentConstraint leftArmTarget;

    #region Getters
    public GameObject LocalPlayerObjects => playerSettings.LocalObjects;
    public GameObject RemotePlayerObjects => playerSettings.RemoteObjects;
    public Animator PlayerAnimator => playerAnimations.m_animator;

    private Transform m_playerCameraTransform;
    public Transform PlayerCameraTransform
    {
        get
        {
            if (m_playerCameraTransform == null) m_playerCameraTransform = playerCamera.transform;
            return m_playerCameraTransform;
        }
    }

    public static bl_PlayerReferences LocalPlayer
    {
        get
        {
            return bl_GameManager.Instance.LocalPlayerReferences;
        }
    }

    private bl_PlayerIK m_playerIK= null;
    public bl_PlayerIK playerIK
    {
        get
        {
            if (playerAnimations == null) return null;
            if (m_playerIK == null) m_playerIK = playerAnimations.GetComponentInChildren<bl_PlayerIK>();
            return m_playerIK;
        }
    }

    private float m_defaultCameraFOV = -1;
    public float DefaultCameraFOV
    {
        get
        {
            if (m_defaultCameraFOV == -1) m_defaultCameraFOV = playerCamera.fieldOfView;
            return m_defaultCameraFOV;
        }
    }


#if MFPS_VEHICLE
    private bl_PlayerVehicle m_playerVehicle = null;
    public bl_PlayerVehicle PlayerVehicle
    {
        get
        {
            if (m_playerVehicle == null) m_playerVehicle = GetComponent<bl_PlayerVehicle>();
            return m_playerVehicle;
        }
    }
#endif

    private Collider[] m_allColliders;
    public Collider[] AllColliders
    {
        get
        {
            if(m_allColliders == null || m_allColliders.Length <= 0)
            {
                m_allColliders = transform.GetComponentsInChildren<Collider>();
            }
            return m_allColliders;
        }
    }

    public int ViewID => photonView.ViewID;
    public int ActorNumber => photonView.Owner.ActorNumber;
    #endregion

    #region Functions
    /// <summary>
    /// 
    /// </summary>
    public void IgnoreColliders(Collider[] list, bool ignore)
    {
        for (int e = 0; e < list.Length; e++)
        {
            for (int i = 0; i < AllColliders.Length; i++)
            {
                if (AllColliders[i] != null)
                {
                    Physics.IgnoreCollision(AllColliders[i], list[e], ignore);
                }
            }
        }
    }
    #endregion
}