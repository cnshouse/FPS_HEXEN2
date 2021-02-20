using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Text.RegularExpressions;

namespace MFPS.Addon.Clan
{
    public class bl_ClanSearch : MonoBehaviour
    {

        public Text SearchLogText;
        public InputField SearchClanInput;
        [HideInInspector] public bl_ClanInfo LastSearchInfo = null;
        private bool isSearching = false;

        /// <summary>
        /// 
        /// </summary>
        public void SearchClan()
        {
            string clanName = SearchClanInput.text;
            if (string.IsNullOrEmpty(clanName))
            {
                SearchLogText.text = "Clan Name can't be empty";
                return;
            }
            if (!Regex.IsMatch(clanName, @"^[a-zA-Z0-9_ ]+$"))
            {
                SearchLogText.text = "Clan Name contain no allowed characters.";
                return;
            }
            SearchLogText.text = string.Empty;
            StartCoroutine(Search(clanName, true));
        }

        public void DoSearch(string clanName, bool display)
        {
            if (isSearching) return;
            StartCoroutine(Search(clanName, display));
        }

        public IEnumerator Search(string clanName, bool display)
        {
            isSearching = true;
            bl_ClanManager.Instance.LoadingOverlays[3].SetActive(false);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 9);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanName);

            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    string[] split = t.Split("|"[0]);
                    if (split[0].Contains("yes"))
                    {
                        bl_ClanInfo ci = new bl_ClanInfo();
                        ci.Name = clanName;
                        ci.ID = int.Parse(split[1]);
                        ci.Date = split[2];
                        ci.DecompileMembers(split[3]);
                        ci.Score = int.Parse(split[4]);
                        ci.Description = split[5];
                        ci.DecompileSettings(split[6]);
                        if (display)
                        {
                            bl_ClanManager.Instance.DisplayClanInfo(ci);
                        }
                        LastSearchInfo = ci;
                    }
                    else
                    {
                        SearchLogText.text = t;
                        Debug.Log(t);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            bl_ClanManager.Instance.LoadingOverlays[3].SetActive(false);
            isSearching = false;
        }

        private static bl_ClanSearch _instance;
        public static bl_ClanSearch Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<bl_ClanSearch>(); }
                return _instance;
            }
        }
    }
}