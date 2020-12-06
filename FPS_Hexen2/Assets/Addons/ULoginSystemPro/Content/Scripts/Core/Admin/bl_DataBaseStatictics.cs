using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class bl_DataBaseStatictics : MonoBehaviour
{

    [SerializeField]private Text PlayersCountText;
    [SerializeField]private Text BanPlayersCountText;
    [SerializeField]private Text LastPlayersCountText;
    [SerializeField]private Text GamePlayTimeText;

    private void Start()
    {
        StartCoroutine(IEGetInfo());
    }

    IEnumerator IEGetInfo()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("type", "3");
        using (UnityWebRequest www = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.RequestUser), wf))
        {
            yield return www.SendWebRequest();
            if (www.error == null && !www.isNetworkError)
            {
                string[] result = www.downloadHandler.text.Split("|"[0]);
                if (result[0].Contains("info"))
                {
                    PlayersCountText.text = result[1];
                    LastPlayersCountText.text = result[2];
                    BanPlayersCountText.text = result[3];
                    GamePlayTimeText.text = bl_DataBaseUtils.TimeFormat(result[4].ToInt());
                }
                else
                {
                    Debug.LogWarning(www.downloadHandler.text);
                }
            }
            else
            {
                Debug.LogError(www.error);
                CancelInvoke("BanComprobation");
            }
        }
    }
}