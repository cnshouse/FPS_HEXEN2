using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Clan
{
    public class bl_ClanJoinRequestUI : MonoBehaviour
    {
        public Text NameText;
        public Text ScoreText;
        public GameObject Buttons;

        private int userID = 0;
        private bl_ClanInfo clanInfo;

        public void Set(string Name, int ID)
        {
            userID = ID;
            NameText.text = Name;
#if CLANS
        Buttons.SetActive((int)bl_DataBase.Instance.LocalUser.Clan.PlayerRole() > 0);
#endif
        }

        public void SetClanRequest(string Name, string score, string _clanID)
        {
            clanInfo = new bl_ClanInfo();
            clanInfo.ID = int.Parse(_clanID);
            clanInfo.Name = Name;
            clanInfo.Score = int.Parse(score);
            NameText.text = Name;
            if (ScoreText != null)
                ScoreText.text = string.Format("SCORE: <b>{0}</b>", score);
        }

        public void AcceptClanInvitation()
        {
            bl_ClanManager.Instance.AcceptClanInvitation(clanInfo, true);
        }

        public void DenyClanInvitation()
        {
            bl_ClanManager.Instance.DenyClanInvitation(clanInfo.ID, OnFetch);
        }

        public void Accept()
        {
            bl_ClanManager.Instance.AcceptJoinRequest(userID, OnFetch);
        }

        public void Deny()
        {
            bl_ClanManager.Instance.DenyUserJoinRequest(userID, OnFetch);
        }

        void OnFetch()
        {
            Destroy(gameObject);
        }
    }
}