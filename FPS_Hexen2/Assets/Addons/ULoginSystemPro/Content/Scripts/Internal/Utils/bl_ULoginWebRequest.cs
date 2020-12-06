using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

public class bl_ULoginWebRequest 
{
    public MonoBehaviour instanceReference;

    public bl_ULoginWebRequest(MonoBehaviour reference)
    {
        instanceReference = reference;
    }

    public void POST(string uri, WWWForm postData, Action<ULoginResult> callBack)
    {
        instanceReference.StartCoroutine(DoPost(uri, postData, callBack));
    }

    /// <summary>
    /// 
    /// </summary>
    IEnumerator DoPost(string uri, WWWForm postData, Action<ULoginResult> callBack)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(uri, postData))
        {
            yield return www.SendWebRequest();
            ULoginResult result = new ULoginResult(www);
            if (callBack != null) { callBack.Invoke(result); }
        }
    }
}