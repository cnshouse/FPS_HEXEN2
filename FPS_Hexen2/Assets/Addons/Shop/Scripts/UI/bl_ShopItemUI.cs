using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

namespace MFPS.Shop
{
    public class bl_ShopItemUI : MonoBehaviour, IPointerEnterHandler
    {
        public Text NameText;
        public Text typeText;
        public Text PriceText;
        public Image[] Icons;
        public RectTransform BuyButton;
        public GameObject OwnedUI;
        public GameObject BuyUI;
        public MonoBehaviour[] oneTimeUsed;

        private bl_ShopItemData Info;
        #if SHOP
        private int ID = 0;
        #endif
        public ShopItemType TypeID;
        private bool isOwned = false;

        public void Set(bl_ShopItemData _info)
        {
            foreach (Image i in Icons) { i.gameObject.SetActive(false); }
            Info = _info;
#if SHOP
            ID = _info.ID;
#endif
            TypeID = _info.Type;
            NameText.text = Info.Name.ToUpper();
            string typeName = _info.Type.ToString();
            typeName = string.Concat(typeName.Select(x => System.Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
            typeText.text = typeName.ToUpper();
            LayoutRebuilder.ForceRebuildLayoutImmediate(NameText.transform.parent.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(typeText.transform.parent.GetComponent<RectTransform>());
            //that's kinda dirty but it works :)
            foreach (MonoBehaviour b in oneTimeUsed) { Destroy(b); }
            if (Info.Price <= 0)
            {
                BuyUI.SetActive(false);
                isOwned = true;
                GetComponent<Selectable>().interactable = false;
                OwnedUI.SetActive(true);
            }
            else
            {
                //if this item has been purchase
#if ULSP && SHOP
                if (bl_DataBase.Instance != null && bl_DataBase.Instance.LocalUser.ShopData != null && bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(TypeID, ID))
                {
                    BuyUI.SetActive(false);
                    isOwned = true;
                    GetComponent<Selectable>().interactable = false;
                    OwnedUI.SetActive(true);
                }
                else
                {
                    PriceText.text = string.Format("${0}", Info.Price);
                    BuyUI.SetActive(true);
                    isOwned = false;
                    BuyButton.gameObject.SetActive(true);
                    OwnedUI.SetActive(false);
                }
#endif
            }
            if (_info.Type == ShopItemType.Weapon)
            {
                SetIcon(Info.GunInfo.GunIcon, 0);
            }
            else if (_info.Type == ShopItemType.PlayerSkin)
            {
#if PSELECTOR
                SetIcon(Info.PlayerSkinInfo.Preview, 1);
#endif
            }
            else if (Info.Type == ShopItemType.WeaponCamo)
            {
#if CUSTOMIZER
                SetIcon(Info.camoInfo.spritePreview(), 2);
#endif
            }
        }

        void SetIcon(Sprite icon, int id)
        {
            Icons[id].gameObject.SetActive(true);
            Icons[id].sprite = icon;
        }

        public void OnBuy()
        {
#if ULSP && SHOP
            if (bl_DataBase.Instance == null) return;

            if (bl_DataBase.Instance.isGuest || !bl_DataBase.Instance.isLogged)
            {
                Debug.LogWarning("You has to be login in order to make a purchase.");
                return;
            }
            else
            {
                if (bl_DataBase.Instance.LocalUser.Coins >= Info.Price)
                {
                    bl_ShopManager.Instance.PreviewItem(Info, BuyButton.position);
                }
                else
                {
                    bl_ShopManager.Instance.NoCoinsWindow.SetActive(true);
                }
            }
#else
                        Debug.LogWarning("You need have ULogin Pro enabled to use this addon");
            return;
#endif
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            bl_ShopManager.Instance.Preview(Info, isOwned);
        }
    }
}