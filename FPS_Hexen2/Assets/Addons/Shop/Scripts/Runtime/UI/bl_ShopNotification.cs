using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Shop
{
    public class bl_ShopNotification : MonoBehaviour
    {
        public GameObject content;
        public Text notificationText;

        public void Show(string text)
        {
            notificationText.text = text.ToUpper();
            content.SetActive(true);
        }

        public async void Hide(int delay)
        {
            await System.Threading.Tasks.Task.Delay(delay * 1000);
            content.SetActive(false);
        }
    }
}