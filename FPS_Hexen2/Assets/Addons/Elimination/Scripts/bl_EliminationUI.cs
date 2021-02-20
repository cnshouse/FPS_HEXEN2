using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace MFPS.GameModes.Elimination
{
    public class bl_EliminationUI : MonoBehaviour
    {
        public GameObject WaitingForRoundUI;
        public Text RoundStateText;
        public Text Team1ScoreText;
        public Text Team2ScoreText;
        [SerializeField] private GameObject ScoreUI;
        [SerializeField] private GameObject PlayerHolderUI;
        [SerializeField] private Transform Team1Panel;
        [SerializeField] private Transform Team2Panel;
        [SerializeField] private Sprite Team1FaceSprite;
        [SerializeField] private Sprite Team2FaceSprite;

        private List<GameObject> cacheHolders = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            RoundStateText.gameObject.SetActive(false);
            if (bl_GameManager.Instance.GameMatchState == MatchState.Playing)
            {
                int t1 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team1);
                Team1ScoreText.text = t1.ToString();
                int t2 = PhotonNetwork.CurrentRoom.GetRoomScore(Team.Team2);
                Team2ScoreText.text = t2.ToString();
                ScoreUI.SetActive(true);
            }
            else
            {
                ScoreUI.SetActive(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {
            bl_UIReferences.Instance.PlayerUI.MaxKillsUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstancePlayers()
        {
            CleanCache();
            MFPSPlayer[] list = bl_GameManager.Instance.GetMFPSPlayerInTeam(Team.Team1);
            for (int i = 0; i < list.Length; i++)
            {
                GameObject g = Instantiate(PlayerHolderUI) as GameObject;
                g.name = list[i].Name;
                g.GetComponent<Image>().sprite = Team1FaceSprite;
                g.transform.SetParent(Team1Panel, false);
                cacheHolders.Add(g);
            }
            list = bl_GameManager.Instance.GetMFPSPlayerInTeam(Team.Team2);
            for (int i = 0; i < list.Length; i++)
            {
                GameObject g = Instantiate(PlayerHolderUI) as GameObject;
                g.name = list[i].Name;
                g.GetComponent<Image>().sprite = Team2FaceSprite;
                g.transform.SetParent(Team2Panel, false);
                cacheHolders.Add(g);
            }
            ScoreUI.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstancePlayersFromServer(string[] alivePlayers)
        {
            InstancePlayers();
            List<string> list = new List<string>();
            list.AddRange(alivePlayers);
            for (int i = 0; i < cacheHolders.Count; i++)
            {
                if (cacheHolders[i].name == PhotonNetwork.LocalPlayer.NickName)
                {
                    cacheHolders[i].GetComponent<Image>().color = Color.grey;
                    continue;
                }
                if (!list.Contains(cacheHolders[i].name))
                {
                    cacheHolders[i].GetComponent<Image>().color = Color.red;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="t"></param>
        public void OnPlayerDeath(string player, Team t)
        {
            for (int i = 0; i < cacheHolders.Count; i++)
            {
                if (cacheHolders[i].name == player)
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
    }
}