using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MFPS.ClassCustomization
{
    [System.Serializable]
    public class HudClassContent
    {

        public Image Icon;
        public Text WeaponNameText;
        public Slider DamageSlider;
        public Slider AccuracySlider;
        public Slider RateSlider;
    }
}