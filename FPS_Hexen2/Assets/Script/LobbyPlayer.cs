using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LobbyPlayer 
{
	public GameObject ActiveLobbyPlayerPrefab;
	public GameObject[] LobbyPlayerSkins;
	public Sprite HeroImage;
	public int UniqueLobbyPlayerNumber;
	public string LobbyPlayerHeroName;

}
