using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyWeaponsManager : MonoBehaviour
{
	public GameObject[] _LobbyWeapons;
    public PlayerClass _player;

    public void ChangePrimarySlot(string WeaponName)
	{
        Debug.Log("Enable Weapon: " + WeaponName);
		foreach(GameObject lw in _LobbyWeapons)
		{
			if (lw.GetComponent<LobbyWeapon>()._WeaponName == WeaponName)
			{
				PlayerPrefs.SetString("LastEquipedWeapon", WeaponName);
				lw.SetActive(true);
				lw.GetComponent<LobbyWeapon>().SetAnimation();
				continue;
			}
			
			lw.SetActive(false);
            
		}
        //PlayerPrefs.SetString("LastEquipedWeapon", WeaponName);
        MFPS.ClassCustomization.bl_ClassCustomize.Instance.RefreshLists();
    }

	public void OnEnable()
	{
        string _weaponName = PlayerPrefs.GetString("LastEquipedWeapon");
        
        if(_weaponName != null && _weaponName != "")
            ChangePrimarySlot(_weaponName);
    }

}
