using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI.Layout
{
    public class bl_RoundFinishUI : MonoBehaviour
    {
        public GameObject content;
        [SerializeField] private Text FinalUIText = null;
        [SerializeField] private Text FinalCountText = null;
        [SerializeField] private Text FinalWinnerText = null;

        /// <summary>
        /// Show the final round UI
        /// </summary>
        public void Show(string winner)
        {
            content.SetActive(true);
#if LOCALIZATION
            FinalUIText.text = (bl_MatchTimeManager.Instance.roundStyle == RoundStyle.OneMacht) ? bl_Localization.GetLocalizedText(38) : bl_Localization.GetLocalizedText(32);
            FinalWinnerText.text = string.Format("{0} {1}", winner, bl_Localization.GetLocalizedText(33)).ToUpper();
#else
        FinalUIText.text = (bl_MatchTimeManager.Instance.roundStyle == RoundStyle.OneMacht) ? bl_GameTexts.FinalOneMatch : bl_GameTexts.FinalRounds;
        FinalWinnerText.text = string.Format("{0} {1}", winner, bl_GameTexts.FinalWinner).ToUpper();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Hide()
        {
            content.SetActive(false);
        }

        public void SetCountDown(int count) { count = Mathf.Clamp(count, 0, int.MaxValue); FinalCountText.text = count.ToString(); }
    }
}