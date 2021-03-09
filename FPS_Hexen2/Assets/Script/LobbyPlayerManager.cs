using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerManager : MonoBehaviour
{
    private const string PLAYER_HERO_SKIN_KEY = "{0}.heroskin";

    public GameObject CurrentLobbyModel;
    public int _selectedModel_Int =  0;
    public string _selectedModel_name = "";
    

    //Prefab for the Hero Selection Screen
    public GameObject HexPrefab;
    //The Ui Container for the Hex Buttons;
    public GameObject HexContianner;
    //Hero Info
    private MFPS.Addon.PlayerSelector.bl_PlayerSelectorInfo Info;
    public Text HeroName;
    public Image Healthfill;
    public Image Speedfill;
    public Image Regenfill;
    public Image Noisefill;

    private GameObject CurrentlyDisplayedHero;
    public PlayerClass mClass;


    private int[] AttachmentsIds = new int[] { 0, 0, 0, 0, 0 };

    private void Awake()
	{
        string key = string.Format(PLAYER_HERO_SKIN_KEY, Application.productName);
        if (PlayerPrefs.GetString(key) == null) {
            //Debug.Log("No Hero Skin Detected");
		}
		else
		{
            HeroSkin _heroName = GetSavePlayerHero();
            FindHeroSkin(_heroName);
            _selectedModel_name = _heroName.ToString();
            //Debug.Log("Hero found " + _heroName.ToString());
        }

    }


	public void Start()
	{
        //create grid on the UI?
        for (int i = 0; i < MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers.Count; i++)
        {
            GameObject g = Instantiate(HexPrefab, HexContianner.transform);

            g.GetComponent<HeroHex>().HeroInt = i;
            g.GetComponent<HeroHex>().HexImage.sprite = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[i].Preview;
            EventTriggerListener.Get(g).onEnter = OnHoverOverHex;
            EventTriggerListener.Get(g).onClick = OnClickHex;
            //EventTrigger
            g.SetActive(true);

        }
    }

	/// <summary>
	/// Save the active HeroSkin
	/// </summary>
	public static void SavePlayerClass(int _selectedModel_int)
    {
        string key = string.Format(PLAYER_HERO_SKIN_KEY, Application.productName);
        //string CKey = string.Format(PLAYER_HERO_CLASS,)
        PlayerPrefs.SetInt(key, (int)_selectedModel_int);
        //PlayerPrefs.SetInt()
    }

    /// <summary>
    /// Get the locally saved Hero
    /// </summary>

    public static HeroSkin GetSavePlayerHero()
    {
        string key = string.Format(PLAYER_HERO_SKIN_KEY, Application.productName);
        int id = PlayerPrefs.GetInt(key);
        HeroSkin skin = (HeroSkin)id;
        return skin;
    }


    void FindHeroSkin(HeroSkin skin)
    {
        Destroy(CurrentlyDisplayedHero);

        switch (skin)
        {
            case HeroSkin.Dragos:
                _selectedModel_name = skin.ToString();
                CurrentLobbyModel = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[0].LobbyPrefab;
                Info = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayer(Team.All, 0);
                CurrentlyDisplayedHero = Instantiate(CurrentLobbyModel, gameObject.transform.position, gameObject.transform.rotation);
                mClass = PlayerClass.Dragos;
                bl_ClassManager.Instance.m_Class = mClass;
                MFPS.ClassCustomization.bl_ClassCustomize.Instance.ChangeClass(mClass);
                GameObject.FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>()?.TakeCurrentClass(PlayerClass.Dragos);
                Set();
                SavePlayerClass(0);
                break;
            case HeroSkin.Johnny:
                _selectedModel_name = skin.ToString();
                CurrentLobbyModel = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[1].LobbyPrefab;
                Info = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayer(Team.All, 1);
                CurrentlyDisplayedHero = Instantiate(CurrentLobbyModel, gameObject.transform.position, gameObject.transform.rotation);
                mClass = PlayerClass.Assault;
                bl_ClassManager.Instance.m_Class = mClass;
                MFPS.ClassCustomization.bl_ClassCustomize.Instance.ChangeClass(mClass);
                GameObject.FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>()?.TakeCurrentClass(PlayerClass.Assault);
                Set();
                SavePlayerClass(1);
                break;
            case HeroSkin.Angel:
                _selectedModel_name = skin.ToString();
                CurrentLobbyModel = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[2].LobbyPrefab;
                Info = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayer(Team.All, 2);
                CurrentlyDisplayedHero = Instantiate(CurrentLobbyModel, gameObject.transform.position, gameObject.transform.rotation);
                mClass = PlayerClass.Angel;
                bl_ClassManager.Instance.m_Class = mClass;
                MFPS.ClassCustomization.bl_ClassCustomize.Instance.ChangeClass(mClass);
                GameObject.FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>()?.TakeCurrentClass(PlayerClass.Angel);
                Set();
                SavePlayerClass(2);
                break;
            case HeroSkin.Shogun:
                _selectedModel_name = skin.ToString();
                CurrentLobbyModel = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[3].LobbyPrefab;
                Info = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayer(Team.All, 3);
                CurrentlyDisplayedHero = Instantiate(CurrentLobbyModel, gameObject.transform.position, gameObject.transform.rotation);
                mClass = PlayerClass.Shogun;
                bl_ClassManager.Instance.m_Class = mClass;
                MFPS.ClassCustomization.bl_ClassCustomize.Instance.ChangeClass(mClass);
                GameObject.FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>()?.TakeCurrentClass(PlayerClass.Shogun);
                Set();
                SavePlayerClass(3);
                break;
            case HeroSkin.Scarlett:
                _selectedModel_name = skin.ToString();
                CurrentLobbyModel = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[4].LobbyPrefab;
                Info = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayer(Team.All, 4);
                CurrentlyDisplayedHero = Instantiate(CurrentLobbyModel, gameObject.transform.position, gameObject.transform.rotation);
                mClass = PlayerClass.Scarlett;
                bl_ClassManager.Instance.m_Class = mClass;
                MFPS.ClassCustomization.bl_ClassCustomize.Instance.ChangeClass(mClass);
                GameObject.FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>()?.TakeCurrentClass(PlayerClass.Scarlett);
                Set();
                SavePlayerClass(4);
                break;
            case HeroSkin.Celina:
                _selectedModel_name = skin.ToString();
                CurrentLobbyModel = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.AllPlayers[5].LobbyPrefab;
                Info = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayer(Team.All, 5);
                CurrentlyDisplayedHero = Instantiate(CurrentLobbyModel, gameObject.transform.position, gameObject.transform.rotation);
                mClass = PlayerClass.Celina;
                bl_ClassManager.Instance.m_Class = mClass;
                MFPS.ClassCustomization.bl_ClassCustomize.Instance.ChangeClass(mClass);
                GameObject.FindObjectOfType<MFPS.ClassCustomization.bl_ClassCustomize>()?.TakeCurrentClass(PlayerClass.Celina);
                Set();
                SavePlayerClass(5);
                break;
        }
    }


    //Accessing Heroes Data
    public void Set()
    {

        HeroName.text = string.Format(Info.Name.ToUpper());

        if (Info.Prefab != null)
        {
            bl_PlayerHealthManager pdm = Info.Prefab.GetComponent<bl_PlayerHealthManager>();
            bl_FirstPersonController fpc = Info.Prefab.GetComponent<bl_FirstPersonController>();

            Healthfill.fillAmount = pdm.health / 125;
            Speedfill.fillAmount = fpc.WalkSpeed / 5;
            Regenfill.fillAmount = pdm.RegenerationSpeed / 5;
            Noisefill.fillAmount = 0.9f;

            AttachmentsIds = bl_CustomizerData.Instance.LoadAttachmentsForWeapon(_selectedModel_name);
            Debug.Log(_selectedModel_name + " Camo is: " + AttachmentsIds.ToString());
        }
        if (Info.Price > 0 && bl_DataBase.Instance != null)
        {
            int pID = MFPS.Addon.PlayerSelector.bl_PlayerSelectorData.Instance.GetPlayerID(Info.Name);
            bool unlock = bl_DataBase.Instance.LocalUser.ShopData.isItemPurchase(ShopItemType.PlayerSkin, pID);
            Debug.Log("Is Hero Locked? " + unlock.ToString());
     //       LockedUI.SetActive(!unlock);
        }
        //else { LockedUI.SetActive(false); }
    }

	#region Listners
	//On Hover 
	void OnHoverOverHex(GameObject _hex)
	{
        int _int = _hex.GetComponent<HeroHex>().HeroInt;
        HeroSkin skin = (HeroSkin)_int;
        Debug.Log("we are hoving over : " + skin.ToString());
	}
	//On click
    void OnClickHex(GameObject _hex)
	{
        int _int = _hex.GetComponent<HeroHex>().HeroInt;
        HeroSkin skin = (HeroSkin)_int;
        Debug.Log("we have clicked : " + skin.ToString());

        FindHeroSkin(skin);
        _selectedModel_name = skin.ToString();
    }

    //on Exit
    void OnExitHex(HeroHex _hex)
	{
        HeroSkin skin = (HeroSkin)_hex.HeroInt;
        Debug.Log("we are leaving : " + skin.ToString());
    }
    #endregion
}
