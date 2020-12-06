using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace MFPS.PlayerSelector
{
    public class bl_PSTeamUI : MonoBehaviour
    {
        public Text TeamNameText;
        public Text OperatorNameText;
        public Image PreviewImage;
        public GameObject FavUI;
        public GameObject FavButton;
        private Team UITeam = Team.All;

        public void SetUp(bl_PlayerSelectorInfo info, Team team)
        {
            UITeam = team;
            TeamNameText.text = team.GetTeamName().ToUpper();
            OperatorNameText.text = info.Name.ToUpper();
            PreviewImage.sprite = info.Preview;
            bool fav = (team == bl_PlayerSelectorData.GetFavoriteTeam());

            FavButton.SetActive(!fav);
            FavUI.SetActive(fav);
        }

        public void SetTeamFav(Team team)
        {
            bool fav = (team == UITeam);

            FavButton.SetActive(!fav);
            FavUI.SetActive(fav);
        }
    }
}