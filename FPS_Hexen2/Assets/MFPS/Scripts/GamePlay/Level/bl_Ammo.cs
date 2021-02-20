using UnityEngine;
using System.Collections;

namespace MFPS.Runtime.Level
{
    public class bl_Ammo : bl_NetworkItem
    {
        [LovattoToogle] public bool isGlobal = true;
        [LovattoToogle] public bool autoRespawn = false;
        [GunID] public int ForGun = 0;

        public int Bullets = 30;
        public int Projectiles = 2;
        public AudioClip PickSound;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m_other"></param>
        void OnTriggerEnter(Collider m_other)
        {
            if (!m_other.transform.CompareTag(bl_PlayerSettings.LocalTag)) return;

            if (PickSound) AudioSource.PlayClipAtPoint(PickSound, transform.position, 1.0f);

            int gunID = isGlobal ? -1 : ForGun;
            bl_EventHandler.OnAmmo(Bullets, Projectiles, gunID);

            //should this ammo reaper after certain time?
            if (autoRespawn)
            {
                bl_ItemManager.Instance.WaitForRespawn(this);
            }
            else
            {
                DestroySync();
            }
        }
    }
}