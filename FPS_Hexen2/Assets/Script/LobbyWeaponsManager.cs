using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyWeaponsManager : MonoBehaviour
{
	public GameObject[] _LobbyWeapons;

    public void ChangePrimarySlot(string WeaponName)
	{
        Debug.Log("Enable Weapon: " + WeaponName);
		foreach(GameObject lw in _LobbyWeapons)
		{
			if (lw.GetComponent<LobbyWeapon>()._WeaponName == WeaponName)
			{
				lw.SetActive(true);
				lw.GetComponent<LobbyWeapon>().SetAnimation();
				continue;
			}
			
			lw.SetActive(false);
            
		}
        PlayerPrefs.SetString("LastEquipedWeapon", WeaponName);
        MFPS.ClassCustomization.bl_ClassCustomize.Instance.RefreshLists();
    }

	public void OnEnable()
	{
        string _weaponName = PlayerPrefs.GetString("LastEquipedWeapon");
        
        if(_weaponName != null && _weaponName != "")
            ChangePrimarySlot(_weaponName);
    }


	public void SetInitWeapon(PlayerClass _class)
	{
        //PlayerClass playerClass = bl_ClassManager.Instance.m_Class;
        //int _currentWeapon = isEquiped(playerClass);
    //    string _weaponName = getPrimaryWeaponName(_class); //MFPS.ClassCustomization.bl_ClassCustomize.Instance.assaultWeapons.AllWeapons[_currentWeapon].Info.Name;
    //    ChangePrimarySlot(_weaponName);
        //PlayerPrefs.SetString("LastEquipedWeapon", _weaponName);
        Debug.Log("Fetching Class: " + _class.ToString());
    }

    public string getPrimaryWeaponName(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Assault:
                return (bl_ClassManager.Instance.AssaultClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Recon:
                return (bl_ClassManager.Instance.ReconClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Engineer:
                return (bl_ClassManager.Instance.EngineerClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Support:
                return (bl_ClassManager.Instance.SupportClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Dragos:
                return (bl_ClassManager.Instance.DragosClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Angel:
                return (bl_ClassManager.Instance.AngelClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Shogun:
                return (bl_ClassManager.Instance.ShogunClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Scarlett:
                return (bl_ClassManager.Instance.ScarlettClass.GetPrimaryGunInfo().Name);
            case PlayerClass.Celina:
                return (bl_ClassManager.Instance.CelinaClass.GetPrimaryGunInfo().Name);
        }
        return null;
    }

}
