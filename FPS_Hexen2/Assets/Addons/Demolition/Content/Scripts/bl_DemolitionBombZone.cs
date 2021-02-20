using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.GameModes.Demolition
{
    public class bl_DemolitionBombZone : MonoBehaviour
    {
        public string ZoneName = "A";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(bl_PlayerSettings.LocalTag))
            {
                bl_Demolition.Instance.OnEnterInZone(true, this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(bl_PlayerSettings.LocalTag))
            {
                bl_Demolition.Instance.OnEnterInZone(false, this);
            }
        }
    }
}