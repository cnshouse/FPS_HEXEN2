using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

namespace MFPS.GameModes.Demolition
{
    public class bl_DemolitionUI : MonoBehaviour
    {
        public Text inZoneText;
        public GameObject ProgressUI;
        public GameObject pickUpIndicationUI;
        public Image BarImg;
        public GameObject WaitingForRoundUI;
        public GameObject BombCarrierUI;
        public GameObject PlantButtonForMobile;
        public GameObject DefuseButtonForMobile;
        public Text RoundStateText;
        public Text Team1ScoreText;
        public Text Team2ScoreText;
        public GameObject onPlantUI;
        [SerializeField] private GameObject ScoreUI = null;
        [SerializeField] private GameObject PlayerHolderUI = null;
        [SerializeField] private Transform Team1Panel = null;
        [SerializeField] private Transform Team2Panel = null;
        [SerializeField] private Sprite Team1FaceSprite = null;
        [SerializeField] private Sprite Team2FaceSprite = null;

        private List<GameObject> cacheHolders = new List<GameObject>();

        private void Start()
        {
            RoundStateText.gameObject.SetActive(false);
            inZoneText.gameObject.SetActive(false);
            ProgressUI.SetActive(false);
            BombCarrierUI.SetActive(false);
            PlantButtonForMobile.SetActive(false);
            DefuseButtonForMobile.SetActive(false);
            if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
            {
                int t1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
                Team1ScoreText.text = t1.ToString();
                int t2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
                Team2ScoreText.text = t2.ToString();
                if (bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard))
                    ScoreUI.SetActive(true);
            }
            else
            {
                ScoreUI.SetActive(false);
            }
        }

        public void UpdateProgress(float time, float normalizedTime)
        {
            BarImg.fillAmount = normalizedTime;
        }

        public void ShowPlantGuide(bool show)
        {
            if (bl_UtilityHelper.isMobile)
            {
                PlantButtonForMobile.SetActive(show);
            }
            else
            {
                inZoneText.GetComponent<Text>().text = string.Format("PRESS <b>[{0}]</b> TO PLANT THE BOMB.", bl_Demolition.Instance.plantKey.ToString().ToUpper());
                inZoneText.gameObject.SetActive(show);
            }
        }

        public void ShowDefuseGuide(bool show)
        {
            if (bl_UtilityHelper.isMobile)
            {
                DefuseButtonForMobile.SetActive(show);
            }
            else
            {
                inZoneText.text = string.Format("PRESS <b>[{0}]</b> TO DEFUSE THE BOMB.", bl_Demolition.Instance.plantKey.ToString().ToUpper());
                inZoneText.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnRoundStart()
        {
            if (bl_Demolition.Instance.bombAssignMethod == bl_Demolition.BombAssignMethod.PlayerPicked)
            {
                if (PhotonNetwork.LocalPlayer.GetPlayerTeam() == bl_Demolition.Instance.terroristsTeam)
                {
                    pickUpIndicationUI.SetActive(true);
                }
            }
            onPlantUI.SetActive(false);
            inZoneText.gameObject.SetActive(false);
            PlantButtonForMobile.SetActive(false);
            DefuseButtonForMobile.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
            onPlantUI.SetActive(false);
            BombCarrierUI.SetActive(false);
            ProgressUI.SetActive(false);
            inZoneText.gameObject.SetActive(false);
            PlantButtonForMobile.SetActive(false);
            DefuseButtonForMobile.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstancePlayers()
        {
            CleanCache();
            Player[] list = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team1);
            for (int i = 0; i < list.Length; i++)
            {
                GameObject g = Instantiate(PlayerHolderUI) as GameObject;
                g.name = list[i].NickName;
                g.GetComponent<Image>().sprite = Team1FaceSprite;
                g.transform.SetParent(Team1Panel, false);
                cacheHolders.Add(g);
            }
            list = PhotonNetwork.PlayerList.GetPlayersInTeam(Team.Team2);
            for (int i = 0; i < list.Length; i++)
            {
                GameObject g = Instantiate(PlayerHolderUI) as GameObject;
                g.name = list[i].NickName;
                g.GetComponent<Image>().sprite = Team2FaceSprite;
                g.transform.SetParent(Team2Panel, false);
                cacheHolders.Add(g);
            }
            if (bl_UIReferences.Instance.UIMask.IsEnumFlagPresent(RoomUILayers.TopScoreBoard))
                ScoreUI.SetActive(true);
        }

        public void OnPlayerDeath(Player player, Team t)
        {
            for (int i = 0; i < cacheHolders.Count; i++)
            {
                if (cacheHolders[i].name == player.NickName)
                {
                    cacheHolders[i].GetComponent<Image>().color = Color.red;
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CleanCache()
        {
            foreach (GameObject g in cacheHolders) { Destroy(g); }
            cacheHolders.Clear();
        }


        private static bl_DemolitionUI _instance;
        public static bl_DemolitionUI Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_DemolitionUI>(); }
                return _instance;
            }
        }
    }
}