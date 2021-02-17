using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
namespace MFPS.InputManager
{
    public class bl_InputBindingUI : MonoBehaviour
    {
        public Text KeyNameText;
        public Text PrimaryKeyText;
        public Text AltKeyText;
        public GameObject PrimaryOverlay, AltOverlay;

        public ButtonData CachedData { get; set; }
        private bl_InputUI UIManager;
        private int waitingFor = 0;

        /// <summary>
        /// 
        /// </summary>
        public void Set(ButtonData data, bl_InputUI uimanager)
        {
            CachedData = data;
            UIManager = uimanager;
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        void ApplyUI()
        {
            KeyNameText.text = CachedData.Description;
            if (!CachedData.PrimaryIsAxis)
                PrimaryKeyText.text = CachedData.PrimaryKey != KeyCode.None ? Regex.Replace(CachedData.PrimaryKey.ToString(), "[A-Z]", " $0").Trim() : "None";
            else
                PrimaryKeyText.text = CachedData.PrimaryAxis;

            if (!CachedData.AlternativeIsAxis)
                AltKeyText.text = CachedData.AlternativeKey != KeyCode.None ? Regex.Replace(CachedData.AlternativeKey.ToString(), "[A-Z]", " $0").Trim() : "None";
            else
                AltKeyText.text = CachedData.AlternativeAxis;


            PrimaryOverlay.SetActive(false);
            AltOverlay.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnEdit(bool primary)
        {
            if (UIManager.ChangeKeyFor(this))
            {
                PrimaryOverlay.SetActive(primary);
                AltOverlay.SetActive(!primary);
                if (primary) { PrimaryKeyText.text = ""; }
                else { AltKeyText.text = ""; }
                waitingFor = primary ? 1 : 2;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnChanged(bl_InputUI.PendingButton button)
        {
            if (waitingFor == 1)
            {
                if (button.isAKey)
                {
                    CachedData.PrimaryIsAxis = false;
                    CachedData.PrimaryKey = button.Key;
                    CachedData.PrimaryAxis = "";
                }
                else
                {
                    CachedData.PrimaryIsAxis = true;
                    CachedData.PrimaryAxis = button.Axis;
                    CachedData.PrimaryKey = KeyCode.None;
                }
            }
            else if (waitingFor == 2)
            {
                if (button.isAKey)
                {
                    CachedData.AlternativeIsAxis = false;
                    CachedData.AlternativeKey = button.Key;
                    CachedData.AlternativeAxis = "";
                }
                else
                {
                    CachedData.AlternativeIsAxis = true;
                    CachedData.AlternativeAxis = button.Axis;
                    CachedData.AlternativeKey = KeyCode.None;
                }
            }

            ApplyUI();
            waitingFor = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelChange()
        {
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetKey(KeyCode key)
        {
            if(CachedData.PrimaryKey == key) { CachedData.PrimaryKey = KeyCode.None; }
            else if (CachedData.AlternativeKey == key) { CachedData.AlternativeKey = KeyCode.None; }
            ApplyUI();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetAxis(string axis)
        {
            if (string.IsNullOrEmpty(axis)) return;

            if (CachedData.PrimaryAxis == axis) { CachedData.PrimaryAxis = ""; }
            else if (CachedData.AlternativeAxis == axis) { CachedData.AlternativeAxis = ""; }
            ApplyUI();
        }
    }
}