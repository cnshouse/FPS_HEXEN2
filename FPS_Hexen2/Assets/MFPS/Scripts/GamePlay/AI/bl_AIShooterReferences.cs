using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using MFPS.Internal.Interfaces;

public class bl_AIShooterReferences : MonoBehaviour, IMFPSPlayerReferences
{
    public bl_AIShooter aiShooter;
    public bl_AIShooterHealth shooterHealth;
    public bl_AIShooterNetwork shooterNetwork;
    public bl_AIShooterWeapon shooterWeapon;
    public bl_AIAnimation aiAnimation;
    public bl_NamePlateDrawer namePlateDrawer;
    public NavMeshAgent Agent;

    private Collider[] m_allColliders;
    public Collider[] AllColliders
    {
        get
        {
            if (m_allColliders == null || m_allColliders.Length <= 0)
            {
                m_allColliders = transform.GetComponentsInChildren<Collider>();
            }
            return m_allColliders;
        }
    }

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
}