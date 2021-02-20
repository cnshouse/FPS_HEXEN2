using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace MFPS.Runtime.AI
{
    public class bl_AIWeapon : MonoBehaviour
    {
        [Header("Info")]
        [GunID] public int GunID;
        [Range(1, 60)] public int Bullets = 30;
        [Range(1, 6)] public int bulletsPerShot = 1;
        public int maxFollowingShots = 5;
        public string BulletName = "bullet";
        [Header("References")]
        public Transform FirePoint;
        public ParticleSystem MuzzleFlash;
        public AudioClip fireSound;
        public AudioClip[] reloadSounds;

        // public WeaponAudioBank weaponAudio;
        // public Transform leftArmPosition;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize(bl_AIShooterWeapon shooterWeapon)
        {
            /* if (shooterWeapon.leftArmConstrain != null)
             {
                 if (shooterWeapon.leftArmConstrain.sourceCount > 0)
                     shooterWeapon.leftArmConstrain.RemoveSource(0);
                 if (leftArmPosition != null)
                 {
                     shooterWeapon.leftArmConstrain.transform.position = leftArmPosition.position;
                     shooterWeapon.leftArmConstrain.transform.rotation = leftArmPosition.rotation;
                     var lap = new ConstraintSource()
                     {
                         sourceTransform = leftArmPosition,
                         weight = 1
                     };
                     shooterWeapon.leftArmConstrain.AddSource(lap);
                 }
             }*/
        }

        private bl_GunInfo m_info;
        public bl_GunInfo Info
        {
            get
            {
                if (m_info == null)
                {
                    m_info = bl_GameData.Instance.GetWeapon(GunID); ;
                }
                return m_info;
            }
        }
    }
}