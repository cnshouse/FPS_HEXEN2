using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class bl_LanguageSelector : MonoBehaviour
{
    [SerializeField] private Dropdown m_Dropdown;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        int cid = bl_Localization.Instance.LoadStoreLanguage(true);
        m_Dropdown.ClearOptions();
        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
        for (int i = 0; i < bl_Localization.Instance.Languages.Length; i++)
        {
            Dropdown.OptionData o = new Dropdown.OptionData();
            o.text = bl_Localization.Instance.Languages[i].Text.LanguageName.ToUpper();
            o.image = bl_Localization.Instance.Languages[i].Text.LanguageIcon;
            options.Add(o);
        }
        m_Dropdown.AddOptions(options);
        m_Dropdown.value = cid;
    }

    public void OnChange(int id)
    {
        bl_Localization.Instance.SetLanguage(id);
    }
}