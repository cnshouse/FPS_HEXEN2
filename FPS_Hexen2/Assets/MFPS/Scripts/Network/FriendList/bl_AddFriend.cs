using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.FriendList
{
    public class bl_AddFriend : MonoBehaviour
    {
        public InputField nameInput;
        public Text logText;
        public Button addButton;

        /// <summary>
        /// 
        /// </summary>
        public void AddFriend()
        {
#if !ULSP
            bl_FriendList.Instance?.AddFriend(nameInput);
            gameObject.SetActive(false);
#else
            var name = nameInput.text;
            if (string.IsNullOrEmpty(name)) return;

#if UNITY_EDITOR
            if (!bl_DataBase.IsUserLogged)
            {
                Debug.Log("Player is not logged, can't be checked if user exist in database.");
                Add(name);
                return;
            }
#endif

            addButton.interactable = false;
            bl_DataBase.CheckIfUserExist(this, "name", name, (exist) =>
               {
                   if (exist)
                   {
                       Add(name);
                   }
                   else
                   {
                       logText.text = $"Player '{name}' not exist.";
                   }
                   nameInput.text = string.Empty;
                   addButton.interactable = true;
               });
#endif
        }

        private void Add(string friendName)
        {
            logText.text = string.Empty;
            bl_FriendList.Instance?.AddFriend(friendName);
            gameObject.SetActive(false);
        }
    }
}