using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace MFPS.ClassCustomization
{
    public class bl_ClassInfoUI : MonoBehaviour
    {
        public Image Icon;
        public Text NameText;
        public Text LockedText;
        public Button button;
        private int ID;
#pragma warning disable 414
        [SerializeField] private GameObject LevelLock = null;
        [HideInInspector] public int ClassId = 0;
#pragma warning restore 414
        [HideInInspector] public int ListID = 0;
        private CanvasGroup Alpha;

        /// <summary>
        /// 
        /// </summary>
        public void GetInfo(WeaponItemData info, int slot, int lID)
        {
            Icon.sprite = info.Info.GunIcon;
            NameText.text = info.Info.Name.ToUpper();
            ID = info.GunID;
            ClassId = slot;
            ListID = lID;
            button.interactable = !bl_ClassManager.Instance.isEquiped(info.GunID, bl_ClassManager.Instance.m_Class);
#pragma warning disable 219
            int lockedStatus = 0;

#if SHOP && ULSP
            if (info.Info.Price > 0 && bl_DataBase.Instance != null)
            {
                int gunID = bl_GameData.Instance.GetWeaponID(info.Info.Name);
                LockedText.text = "PRICE: $" + info.Info.Price;
                if (bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(ShopItemType.Weapon, gunID))
                {
                    lockedStatus = 2;
                    LevelLock.SetActive(false);
                }
                else
                {
                    lockedStatus = 1;
                    LevelLock.SetActive(true);                 
                    GetComponent<Button>().interactable = false;
                }
            }
            else
            {
                LevelLock.SetActive(false);
            }
#else
            lockedStatus = 0;
#endif
#if LM
            if (bl_GameData.Instance.LockWeaponsByLevel && lockedStatus == 0)
            {
                int al = bl_LevelManager.Instance.GetLevelID();
                bool UnLock = (al >= info.Info.LevelUnlock);
                LevelLock.SetActive(!UnLock);
                if (!UnLock)
                {
                    LockedText.text = "REQUIRE LEVEL: " + info.Info.LevelUnlock;
                    GetComponent<Button>().interactable = false;
                }
            }
#endif
#pragma warning restore 219
            StartCoroutine(Fade(lID * 0.04f));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeSlot()
        {
            bl_ClassCustomize c = FindObjectOfType<bl_ClassCustomize>();
            c.ChangeSlotClass(ID, ClassId, ListID);
        }

        IEnumerator Fade(float wait)
        {
            Alpha = GetComponent<CanvasGroup>();
            Alpha.alpha = 0;
            yield return new WaitForSeconds(wait);
            float d = 0;
            while (d < 1)
            {
                d += Time.deltaTime * 2;
                Alpha.alpha = d;
                yield return null;
            }
        }
    }
}