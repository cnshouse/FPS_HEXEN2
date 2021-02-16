using UnityEngine;
using UnityEngine.UI;

namespace MFPS.ULogin
{
    public class bl_RankingUIPro : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Color AdminColor;
        [SerializeField] private Color ModColor;
        [Header("References")]
        [SerializeField] private Text RankText;
        [SerializeField] private Text PlayerNameText;
        [SerializeField] private Text ScoreText;
        [SerializeField] private Text KillsText;
        [SerializeField] private Text DeathsText;

        public void SetInfo(LoginUserInfo info, int rank)
        {
            RankText.text = rank.ToString();
            PlayerNameText.text = info.NickName;
            ScoreText.text = info.Score.ToString();
            KillsText.text = info.Kills.ToString();
            DeathsText.text = info.Deaths.ToString();
            CheckStatus(info.UserStatus);
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckStatus(LoginUserInfo.Status status)
        {
            if (status == LoginUserInfo.Status.Admin)
            {
                PlayerNameText.text += " [Admin]";
                PlayerNameText.color = AdminColor;
            }
            else if (status == LoginUserInfo.Status.Moderator)
            {
                PlayerNameText.text += " [Moderator]";
                PlayerNameText.color = ModColor;
            }
        }
    }
}