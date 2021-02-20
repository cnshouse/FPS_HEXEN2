using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Clan
{
    public class bl_MemberListUI : MonoBehaviour
    {
        public Text NameText;
        public GameObject KickButton;
        public GameObject AscendButton;
        public GameObject DesendButton;
        private bl_ClanInfo.ClanMember MemberInfo;
        private bl_MyClan MyClan;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="mc"></param>
        public void Set(bl_ClanInfo.ClanMember info, bl_MyClan mc)
        {
            MyClan = mc;
            MemberInfo = info;
            NameText.text = MemberInfo.GetNameWithRole();
#if CLANS
            var localUser = bl_DataBase.Instance.LocalUser;
            ClanMemberRole pr = localUser.Clan.PlayerRole();
            if (pr != ClanMemberRole.Member)
            {
                int ph = (int)pr;
                int uh = (int)MemberInfo.Role;
                if (ph > uh)
                {
                    KickButton.SetActive(MemberInfo.ID != localUser.ID);//don't allow kick ourselves
                    DesendButton.SetActive(uh > 0);
                    if ((ph - uh) >= 2)//parent ranks can't accent others just one above him.
                    {
                        AscendButton.SetActive(true);
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Kick()
        {
            bl_ClanManager.Instance.AskComfirmationFor(() =>
            {
                MyClan.KickMember(MemberInfo, false);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Ascend()
        {
            bl_ClanManager.Instance.AskComfirmationFor(() =>
            {
                MyClan.ChangeMemberRole(MemberInfo, true);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public void Desend()
        {
            bl_ClanManager.Instance.AskComfirmationFor(() =>
            {
                MyClan.ChangeMemberRole(MemberInfo, false);
            });
        }
    }
}