using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace MFPS.ULogin
{
    public class bl_PasswordField : MonoBehaviour
    {
        public InputField inputField;
        public Image iconImg;
        public Sprite HideIcon, VisibleIcon;

        /// <summary>
        /// 
        /// </summary>
        public void Switch()
        {
            bool isHidde = inputField.contentType == InputField.ContentType.Password;
            isHidde = !isHidde;
            inputField.contentType = (isHidde) ? InputField.ContentType.Password : InputField.ContentType.Standard;
            inputField.ForceLabelUpdate();
            iconImg.sprite = (isHidde) ? HideIcon : VisibleIcon;
        }
    }
}