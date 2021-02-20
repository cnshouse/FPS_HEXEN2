﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Addon.PlayerSelector
{
    public class bl_PlayerSelectorLobby : MonoBehaviour
    {
        public bl_PSTeamUI[] TeamsUI;

        [Header("References")]
        public GameObject[] Windows;
        public GameObject OperatorUIPrefab;
        public Transform OperatorsListPanel;
        public Text TeamNameSelectorText;
        public Text OperatorSelectorText;
        public Image OperatorSelectorImg;
        public CanvasGroup FadeAlpha;

        private List<bl_PSOperatorUI> cacheOperatorsUI = new List<bl_PSOperatorUI>();
        public bl_PlayerSelectorInfo Team1PlayerInfo;
        public bl_PlayerSelectorInfo Team2PlayerInfo;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            LoadData();
            InitialSetUp();
        }

        /// <summary>
        /// 
        /// </summary>
        void LoadData()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        void InitialSetUp()
        {
           int team1op = bl_PlayerSelectorData.GetTeamOperatorID(Team.Team1);
           int team2op = bl_PlayerSelectorData.GetTeamOperatorID(Team.Team2);

            Team1PlayerInfo = bl_PlayerSelectorData.Instance.GetPlayerByIndex(team1op);
            Team2PlayerInfo = bl_PlayerSelectorData.Instance.GetPlayerByIndex(team2op);

            TeamsUI[0].SetUp(Team1PlayerInfo,Team.Team1);
            TeamsUI[1].SetUp(Team2PlayerInfo, Team.Team2);
        }

        public void ShowUpOperators(int teamID)
        {
            Team team = (Team)teamID;
            StopAllCoroutines();
            StartCoroutine(FadeOutToWindow(team, 1));
        }

        IEnumerator FadeOutToWindow(Team team, int windowID)
        {
            float d = 0;
            while(d < 1)
            {
                d += Time.deltaTime * 5;
                FadeAlpha.alpha = d;
                yield return null;
            }
            foreach(GameObject g in Windows) { g.SetActive(false); }
            Windows[windowID].SetActive(true);
            if (windowID == 1)
            {
                ShowOperators(team);
            }
            while (d > 0)
            {
                d -= Time.deltaTime * 5;
                FadeAlpha.alpha = d;
                yield return null;
            }
        }

        public void ShowMain()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutToWindow(Team.All, 0));
        }

        /// <summary>
        /// 
        /// </summary>
        public void ShowOperators(Team team)
        {
            ClearCache();
            List<bl_PlayerSelectorInfo> list = bl_PlayerSelectorData.Instance.GetPlayerList(team);
            for (int i = 0; i < list.Count; i++)
            {
                GameObject g = Instantiate(OperatorUIPrefab) as GameObject;
                g.transform.SetParent(OperatorsListPanel, false);
                bl_PSOperatorUI oui = g.GetComponent<bl_PSOperatorUI>();
                oui.SetUp(list[i], this);
                cacheOperatorsUI.Add(oui);
            }
            OnShowUpOp(team == Team.Team1 ? Team1PlayerInfo : Team2PlayerInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        void ClearCache()
        {
            cacheOperatorsUI.ForEach(x => Destroy(x.gameObject));
            cacheOperatorsUI.Clear();
        }

        public void SelectOperator(bl_PlayerSelectorInfo operatorInfo)
        {
            bl_PlayerSelectorData.SetTeamOperator(operatorInfo.ID, operatorInfo.team);
            OnShowUpOp(operatorInfo);
            Team t = operatorInfo.team;
            TeamsUI[((int)t - 1)].SetUp(bl_PlayerSelectorData.Instance.GetPlayerByIndex(operatorInfo.ID), t);
            if(t == Team.Team1) { Team1PlayerInfo = operatorInfo; }
            else { Team2PlayerInfo = operatorInfo; }
        }

        public void OnShowUpOp(bl_PlayerSelectorInfo operatorInfo)
        {
            TeamNameSelectorText.text = operatorInfo.team.GetTeamName().ToUpper();
            OperatorSelectorText.text = operatorInfo.Name.ToUpper();
            OperatorSelectorImg.sprite = operatorInfo.Preview;
        }

        public void ShowUpSelectedOne(Team showingTeam)
        {
            bl_PlayerSelectorInfo player = showingTeam == Team.Team1 ? Team1PlayerInfo : Team2PlayerInfo;
            OnShowUpOp(player);
        }

        public void SetTeamFav(int teamID)
        {
            Team t = (Team)teamID;
            bl_PlayerSelectorData.Instance.SetFavoriteTeam(t);
            foreach(bl_PSTeamUI ui in TeamsUI) { ui.SetTeamFav(t); }
        }
    }
}