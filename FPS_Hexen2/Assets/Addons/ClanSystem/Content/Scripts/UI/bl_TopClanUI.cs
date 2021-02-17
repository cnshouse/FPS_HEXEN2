using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.Clan
{
    public class bl_TopClanUI : MonoBehaviour
    {
        public Text NameText;
        public Text MembersText;
        public Text ScoreText;

        public void Set(string Name, string Members, string Score)
        {
            NameText.text = Name;
            MembersText.text = string.Format("<b>{0}</b> / 20 MEMBERS", Members);
            ScoreText.text = Score;
        }

        public void OnClick()
        {
            bl_ClanSearch.Instance.DoSearch(NameText.text, true);
        }
    }
}