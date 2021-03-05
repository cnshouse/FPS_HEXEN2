using UnityEngine;
namespace MFPS.Addon.PlayerSelector
{
    [System.Serializable]
    public class bl_PlayerSelectorInfo
    {
        public string Name;
        public int Price;
        public Sprite Preview;
        public GameObject Prefab;
        //Lobby prefab;
        public GameObject LobbyPrefab;

        [HideInInspector] public int ID;
        [HideInInspector] public Team team;

        public bool isEquipedOne()
        {
            return bl_PlayerSelectorData.GetTeamOperatorID(team) == ID;
        }
    }
}