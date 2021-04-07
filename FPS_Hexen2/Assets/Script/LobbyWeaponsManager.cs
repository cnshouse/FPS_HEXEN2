using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyWeaponsManager : MonoBehaviour
{
	public GameObject[] _LobbyWeapons;
    public PlayerClass _player;
	private MFPS.ClassCustomization.bl_ClassCustomize cClass;

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
#if UNITY_EDITOR
		if (cClass != null)
		{
			cClass.RefreshLists();
		}
#endif
		//PlayerPrefs.SetString("LastEquipedWeapon", WeaponName);
		// MFPS.ClassCustomization.bl_ClassCustomize.Instance.RefreshLists();
	}

	public void OnEnable()
	{
        string _weaponName = PlayerPrefs.GetString("LastEquipedWeapon");
        
        if(_weaponName != null && _weaponName != "")
            ChangePrimarySlot(_weaponName);

		cClass = FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>();
    }

}
