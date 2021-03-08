﻿using UnityEngine;
using System.Collections.Generic;
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using System.Text;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Globalization;
using Random = UnityEngine.Random;
using System.Text.RegularExpressions;
using UnityEngine.Analytics;
using MFPS.ULogin;

public static class bl_DataBaseUtils
{
    public const string LOCK_TIME_KEY = "ulsp.ll.time";
    public static void LoadLevel(string scene)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(scene);
#else
        Application.LoadLevel(scene);
#endif
    }

    public static void LoadLevel(int scene)
    {
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(scene);
#else
        Application.LoadLevel(scene);
#endif
    }

    public static string GenerateKey(int length = 7)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghlmnopqrustowuvwxyz";
        string key = "";
        for (int i = 0; i < length; i++)
        {
            key += chars[Random.Range(0, chars.Length)];
        }
        return key;
    }

    public static int ToInt(this string str)
    {
        int i = 0;
        if (!int.TryParse(str, out i))
        {
            Debug.LogWarning("Can't parse this string: " + str);
        }
        return i;
    }

    public static string Md5Sum(string input)
    {
        MD5 md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++) { sb.Append(hash[i].ToString("X2")); }
        return sb.ToString();
    }

    public static string CreateSecretHash(string parameters)
    {
        return Md5Sum(parameters + bl_LoginProDataBase.Instance.SecretKey).ToLower();
    }

    public static string TimeFormat(float minutes)
    {
        TimeSpan t = TimeSpan.FromMinutes((double)minutes);
        string answer = string.Format("{0:D2}h:{1:D2}m", t.Hours, t.Minutes);
        return answer;
    }

    public static bool ContainsAny(this string haystack, params string[] needles)
    {
        foreach (string needle in needles)
        {
            if (haystack.Contains(needle))
                return true;
        }

        return false;
    }
    public static WWWForm CreateWWWForm(FormHashParm hashParamenter = FormHashParm.ID, bool addSID = false)
    {
        WWWForm wf = new WWWForm();
        if (hashParamenter != FormHashParm.None)
        {
            string parm = "0";
            if (bl_DataBase.Instance != null && bl_DataBase.Instance.LocalUser != null)
            {
                parm = hashParamenter == FormHashParm.ID ? bl_DataBase.Instance.LocalUser.ID.ToString() : bl_DataBase.Instance.LocalUser.LoginName;
            }
            if (hashParamenter == FormHashParm.Name)
            {
                wf.AddSecureField("name", parm);
            }
            string hash = Md5Sum(parm + bl_LoginProDataBase.Instance.SecretKey).ToLower();
            wf.AddSecureField("hash", hash);
        }
        if (addSID && bl_DataBase.Instance != null)
        {
            wf.AddField("sid", AnalyticsSessionInfo.sessionId.ToString());
        }
        return wf;
    }

    public static WWWForm CreateWWWForm(bool addBasicHash = true, bool addSID = false) => CreateWWWForm(addBasicHash ? FormHashParm.ID : FormHashParm.None, addSID);

    /// <summary>
    /// Simple AES Encryption
    /// </summary>
    /// <param name="encryptString"></param>
    /// <returns></returns>
    public static string Encrypt(string encryptString)
    {
        if (string.IsNullOrEmpty(encryptString))
        {
            return string.Empty;
        }
        string EncryptionKey = bl_LoginProDataBase.Instance.SecretKey;
        byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                encryptString = Convert.ToBase64String(ms.ToArray());
            }
        }
        return encryptString;
    }

    /// <summary>
    /// Simple AES decryption
    /// </summary>
    /// <param name="cipherText"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return string.Empty;
        }
        string EncryptionKey = bl_LoginProDataBase.Instance.SecretKey;
        cipherText = cipherText.Replace(" ", "+");
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
        });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
        }
        return cipherText;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    public static bool IsUsername(string username)
    {
        string pattern;
        // start with a letter, allow letter or number, length between 6 to 12.
        pattern = @"^[a-zA-Z0-9_ ]+$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(username);
    }

    /// <summary>
    /// Sanitize input from the client before send to the server
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string SanitazeString(string input)
    {
        input = input.Replace("|", "");
        input = input.Replace("&&", "");
        return input;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void AddSecureField(this WWWForm wf, string key, int value) => AddSecureField(wf, key, value.ToString());

    /// <summary>
    /// 
    /// </summary>
    public static void AddSecureField(this WWWForm wf, string key, string value)
    {
        if (bl_LoginProDataBase.Instance.PerToPerEncryption && bl_DataBase.Instance != null && !string.IsNullOrEmpty(bl_DataBase.Instance.RSAPublicKey))
            wf.AddField(key, bl_RSA.Encrypter(value, bl_DataBase.Instance.RSAPublicKey));
        else
            wf.AddField(key, value);
    }

    public static void AddNonSecureField(this WWWForm wf, string key, string value)
	{
        wf.AddField(key, value);
	}

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string GetUnitySessionID() => AnalyticsSessionInfo.sessionId.ToString();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bool IsLoginBlocked()
    {
        string key = Encrypt(LOCK_TIME_KEY);
        string date = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrEmpty(date)) return false;

        date = Decrypt(date);
        var gl = new CultureInfo("en-US");
        var lockDate = DateTime.Parse(date, gl);
        var ct = lockDate.Subtract(DateTime.Now.ToUniversalTime());

        return (ct.Minutes > 0 || ct.Seconds > 0);
    }
}