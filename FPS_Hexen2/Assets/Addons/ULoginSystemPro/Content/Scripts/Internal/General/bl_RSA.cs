using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public class bl_RSA
{
    private const int KEY_LENGHT = 1024;

    /// <summary>
    /// 
    /// </summary>
    public static string Encrypter(string data, string publicKey)
    {
        if(string.IsNullOrEmpty(publicKey) || publicKey.Length < 100)
        {
            Debug.LogError("RSA key is null or not valid.");
            return string.Empty;
        }

        var csp = new RSACryptoServiceProvider(KEY_LENGHT);
        csp.FromXmlString(publicKey);
        var byteArray = Encoding.UTF8.GetBytes(data);
        var bytesCypherText = csp.Encrypt(byteArray, false);
        return Convert.ToBase64String(bytesCypherText);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string Decrypt(string encrypted, string publicKey)
    {
        if (string.IsNullOrEmpty(publicKey) || publicKey.Length < 100)
        {
            Debug.LogError("RSA key is null or not valid.");
            return string.Empty;
        }

        var csp = new RSACryptoServiceProvider(KEY_LENGHT);
        csp.FromXmlString(publicKey);
        var byteArray = Encoding.UTF8.GetBytes(encrypted);
        var bytesCypherText = csp.Decrypt(byteArray, false);
        return Convert.ToBase64String(bytesCypherText);
    }
}