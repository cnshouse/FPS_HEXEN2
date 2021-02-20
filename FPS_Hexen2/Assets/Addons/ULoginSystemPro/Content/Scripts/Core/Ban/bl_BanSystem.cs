using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

namespace MFPS.ULogin
{
    public class bl_BanSystem : bl_LoginProBase
    {
        [Header("References")]
        [SerializeField] private GameObject BannedUI = null;
        [SerializeField] private Text BannedText = null;

        public List<BanUserInfo> BanList = new List<BanUserInfo>();
        private BanUserInfo banUserInfo;

        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            if (BannedUI) { BannedUI.transform.SetAsLastSibling(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public void VerifyIP(Action callback)
        {
            var ip = bl_LoginPro.Instance.CurrentIp;
            if (string.IsNullOrEmpty(ip))
            {
                callback?.Invoke();
                return;
            }

            bl_ULoginLoadingWindow.Instance.SetText("Checking client status with server...");
            var wf = CreateForm(false);
            wf.AddField("typ", 2);
            wf.AddField("ip", ip);

            WebRequest.POST(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.BanList), wf, (result) =>
              {
                  if (result.isError)
                  {
                      result.PrintError();
                  }
                  else
                  {
                      if(result.HTTPCode == 202)//found user IP in the banned users
                      {
                          banUserInfo = result.FromJson<BanUserInfo>();
                          if(banUserInfo == null)
                          {
                              Debug.LogWarning($"Detected ban but receive wrong response from server: {result.RawText}");
                              callback?.Invoke();
                              return;
                          }
                          BannedText.text = string.Format("You has been permanently <b>banned</b> from the game due {0} by {1}.", banUserInfo.reason, banUserInfo.by);
                          BannedUI.SetActive(true);
                          bl_ULoginLoadingWindow.Instance.SetActive(false);
                          result.Print();
                          return;
                      }
                      else
                      {
                          //User IP is clean
                      }
                  }
                  callback?.Invoke();
              });

        }

        [Obsolete]
        public void Process(Action callback)
        {
            StartCoroutine(GetBanList(callback));
        }

        /// <summary>
        /// Get Ban List for sure that this IP is not ban
        /// </summary>
        /// <returns></returns>
        IEnumerator GetBanList(Action callback)
        {
            // yield return new WaitForSeconds(1);
            bl_ULoginLoadingWindow.Instance.SetText("Checking client status with server...");
            Dictionary<string, string> wf = new Dictionary<string, string>();
            wf.Add("typ", "0");
            UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.BanList), wf);

            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                print("Error: " + www.error);
            }
            else
            {
                string text = www.downloadHandler.text;
                if (text.Contains("result") == true)
                {
                    string[] splitusers = text.Split("\n"[0]);
                    foreach (string t in splitusers)
                    {
                        if (!string.IsNullOrEmpty(t) && !t.Contains("result"))
                        {
                            string[] splitinfo = t.Split("|"[0]);
                            BanUserInfo bui = new BanUserInfo();
                            bui.name = splitinfo[0];
                            bui.reason = splitinfo[1];
                            bui.ip = splitinfo[2];
                            bui.by = splitinfo[3];
                            BanList.Add(bui);
                        }
                    }
                    CheckBan();
                }
                else
                {
                    if (text.Contains("empty") == true)
                    {
                        Debug.Log("Don't have banned users.");
                       /* if (Login.GuestButton != null) { Login.GuestButton.interactable = true; Login.GuestButton.gameObject.SetActive(true); }
                        Login.onReady?.Invoke();*/
                    }
                    else
                    {
                        Debug.Log(text);
                    }
                }
            }
            if (!bl_GameData.isDataCached)
            {
                yield return StartCoroutine(bl_GameData.AsyncLoadData());
            }
            callback?.Invoke();
            www.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckBan()
        {
            for (int i = 0; i < BanList.Count; i++)
            {
                if (BanList[i].ip == Login.CurrentIp)
                {
                    OnBan(i);
                    return;
                }
            }

            Login.SetPlayAsGuestButtonActive(true);
            Login.onReady?.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckBanAccount(string username)
        {
            for (int i = 0; i < BanList.Count; i++)
            {
                if (BanList[i].name == username)
                {
                    Debug.Log("Ban detected!");
                    OnBan(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnBan(int id)
        {
            BannedText.text = string.Format("You has been permanently <b>banned</b> from the game due {0} by {1}.", BanList[id].reason, BanList[id].by);
            BannedUI.SetActive(true);
        }

        private bl_LoginPro Login => bl_LoginPro.Instance;

        [System.Serializable]
        public class BanUserInfo
        {
            public string id;
            public string name;
            public string reason;
            public string ip;
            public string by;
        }
    }
}
