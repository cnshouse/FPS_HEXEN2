using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class bl_DataBaseCreator : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Text LogText;
    [SerializeField] private Button[] OptionsButtons;
    private string code = "0";
    private string erroCode = "<color=green>NONE</color>";

    private void Start()
    {
        if (!string.IsNullOrEmpty(bl_LoginProDataBase.Instance.HostName))
        {
            StartCoroutine(Check());
        }
        else
        {
            erroCode = "Database access information has not been assigned in 'LoginDataBasePro', can't process with the request.";
            SetLog();
        }
    }

    public void CreateTables()
    {
        StartCoroutine(SetCreateTables());
    }

    IEnumerator Check()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("type", 2);
        wf.AddField("localhost", bl_LoginProDataBase.Instance.HostName);
        wf.AddField("dbuser", bl_LoginProDataBase.Instance.DataBaseUser);
        wf.AddField("dbname", bl_LoginProDataBase.Instance.DataBaseName);
        wf.AddField("dbpassword", bl_LoginProDataBase.Instance.Passworld);

        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf))
        {
            yield return w.SendWebRequest();
            if (!w.isNetworkError)
            {
                code = w.downloadHandler.text;
                SetLog();
            }
            else
            {
                code = "-1";
                erroCode = w.error;
                SetLog();
            }
        }
    }

    IEnumerator SetCreateTables()
    {
        WWWForm wf = new WWWForm();
        wf.AddField("type", 0);
        wf.AddField("localhost", bl_LoginProDataBase.Instance.HostName);
        wf.AddField("dbuser", bl_LoginProDataBase.Instance.DataBaseUser);
        wf.AddField("dbname", bl_LoginProDataBase.Instance.DataBaseName);
        wf.AddField("dbpassword", bl_LoginProDataBase.Instance.Passworld);

        using (UnityWebRequest w = UnityWebRequest.Post(bl_LoginProDataBase.Instance.GetUrl(bl_LoginProDataBase.URLType.Creator), wf))
        {
            yield return w.SendWebRequest();
            if (!w.isNetworkError)
            {
                string t = w.downloadHandler.text;
                if(t == "2") { code = t; OptionsButtons[0].interactable = false; }
               // Debug.Log(t);               
            }
            else
            {
                erroCode = w.error;
            }
            SetLog();
        }
    }

    public void SetLog(string extraLine = "")
    {
        string format = "DATABASE CONNECTION: {0}\nDATABASE CREATED: {1}\nTABLES CREATED: {2}\nPHP DIRECTORY SET:{3},\nSERVER DATABASE INFO ASSIGNED: {5}{6}\nERROR CODE: {4}";
        int codeID = 0;
        if(!int.TryParse(code,out codeID))
        {
            codeID = -1;
        }
        string connected = (codeID >= 1) ? "<color=green>YES</color>" : "<color=red>NO</color>";
        string dbcreated = (codeID >= 1) ? "<color=green>YES</color>" : "<color=red>NO</color>";
        string tabletcreated = (codeID >= 2) ? "<color=green>YES</color>" : "<color=red>NO</color>";
        string dirset = string.IsNullOrEmpty(bl_LoginProDataBase.Instance.PhpHostPath) ? "<color=red>NO</color>" : "<color=green>YES</color>";
        string serverinfo = (codeID >= 3) ? "<color=green>YES</color>" : "<color=red>NO</color>";
        if(serverinfo == "NO") { extraLine += "\n The information of the database has not been assigned in bl_Common.php in your server <b>or</b> it is wrong."; }
        string t = string.Format(format, connected, dbcreated, tabletcreated, dirset, erroCode, serverinfo, extraLine);
        LogText.text = t;
        OptionsButtons[0].interactable = (codeID >= 2) ? false : true;
    }

#endif
}