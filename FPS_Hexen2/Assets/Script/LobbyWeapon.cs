using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MFPS.Addon.Customizer;

[System.Serializable]
public class LobbyWeapon:MonoBehaviour
{
	public string _WeaponName;
    public RuntimeAnimatorController _animationController;
	public GameObject _Weapon;
    private int[] AttachmentsIds = new int[] { 0, 0, 0, 0, 0 };

    public MFPS.Addon.Customizer.CustomizerCamoRender _Mesh;
    public CustomizerAttachments Attachments;
    private int _Camo;

	public void Start()
	{
        LoadAttachmentsForWeapon(_WeaponName);
    }

	private void OnEnable()
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
        AttachmentsIds = array;
        Apply(AttachmentsIds);
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

    public void SetAnimation()
	{
        GameObject LobbyPlayer = FindObjectOfType<Animator>().gameObject;
        RuntimeAnimatorController _curAC = LobbyPlayer.GetComponent<Animator>().runtimeAnimatorController;
        //Debug.Log("Player is : " + LobbyPlayer.name);
        if (_animationController != null && _curAC != _animationController)
        {
            LobbyPlayer.GetComponent<Animator>().runtimeAnimatorController = _animationController;
        }
    }

    public void Apply(int[] array)
    {
        Attachments.Suppressers.ForEach(x => { SetActive(x.Model, false); });
        Attachments.Sights.ForEach(x => { SetActive(x.Model, false); });
        Attachments.Foregrips.ForEach(x => { SetActive(x.Model, false); });
        Attachments.Magazines.ForEach(x => { SetActive(x.Model, false); });

        ActiveModelInList(Attachments.Suppressers, array[0]);
        ActiveModelInList(Attachments.Sights, array[1]);
        ActiveModelInList(Attachments.Foregrips, array[2]);
        ActiveModelInList(Attachments.Magazines, array[3]);
    }

    void ActiveModelInList(List<CustomizerModelInfo> list, int id, bool active = true)
    {
        if (list == null || id >= list.Count || list[id].Model == null) return;
        list[id].Model.SetActive(active);
    }

    void SetActive(GameObject obj, bool active)
    {
        if (obj == null) return;
        obj.SetActive(active);
    }

}
