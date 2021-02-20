using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace MFPS.Runtime.Settings
{
    public class bl_SingleSettingsBinding : MonoBehaviour
    {
        [Header("Settings")]
        public string SettingKeyName = "";
        public string[] optionsNames = new string[] { "Disable", "Enable" };
        public bool autoUpperCase = false;

        [Header("Listener")]
        public OnChange onChange;

        public int currentOption { get; set; } = 0;
        public bool CurrentSelectionAsBool() => currentOption == 1 ? true : false;

        [System.Serializable] public class OnChange : UnityEvent<int> { }
        [Header("References")]
        public Text optionText;

#if LOCALIZATION
        private string[] cachedKeys;
#endif

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
#if LOCALIZATION
            if(cachedKeys == null)
            cachedKeys = optionsNames;
            bl_Localization.Instance.OnLanguageChange += Localize;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator Start()
        {
            if (string.IsNullOrEmpty(SettingKeyName))
            {
                ApplyCurrentValue();
                yield break;
            }

            while (!bl_GameData.isDataCached) yield return null;
            Load();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
#if LOCALIZATION
            bl_Localization.Instance.OnLanguageChange -= Localize;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            var val = bl_MFPS.Settings.GetSettingOf(SettingKeyName);
            int v = 0;
            if (val is bool) { v = (bool)val == true ? 1 : 0; }
            else if (val is int)
            {
                v = (int)val;
            }
            else
            {
                Debug.LogWarning($"Setting {SettingKeyName} is a invalid type"); return;
            }
            currentOption = v;
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetOptions(string[] newOptions)
        {
            optionsNames = newOptions;
            currentOption = 0;
            ApplyCurrentValue();

#if LOCALIZATION
            cachedKeys = optionsNames;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplyCurrentValue()
        {
            if (currentOption == -1) return;
            if (currentOption >= optionsNames.Length) return;

            string value = autoUpperCase ? optionsNames[currentOption].ToUpper() : optionsNames[currentOption];
            optionText.text = value;

            ApplySetting();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplySetting()
        {
            onChange?.Invoke(currentOption);
            if (string.IsNullOrEmpty(SettingKeyName)) return;

            //Set the changed setting value to the instance settings group.
            bl_MFPS.Settings.SetSettingOf(SettingKeyName, currentOption, bl_RuntimeSettings.Instance.autoSaveSettings);
            if (bl_RuntimeSettings.Instance.applySettingsOnChange)
            {
                //call the listener that will apply the settings in game
                bl_MFPS.Settings.ApplySettings(false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeOption(bool forward)
        {
            if (forward)
            {
                currentOption = (currentOption + 1) % optionsNames.Length;
            }
            else
            {
                if (currentOption <= 0) { currentOption = optionsNames.Length - 1; }
                else { currentOption--; }
            }
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveValue()
        {
            bl_MFPS.Settings.SaveSettings();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Revert()
        {
            Load();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetSelection()
        {
            currentOption = 0;
            ApplyCurrentValue();
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetCurrentFromOptionName(string optionName)
        {
            for (int i = 0; i < optionsNames.Length; i++)
            {
                if(optionsNames[i] == optionName)
                {
                    currentOption = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Localize(Dictionary<string,string> data)
        {
#if LOCALIZATION
            for (int i = 0; i < optionsNames.Length; i++)
            {
                bl_Localization.Instance.TryGetText(cachedKeys[i].ToLower(), ref optionsNames[i]);
            }
            ApplyCurrentValue();
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (optionText == null || optionsNames == null || optionsNames.Length <= 0) return;
            if (currentOption >= optionsNames.Length) { currentOption = 0; return; }

            string value = autoUpperCase ? optionsNames[currentOption].ToUpper() : optionsNames[currentOption];
            optionText.text = value;
        }
#endif
    }
}