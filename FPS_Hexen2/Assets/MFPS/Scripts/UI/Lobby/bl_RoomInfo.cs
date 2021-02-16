using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace MFPS.Runtime.UI
{
    public class bl_RoomInfo : MonoBehaviour
    {

        public Text RoomNameText = null;
        public Text MapNameText = null;
        public Text PlayersText = null;
        public Text GameModeText = null;
        public Text PingText = null;
        [SerializeField] private Text MaxKillText;
        public GameObject JoinButton = null;
        public GameObject FullText = null;
        [SerializeField] private GameObject PrivateUI;
        private RoomInfo cacheInfo = null;

        /// <summary>
        /// This method assign the RoomInfo and get the properties of it
        /// to display in the UI
        /// </summary>
        /// <param name="info"></param>
        public void GetInfo(RoomInfo info)
        {
            cacheInfo = info;
            RoomNameText.text = info.Name;
            MapNameText.text = (string)info.CustomProperties[PropertiesKeys.CustomSceneName];
            GameModeText.text = (string)info.CustomProperties[PropertiesKeys.GameModeKey];
            PlayersText.text = info.PlayerCount + "/" + info.MaxPlayers;
            MaxKillText.text = string.Format("{0} {1}", info.CustomProperties[PropertiesKeys.RoomGoal], info.GetGameMode().GetModeInfo().GoalName);
            PingText.text = ((int)info.CustomProperties[PropertiesKeys.MaxPing]).ToString() + " ms";
            bool _active = (info.PlayerCount < info.MaxPlayers) ? true : false;
            PrivateUI.SetActive((string.IsNullOrEmpty((string)cacheInfo.CustomProperties[PropertiesKeys.RoomPassword]) == false));
            JoinButton.SetActive(_active);
            FullText.SetActive(!_active);
        }

        /// <summary>
        /// Join to the room that this UI Row represent
        /// </summary>
        public void JoinRoom()
        {
            //If the local player ping is higher than the max allowed in the room
            if (PhotonNetwork.GetPing() >= (int)cacheInfo.CustomProperties[PropertiesKeys.MaxPing])
            {
                //display the message and Don't join to the room
                bl_LobbyUI.Instance.ShowPopUpWindow("max room ping");
                return;
            }

            //if the room doesn't require a password
            if (string.IsNullOrEmpty((string)cacheInfo.CustomProperties[PropertiesKeys.RoomPassword]))
            {
                bl_LobbyUI.Instance.blackScreenFader.FadeIn(1);
                if (cacheInfo.PlayerCount < cacheInfo.MaxPlayers)
                {
                    PhotonNetwork.JoinRoom(cacheInfo.Name);
                }
            }
            else
            {
                bl_LobbyUI.Instance.CheckRoomPassword(cacheInfo);
            }
        }
    }
}