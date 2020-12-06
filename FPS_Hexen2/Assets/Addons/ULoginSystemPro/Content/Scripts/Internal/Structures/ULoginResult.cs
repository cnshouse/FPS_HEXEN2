using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
            if (resultPrefix.Contains("success"))
            {
                resultState = Status.Success;
            }
            else
            {
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
    public void Print()
    {
        if (!WWW.isNetworkError && !WWW.isHttpError)
        {
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
            string resultPrefix = text.Length >= 7 ? WWW.downloadHandler.text.Substring(0, 7) : text;
            if (resultPrefix.Contains("success"))
            {
                return WWW.downloadHandler.text.Substring(7, WWW.downloadHandler.text.Length - 7);
            }
            else
            {
                return WWW.downloadHandler.text;
            }
        }
    }

    public bool isError { get { return resultState == Status.Error; } }

    public enum Status
    {
        Success,
        Error,
        Unknown,
    }
}