using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MFPS.PlayerSelector
{
    public class bl_PlayerSelectorUI : MonoBehaviour
    {
        [SerializeField] private Image PlayerPreview;
        [SerializeField] private Text PlayerNameText;
        [SerializeField] private Image HealthText;
        [SerializeField] private Image SpeedText;
        [SerializeField] private Image RegenerationText;
        [SerializeField] private Image NoiseText;
        public GameObject LockedUI;
        public AnimationCurve VerticalCurve;

        private bl_PlayerSelector Selector;
        private bl_PlayerSelectorInfo Info;
        private Animator Anim;

        public void Set(bl_PlayerSelectorInfo info, bl_PlayerSelector script)
        {
            Info = info;
            PlayerPreview.sprite = info.Preview;
            PlayerNameText.text = string.Format("<b>NAME:</b> {0}", info.Name.ToUpper());
            Selector = script;
            if (info.Prefab != null)
            {
                bl_PlayerHealthManager pdm = info.Prefab.GetComponent<bl_PlayerHealthManager>();
                bl_FirstPersonController fpc = info.Prefab.GetComponent<bl_FirstPersonController>();

                HealthText.fillAmount = pdm.health / 125;
                SpeedText.fillAmount = fpc.WalkSpeed / 5;
                RegenerationText.fillAmount = pdm.RegenerationSpeed / 5;
                NoiseText.fillAmount = fpc.FootStepVolume / 1;
            }
#if SHOP && ULSP
            if (info.Price > 0 && bl_DataBase.Instance != null)
            {
                int pID = bl_PlayerSelectorData.Instance.GetPlayerID(info.Name);
                bool unlock = bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(ShopItemType.PlayerSkin, pID);
                LockedUI.SetActive(!unlock);
            }
            else { LockedUI.SetActive(false); }
#else
            LockedUI.SetActive(false);
#endif
        }

        public void Select()
        {
            if (Anim != null) return;

            Transform parent = transform.parent;
            parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
            parent.GetComponent<ContentSizeFitter>().enabled = false;
            Selector.DeleteAllBut(gameObject);
            StartCoroutine(ShowUp());
        }

        IEnumerator ShowUp()
        {
            Anim = GetComponent<Animator>();
            Anim.SetTrigger("play");
            float time = 2;
            float d = 0;
            Vector3 origin = transform.position;
            while (d < 1)
            {
                d += Time.deltaTime / (time * 0.5f);
                Vector3 v = Selector.CenterReference.position;
                v.y = v.y + (VerticalCurve.Evaluate(d) * 275);
                transform.position = Vector3.Lerp(origin, v, d);
                yield return null;
            }
            yield return new WaitForSeconds(time * 0.5f);
            Selector.SelectPlayer(Info);
            Destroy(gameObject);
        }
    }
}