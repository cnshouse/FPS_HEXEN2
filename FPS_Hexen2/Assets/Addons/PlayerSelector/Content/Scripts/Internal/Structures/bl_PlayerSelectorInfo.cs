using UnityEngine;
namespace MFPS.PlayerSelector
{
    [System.Serializable]
    public class bl_PlayerSelectorInfo
    {
        public string Name;
        public int Price;
        public int Hero; //this will over ride the players class as a hero;
        public Sprite Preview;
        public GameObject Prefab;

        [HideInInspector] public int ID;
        [HideInInspector] public Team team;

        public bool isEquipedOne()
        {
            return bl_PlayerSelectorData.GetTeamOperatorID(team) == ID;
        }
    }
}