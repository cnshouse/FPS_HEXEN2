using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

namespace MFPS.GameModes.FreeForAll
{
    public class bl_FreeForAllUI : MonoBehaviour
    {
        public GameObject Content;
        public Text ScoreText;

        public void SetScores(Player bestPlayer)
        {
            string scoreText = string.Format(bl_GameTexts.PlayerStart, bestPlayer.NickName);
            ScoreText.text = scoreText;
        }

        public void ShowUp()
        {
            if (!bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard)) return;

            Content.SetActive(true);
        }

        public void Hide()
        {
            Content.SetActive(false);
        }

        private static bl_FreeForAllUI _instance;
        public static bl_FreeForAllUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<bl_FreeForAllUI>();
                }
                return _instance;
            }
        }
    }
}