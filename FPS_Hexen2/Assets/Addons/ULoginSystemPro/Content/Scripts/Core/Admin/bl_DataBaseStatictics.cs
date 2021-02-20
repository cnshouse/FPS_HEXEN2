using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class bl_DataBaseStatictics : MonoBehaviour
{

    [SerializeField]private Text PlayersCountText = null;
    [SerializeField]private Text BanPlayersCountText = null;
    [SerializeField]private Text LastPlayersCountText = null;
    [SerializeField]private Text GamePlayTimeText = null;

    private bl_ULoginWebRequest _webRequest;
    public bl_ULoginWebRequest WebRequest { get { if (_webRequest == null) { _webRequest = new bl_ULoginWebRequest(this); } return _webRequest; } }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        GetDataBaseStats();
    }

    /// <summary>
    /// 
    /// </summary>
    void GetDataBaseStats()
    {

        WWWForm wf = new WWWForm();
        wf.AddField("name", "admin");
        wf.AddField("type", 6);
        wf.AddField("hash", bl_DataBaseUtils.CreateSecretHash("admin"));

        var url = bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Admin);
        WebRequest.POST(url, wf, (result) =>
        {
            if (result.isError)
            {
                result.PrintError();
                return;
            }

            if (result.resultState == ULoginResult.Status.Success)
            {
                string[] raw = result.RawText.Split("|"[0]);
                PlayersCountText.text = raw[1];
                LastPlayersCountText.text = raw[2];
                BanPlayersCountText.text = raw[3];
                GamePlayTimeText.text = bl_DataBaseUtils.TimeFormat(raw[4].ToInt());
            }
            else
            {
                result.Print(true);
            }
        });
    }
}