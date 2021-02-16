using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_ULoginLoadingWindow : MonoBehaviour
{
    public GameObject content;
    [SerializeField] private Text middleText = null;

    public void SetActive(bool active)
    {
        content.SetActive(active);
    }

    public void SetText(string text)
    {
        middleText.text = text;
    }

    public void SetText(string text, bool active)
    {
        middleText.text = text;
        SetActive(active);
    }

    public bool isShowing => content.activeInHierarchy;

    private static bl_ULoginLoadingWindow _instance;
    public static bl_ULoginLoadingWindow Instance
    {
        get
        {
            if (_instance == null) { _instance = FindObjectOfType<bl_ULoginLoadingWindow>(); }
            return _instance;
        }
    }
}