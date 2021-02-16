using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MFPS.Runtime.Settings;
using System;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class bl_MFPS
{
    public static float MusicVolume = 1;

    public static bl_RuntimeSettingsProfile Settings => bl_GameData.Instance.RuntimeSettings;
    public static bl_PlayerReferences LocalPlayerReferences => bl_GameManager.Instance.LocalPlayerReferences;
    public static List<bl_GunInfo> AllWeapons => bl_GameData.Instance.AllWeapons;

    public const string LOCAL_PLAYER_TAG = "Player";
    public const string REMOTE_PLAYER_TAG = "Remote";
    public const string AI_TAG = "AI";
    public const string HITBOX_TAG = "BodyPart";

    /// <summary>
    /// Class helper with some useful function referenced to the local player only.
    /// </summary>
    public static class LocalPlayer
    {
        public static int ViewID => bl_GameManager.LocalPlayerViewID;

        /// <summary>
        /// Make the local player die
        /// </summary>
        public static void Suicide(bool increaseWarnings = true)
        {
            if (!PhotonNetwork.InRoom || bl_GameManager.Instance.LocalPlayerReferences == null) return;

            var pdm = bl_GameManager.Instance.LocalPlayerReferences.playerHealthManager;
            pdm.Suicide();
            bl_UtilityHelper.LockCursor(true);
            if(increaseWarnings)
            bl_GameManager.SuicideCount++;
            //if player is a joker o abuse of suicide, them kick of room
            if (bl_GameManager.SuicideCount > bl_GameData.Instance.maxSuicideAttempts)
            {              
                bl_GameManager.isLocalAlive = false;
                bl_UtilityHelper.LockCursor(false);
                bl_RoomMenu.Instance.LeftOfRoom();
            }
        }

        /// <summary>
        /// Make the local player ignore colliders
        /// </summary>
        public static void IgnoreColliders(Collider[] colliders, bool ignore)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.bodyPartManager.IgnoreColliders(colliders, ignore);
        }

        /// <summary>
        /// Make the player detect an object when the camera look at it
        /// </summary>
        public static byte AddCameraRayDetection(string objectName, Action<bool> callback, byte id)
        {
            if (LocalPlayerReferences == null) return 0;

            return LocalPlayerReferences.cameraRay.AddTrigger(objectName, callback, id);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveCameraDetection(string objectName, byte id)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.cameraRay.RemoveTrigger(objectName, id);
        }
    }

    public static class Network
    {
        /// <summary>
        /// Send a RPC-like call (without a Photon view required) to all other clients in the same room.
        /// </summary>
        public static void SendNetworkCall(byte code, Hashtable data) => bl_PhotonNetwork.Instance.SendDataOverNetwork(code, data);
    }
}