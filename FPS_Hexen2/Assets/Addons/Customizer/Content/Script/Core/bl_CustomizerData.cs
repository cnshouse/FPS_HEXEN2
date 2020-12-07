using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class bl_CustomizerData : ScriptableObject
{
    public List<CustomizerInfo> Weapons = new List<CustomizerInfo>();
    [Space(10)]
    public List<GlobalCamo> GlobalCamos = new List<GlobalCamo>();

    [HideInInspector] public int OpenedWeapon = 0;
    public const string CURRENT_CUSTOMIZER = "cmz.cweaponid";

    public CustomizerInfo GetWeapon(string weaponName)
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i].WeaponName == weaponName)
            {
                return Weapons[i];
            }
        }
        return null;
    }

    public int GetCustomizerID(int GunID)
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            if (Weapons[i].GunID == GunID) return i;
        }
        return 0;
    }

    public string[] GetWeaponStringArray()
    {
        string[] array = new string[Weapons.Count];
        for (int i = 0; i < Weapons.Count; i++)
        {
            array[i] = Weapons[i].WeaponName;
        }
        return array;
    }

    public int[] LoadAttachmentsForWeapon(string weapon)
    {
        int[] array = new int[5] { 0, 0, 0, 0, 0 };
        if (PlayerPrefs.HasKey(GetWeaponKey(weapon)))
        {
            string t = PlayerPrefs.GetString(GetWeaponKey(weapon));
            array = DecompileLine(t);
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
        return array;
    }

    public string CompileArray(int[] array)
    {
        string line = string.Join(",", array.Select(x => x.ToString()).ToArray());
        return line;
    }

    public void SaveAttachmentsForWeapon(string weapon, int[] ids)
    {
        string t = GetWeaponKey(weapon);
        string line = string.Join(",", ids.Select(x => x.ToString()).ToArray());
        PlayerPrefs.SetString(t, line);
        PlayerPrefs.SetInt(CURRENT_CUSTOMIZER, Weapons.FindIndex(x => x.WeaponName == weapon));
    }

    public string GetWeaponKey(string wname)
    {
        string t = string.Format("cmz.att.{0}", wname);
        return t;
    }

    public string[] GetGlobalCamosStringArray()
    {
        List<string> list = new List<string>();
        list = GlobalCamos.Select(x => x.Name).ToList();
        return list.ToArray();
    }
    /// <summary>
    /// 
    /// </summary>
    void OnValidate()
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            for (int e = 0; e < Weapons[i].Camos.Count; e++)
            {
                Weapons[i].Camos[e].ID = e;
                Weapons[i].Camos[e].ofWeaponID = i;
            }
        }
    }

    private static bl_CustomizerData _data;
    public static bl_CustomizerData Instance
    {
        get
        {
            if(_data == null)
            {
                _data = Resources.Load<bl_CustomizerData>("CustomizerData");
            }
            return _data;
        }
    }
}