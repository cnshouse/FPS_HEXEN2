using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.PlayerSelector
{
    public class bl_PSOperatorUI : MonoBehaviour
    {

        public Image PreviewImage;
        public GameObject BlockedUI;
        public GameObject SelectedUI;

        private bl_PlayerSelectorLobby Manager;
        private bl_PlayerSelectorInfo cacheInfo;
        public bool isBlocked = false;
        public void SetUp(bl_PlayerSelectorInfo info, bl_PlayerSelectorLobby manager)
        {
            Manager = manager;
            cacheInfo = info;
            PreviewImage.sprite = info.Preview;
            SelectedUI.SetActive(info.isEquipedOne());
#if SHOP && ULSP
            if (info.Price > 0 && bl_DataBase.Instance != null)
            {
                int pID = bl_PlayerSelectorData.Instance.GetPlayerID(info.Name);
                bool unlock = bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(ShopItemType.PlayerSkin, pID);
                isBlocked = !unlock;
                BlockedUI.SetActive(!unlock);
            }
            else { BlockedUI.SetActive(false); }
#else
            BlockedUI.SetActive(false);
#endif
        }

        public void OnOver()
        {
            Manager.OnShowUpOp(cacheInfo);
        }

        public void OnExit()
        {
            Manager.ShowUpSelectedOne(cacheInfo.team);
        }

        public void SelectThis()
        {
            if (isBlocked) return;
            Manager.SelectOperator(cacheInfo);
            bl_PSOperatorUI[] all = transform.parent.GetComponentsInChildren<bl_PSOperatorUI>();
            foreach(bl_PSOperatorUI ui in all)
            {
                ui.SelectedUI.SetActive(false);
            }
            SelectedUI.SetActive(true);
        }
    }
}