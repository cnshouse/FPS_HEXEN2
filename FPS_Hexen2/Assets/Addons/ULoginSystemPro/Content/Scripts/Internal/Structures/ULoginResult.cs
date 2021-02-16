using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using MFPS.ULogin;

public struct ULoginResult
{
    public UnityWebRequest WWW { get; private set; }
    public Status resultState { get; set; }

    public ULoginResult(UnityWebRequest www)
    {
        WWW = www;
        resultState = Status.Unknown;
        Build();
    }

    /// <summary>
    /// 
    /// </summary>
    void Build()
    {
        if (!WWW.isNetworkError && !WWW.isHttpError)
        {
            string text = WWW.downloadHandler.text;
            string resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;
            if (bl_LoginProDataBase.Instance.PerToPerEncryption && text.StartsWith("encrypt"))
            {
                text = text.Replace("encrypt", "");
                text = AES.Decrypt(text, bl_DataBaseUtils.GetUnitySessionID());
                resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;
            }

            if (resultPrefix.Contains("success"))
            {
                resultState = Status.Success;
            }
            else
            {
                if(HTTPCode == 400)
                {
                    resultState = Status.Fail;
                }else
                resultState = Status.Unknown;
            }
        }
        else
        {
            resultState = Status.Error;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public T FromJson<T>()
    {
        return JsonUtility.FromJson<T>(Text);
    }

    /// <summary>
    /// 
    /// </summary>
    public bool isEqual(string txt)
    {
        return Text.Contains(txt);
    }

    /// <summary>
    /// 
    /// </summary>
    public void Print(bool asWarning = false)
    {
        if (!WWW.isNetworkError && !WWW.isHttpError)
        {
            if (asWarning) Debug.LogWarning(Text);
            else
            Debug.Log(Text);
        }
        else
        {
            Debug.Log(WWW.error);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void PrintError()
    {
        Debug.LogError(WWW.error);
    }

    public string Text
    {
        get
        {
            string text = WWW.downloadHandler.text;
            string resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;

            if (bl_LoginProDataBase.Instance.PerToPerEncryption && text.StartsWith("encrypt"))
            {
                text = text.Replace("encrypt", "");
                text = AES.Decrypt(text, bl_DataBaseUtils.GetUnitySessionID());
                resultPrefix = text.Length >= 7 ? text.Substring(0, 7) : text;
            }
            if (resultPrefix.Contains("success"))
            {
                text = text.Substring(7, text.Length - 7);
            }

            return text;
        }
    }

    public string RawTextReadable
    {
        get
        {
            string text = WWW.downloadHandler.text;
            if (bl_LoginProDataBase.Instance.PerToPerEncryption && text.StartsWith("encrypt"))
            {
                text = text.Replace("encrypt", "");
                text = AES.Decrypt(text, bl_DataBaseUtils.GetUnitySessionID());
            }
            return text;
        }
    }

    public int HTTPCode => (int)WWW.responseCode;

    public string RawText => WWW.downloadHandler.text;

    public bool isError { get { return resultState == Status.Error; } }

    public enum Status
    {
        Success,
        Error,
        Unknown,
        Fail,
    }
}