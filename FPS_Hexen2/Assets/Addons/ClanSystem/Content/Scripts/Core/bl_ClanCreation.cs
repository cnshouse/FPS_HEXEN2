using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;

namespace MFPS.Addon.Clan
{
    public class bl_ClanCreation : MonoBehaviour
    {
        public InputField NameField;
        public InputField DescriptionText;
        public Button CreateButton;
        public GameObject MyClanWindow;
        public Text LogText;
        public Text CreatePriceText;
        public GameObject CantCreateUI;

        public bool Invitations { get; set; }
        public bool isPublic { get; set; }
        private bool isRequesting = false;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            bool v = false;
#if CLANS
            v = (bl_DataBase.Instance.LocalUser.Coins >= bl_LoginProDataBase.Instance.CreateClanPrice);
            CreateButton.interactable = v;
#endif
            CantCreateUI.SetActive(!v);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
#if CLANS
            CreatePriceText.text = string.Format("{0} COINS", bl_LoginProDataBase.Instance.CreateClanPrice);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Create()
        {
            if (isRequesting) return;
#if CLANS
            if (bl_DataBase.Instance.LocalUser.Coins < bl_LoginProDataBase.Instance.CreateClanPrice)
            {
                LogText.text = "You don't have enough coins to create a clan.";
                return;
            }
#endif
            string clanName = NameField.text;
            string des = DescriptionText.text;
            if (string.IsNullOrEmpty(clanName) || string.IsNullOrEmpty(des))
            {
                LogText.text = "One or more fields are not assigned";
                Debug.Log("One or more fields are not assigned");
                return;
            }
            if (!Regex.IsMatch(clanName, @"^[a-zA-Z0-9_ ]+$"))
            {
                LogText.text = "Clan Name contain no allowed characters.";
                Debug.Log("Clan Name contain no allowed characters.");
                return;
            }
            if (!Regex.IsMatch(des, @"^[a-zA-Z0-9_. ]+$"))
            {
                LogText.text = "Description contain no allowed characters.";
                Debug.Log("Description contain no allowed characters.");
                return;
            }
            if (clanName.ContainsAny("|", ",", "-", "{", "'", "$", "\""))
            {
                LogText.text = "Clan Name contain no allowed characters.";
                Debug.Log("Clan Name contain no allowed characters.");
                return;
            }
            if (des.ContainsAny("|", ",", "-", "{", "'", "$", "\""))
            {
                LogText.text = "Description contain no allowed characters.";
                Debug.Log("Description contain no allowed characters.");
                return;
            }

            StartCoroutine(CreateClan(clanName, des));
        }

        IEnumerator CreateClan(string clanName, string descr)
        {
            LogText.text = "";
            isRequesting = true;
            bl_ClanManager.Instance.LoadingOverlays[4].SetActive(true);
            WWWForm wf = new WWWForm();
            wf.AddField("type", 3);
            wf.AddField("hash", bl_DataBase.Instance.GetUserToken());
            wf.AddField("clanID", clanName);
            wf.AddField("desc", descr);
            wf.AddField("userID", bl_DataBase.Instance.LocalUser.ID);
            string settings = string.Format("{0},{1},", Invitations ? 1 : 0, isPublic ? 1 : 0);
            wf.AddField("settings", settings);


            using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Clans), wf))
            {
                yield return w.SendWebRequest();

                if (!w.isNetworkError)
                {
                    string t = w.downloadHandler.text;
                    if (t.Contains("done"))
                    {
#if CLANS
                        string[] split = t.Split("|"[0]);
                        int ci = int.Parse(split[1]);
                        if (bl_DataBase.Instance.LocalUser.Clan == null) bl_DataBase.Instance.LocalUser.Clan = new bl_ClanInfo();

                        var clan = bl_DataBase.Instance.LocalUser.Clan;
                        bl_DataBase.Instance.LocalUser.Clan.ID = ci;
                        clan.Members.Add(new bl_ClanInfo.ClanMember()
                        {
                            ID = bl_DataBase.LocalUserInstance.ID,
                            Name = bl_DataBase.LocalUserInstance.NickName,
                            Role = ClanMemberRole.Leader
                        });
                        bl_DataBase.Instance.SubtractCoins(bl_LoginProDataBase.Instance.CreateClanPrice);
#endif
                        NameField.text = string.Empty;
                        DescriptionText.text = string.Empty;
                        yield return new WaitForSeconds(2);
                        MyClanWindow.SetActive(true);
                        bl_UserProfile.Instance.OnLogin();
                        gameObject.SetActive(false);
                    }
                    else
                    {
                        LogText.text = t;
                        Debug.Log(t);
                    }
                }
                else
                {
                    Debug.LogError(w.error);
                }
            }
            isRequesting = false;
        }
    }
}