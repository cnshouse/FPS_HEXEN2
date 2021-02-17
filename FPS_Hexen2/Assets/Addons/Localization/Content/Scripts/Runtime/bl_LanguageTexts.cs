using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lovatto.Localization
{
    public class bl_LanguageTexts : ScriptableObject
    {
        public string LanguageName = "";
        public string PlurarLetter = "s";
        public Sprite LanguageIcon;
        public TextData[] Data;

        [ContextMenu("Print Texts")]
        void PrintText()
        {
            string t = "";
            for (int i = 0; i < Data.Length; i++)
            {
                t += Data[i].Text + "\n";
            }
            Debug.Log(t);
        }

        [System.Serializable]
        public class TextData
        {
            public string StringID = "";
            public string Text = "";
        }
    }
}