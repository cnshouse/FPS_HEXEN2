using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
using HashTable = ExitGames.Client.Photon.Hashtable;

namespace MFPS.GameModes.Demolition
{
    public class bl_DemolitionBomb : MonoBehaviour
    {
        public BombStatus bombStatus = BombStatus.Initial;
        public LayerMask layerMask;
        [HideInInspector] public Vector3 InitialPosition;
        [HideInInspector] public Vector3 InitialRotation;

        [Header("On Carrier Position")]
        public Vector3 onCarrierPosition;
        public Vector3 onCarrierRotation;
        public GameObject bombModel;
        public GameObject bombEffects;
        public Texture2D bombIcon;
        public AudioClip pickUpSound;

        public float plantedTime { get; set; }
        private bool visibleForLocal = false;
        private Collider[] colliders;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            if (bl_PhotonNetwork.Instance == null) return;

            //use custom event system instead of RPC
            bl_PhotonNetwork.Instance.AddCallback(PropertiesKeys.DMBombEvent, this.OnEventReceived);
            colliders = transform.GetComponentsInChildren<Collider>();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            if (bl_PhotonNetwork.Instance == null) return;

            bl_PhotonNetwork.Instance.RemoveCallback(this.OnEventReceived);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetToInit()
        {
            bombStatus = BombStatus.Initial;
            transform.parent = null;
            transform.position = InitialPosition;
            transform.eulerAngles = InitialRotation;
            bombModel.SetActive(true);
            SetColliderActive(true);
            visibleForLocal = PhotonNetwork.LocalPlayer.GetPlayerTeam() == bl_Demolition.Instance.terroristsTeam;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(bl_PlayerSettings.LocalTag))
            {
                PhotonView pv = other.GetComponent<PhotonView>();
                if (pv == null) return;
                if (bombStatus == BombStatus.Initial || bombStatus == BombStatus.Droped)//if the bomb is in the ground
                {
                    if (pv.Owner.GetPlayerTeam() == bl_Demolition.Instance.terroristsTeam)//and picked by a terrorist player
                    {
                        var data = bl_UtilityHelper.CreatePhotonHashTable();
                        data.Add("type", (int)BombStatus.Carried);
                        data.Add("carrierID", pv.Owner.ActorNumber);
                        data.Add("viewID", pv.ViewID);
                        bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DMBombEvent, data);
                        if (pickUpSound != null) { AudioSource.PlayClipAtPoint(pickUpSound, transform.position); }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnLocalPlant()
        {
            Transform camera = bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerSettings>().PlayerCamera.transform;
            Vector3 dir = camera.forward;
            Vector3 origin = camera.position;
            RaycastHit hit;
            Vector3 finalPoint = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            Ray r = new Ray(origin, dir);
            bombStatus = BombStatus.Actived;

            //check if there is a object in front of the player camera where stick the bomb
            if (Physics.Raycast(r, out hit, 3, layerMask, QueryTriggerInteraction.Ignore))
            {
                finalPoint = hit.point;
                rot = Quaternion.LookRotation(hit.normal, Vector3.up);
            }
            else//otherwise just instance the bomb in front of the player position
            {
                r = new Ray(origin + dir, Vector3.down);
                if (Physics.Raycast(r, out hit, 10, layerMask, QueryTriggerInteraction.Ignore))
                {
                    finalPoint = hit.point;
                    rot = Quaternion.LookRotation(hit.normal, Vector3.up);
                }
            }
            //send the bomb plant event to all players
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("type", (int)BombStatus.Actived);
            data.Add("position", finalPoint);
            data.Add("rotation", rot);
            data.Add("time", (float)PhotonNetwork.Time);
            data.Add("zone", bl_Demolition.Instance.plantingZone.ZoneName);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DMBombEvent, data);

            bl_KillFeed.Instance.SendTeamHighlightMessage(PhotonNetwork.LocalPlayer.NickName, string.Format("has planted the bomb in <b>{0}</b>", bl_Demolition.Instance.plantingZone.ZoneName), PhotonNetwork.LocalPlayer.GetPlayerTeam());
            bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerNetwork>().gunManager.ReleaseWeapons(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnLocalDefuse()
        {
            //send the bomb plant event to all players
            var data = bl_UtilityHelper.CreatePhotonHashTable();
            data.Add("type", (int)BombStatus.Defused);
            bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DMBombEvent, data);

            bl_KillFeed.Instance.SendTeamHighlightMessage(PhotonNetwork.LocalPlayer.NickName, "has defuse the bomb!", PhotonNetwork.LocalPlayer.GetPlayerTeam());
            bl_GameManager.Instance.LocalPlayer.GetComponent<bl_PlayerNetwork>().gunManager.ReleaseWeapons(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(bl_PlayerSettings.LocalTag))
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnEventReceived(ExitGames.Client.Photon.Hashtable data)
        {
            BombStatus getStatus = (BombStatus)(int)data["type"];
            if (getStatus == BombStatus.Carried)
            {
                PickUp(data);
            }
            else if (getStatus == BombStatus.Droped)
            {
                Drop(data);
            }
            else if (getStatus == BombStatus.Actived)
            {
                Plant(data);
            }
            else if (getStatus == BombStatus.Defused)
            {
                Defuse(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void PickUp(ExitGames.Client.Photon.Hashtable data)
        {
            bombStatus = BombStatus.Carried;
            int actorID = (int)data["carrierID"];
            int viewID = (int)data["viewID"];
            bl_DemolitionBombManager.Instance.carrierActorNumber = actorID;
            //if local player is the carrier
            if (actorID == PhotonNetwork.LocalPlayer.ActorNumber)
            {
                bl_DemolitionUI.Instance.BombCarrierUI.SetActive(true);
                bombModel.SetActive(false);
            }
            else
            {

                PhotonView carrierView = bl_GameManager.Instance.FindPlayerView(viewID);
                if (carrierView == null) { Debug.LogError("Couldn't find the carrier view"); return; }

                Transform carrierParent = carrierView.GetComponent<bl_PlayerSettings>().carrierPoint;
                transform.parent = carrierParent;
                transform.localPosition = onCarrierPosition;
                transform.localEulerAngles = onCarrierRotation;
            }
            SetColliderActive(false);
            bl_DemolitionUI.Instance.pickUpIndicationUI.SetActive(false);
            bombEffects.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Drop(ExitGames.Client.Photon.Hashtable data)
        {
            bombModel.SetActive(true);
            Vector3 pos = (Vector3)data["position"];
            transform.parent = null;
            transform.position = pos;
            transform.rotation = (Quaternion)data["rotation"];
            bombStatus = BombStatus.Droped;
            bl_DemolitionBombManager.Instance.carrierActorNumber = -1;
            bombEffects.SetActive(false);
            SetColliderActive(true);
            bl_DemolitionUI.Instance.BombCarrierUI.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        void Plant(ExitGames.Client.Photon.Hashtable data)
        {
            bombStatus = BombStatus.Actived;
            bombModel.SetActive(true);
            Vector3 position = (Vector3)data["position"];
            Quaternion rot = (Quaternion)data["rotation"];
            plantedTime = (float)data["time"];
            transform.parent = null;
            transform.position = position;
            transform.rotation = rot;
            bl_DemolitionBombManager.Instance.StartDetonationCountDown(true);
            bl_DemolitionBombManager.Instance.carrierActorNumber = -1;
            bombEffects.SetActive(true);
            bl_DemolitionUI.Instance.onPlantUI.GetComponentInChildren<Text>().text = string.Format("BOMB HAS BEEN PLANTED IN <b>{0}</b>\n<size=8>{1} SECONDS BEFORE DETONATION</size>", (string)data["zone"], bl_Demolition.Instance.DetonationTime);
            bl_DemolitionUI.Instance.onPlantUI.SetActive(true);
            bl_DemolitionUI.Instance.BombCarrierUI.SetActive(false);
            SetColliderActive(true);
            bl_Demolition.Instance.virtualAudioController.PlayClip("bomb planted");
            bl_Demolition.Instance.virtualAudioController.PlayClip("planted loop");
            bl_MatchTimeManager.Instance.PauseTime();
        }

        /// <summary>
        /// 
        /// </summary>
        void Defuse(HashTable data)
        {
            if (bombStatus == BombStatus.Defused) return;

            bombStatus = BombStatus.Defused;
            bombEffects.SetActive(false);
            bl_DemolitionBombManager.Instance.CancelDefuse(false);
            bl_Demolition.Instance.virtualAudioController.StopClip("planted loop");
            bl_Demolition.Instance.virtualAudioController.PlayClip("bomb defuse");
            if (PhotonNetwork.IsMasterClient)
            {
                bl_PhotonNetwork.Instance.SendDataOverNetwork(PropertiesKeys.DemolitionEvent, new HashTable()
                {
                {"type", bl_Demolition.DemolitionEventType.RoundFinish },
                {"finalType",1 },
                {"winner",bl_Demolition.Instance.terroristsTeam.OppsositeTeam() }
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            if (!visibleForLocal) return;
            if (bombStatus != BombStatus.Droped && bombStatus != BombStatus.Initial) return;

            if (bl_GameManager.Instance.CameraRendered)
            {
                Vector3 vector = bl_GameManager.Instance.CameraRendered.WorldToScreenPoint(transform.position);
                if (vector.z > 0)
                {
                    GUI.DrawTexture(new Rect(vector.x - 15, (Screen.height - vector.y) - 15, 30, 30), bombIcon);
                }
            }
        }

        void SetColliderActive(bool active)
        {
            if (colliders == null || colliders.Length <= 0)
            {
                colliders = transform.GetComponentsInChildren<Collider>();
            }
            foreach (var item in colliders)
            {
                if (item != null)
                    item.enabled = active;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InitialPosition = transform.position;
            InitialRotation = transform.eulerAngles;
        }
#endif

        public bool isAvailableToPlant { get { return bombStatus == BombStatus.Carried; } }
        public bool isAvailableToDefuse { get { return bombStatus == BombStatus.Actived; } }

        [System.Serializable]
        public enum BombStatus
        {
            Initial = 0,
            Carried = 1,
            Droped = 2,
            Actived = 3,
            Defused = 4,
        }
    }
}