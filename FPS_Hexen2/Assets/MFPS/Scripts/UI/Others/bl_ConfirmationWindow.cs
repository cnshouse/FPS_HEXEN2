using System;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_ConfirmationWindow : MonoBehaviour
    {
        public Text descriptionText;
        public GameObject content;

        [Header("Events")]
        public bl_EventHandler.UEvent onConfirm;
        public bl_EventHandler.UEvent onCancel;

        private Action callback;
        private Action cancelCallback;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="onAccept"></param>
        public void AskConfirmation(string description, Action onAccept, Action onCancel = null)
        {
            callback = onAccept;
            cancelCallback = onCancel;
            if(!string.IsNullOrWhiteSpace(description))
            descriptionText.text = description;

            content.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Confirm()
        {
            callback?.Invoke();
            onConfirm?.Invoke();
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Cancel()
        {
            callback = null;
            cancelCallback?.Invoke();
            onCancel?.Invoke();
            cancelCallback = null;
            content.SetActive(false);
        }
    }
}