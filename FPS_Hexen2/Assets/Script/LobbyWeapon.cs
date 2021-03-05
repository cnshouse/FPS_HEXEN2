using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LobbyWeapon:MonoBehaviour
{
	public string _WeaponName;
	public GameObject _Weapon;
	public MFPS.Addon.Customizer.CustomizerCamoRender _Mesh;
    private int _Camo;

	public void Start()
	{
        LoadAttachmentsForWeapon(_WeaponName);
    }

	public int[] LoadAttachmentsForWeapon(string _WeaponName)
    {
        int[] array = new int[5] { 0, 0, 0, 0, 0 };
        var key = GetWeaponKey(_WeaponName);
        if (PlayerPrefs.HasKey(key))
        {
            string t = PlayerPrefs.GetString(key);
            array = DecompileLine(t);
            Debug.Log("Line: " + t);
        }
        return array;
    }

    public int[] DecompileLine(string line)
    {
        int[] array = new int[5] { 0, 0, 0, 0, 0 };
        string[] split = line.Split(","[0]);
        array[0] = int.Parse(split[0]);
        array[1] = int.Parse(split[1]);
        array[2] = int.Parse(split[2]);
        array[3] = int.Parse(split[3]);
        array[4] = int.Parse(split[4]);
        _Camo = array[4];
        LoadCamo(_Camo);
        return array;
    }

    public string GetWeaponKey(string wname)
    {
        string t = string.Format("{0}.{1}.cmz.att.{2}", Application.companyName, Application.productName, wname);
        return t;
    }

    public void LoadCamo(int _int)
	{
        _Mesh.ApplyCamo(_WeaponName, _int);
    }


}
