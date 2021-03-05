using UnityEngine;
using System;
#if UNITY_EDITOR
using MFPSEditor;
#endif
using MFPS.ClassCustomization;

public class bl_ClassManager : ScriptableObject {

    [Header("Player Class")]
    public PlayerClass m_Class = PlayerClass.Assault;
    //When if new player and not have data saved
    //take this weapons ID for default
    [Header("Assault")]
    public bl_PlayerClassLoadout DefaultAssaultClass;
    [Header("Engineer")]
    public bl_PlayerClassLoadout DefaultEngineerClass;
    [Header("Support")]
    public bl_PlayerClassLoadout DefaultSupportClass;
    [Header("Recon")]
    public bl_PlayerClassLoadout DefaultReconClass;
    [Header("Dragos")]
    public bl_PlayerClassLoadout DefaultDragosClass;
    [Header("Angel")]
    public bl_PlayerClassLoadout DefaultAngelClass;
    [Header("Shogun")]
    public bl_PlayerClassLoadout DefaultShogunClass;
    [Header("Scarlett")]
    public bl_PlayerClassLoadout DefaultScarlettClass;

#if UNITY_EDITOR
    [Space(10)]
    [InspectorButton("DeleteKeys",ButtonWidth = 150)] public bool DeleteSavedClasses;
#endif

    public bl_PlayerClassLoadout AssaultClass { get; set; }
    public bl_PlayerClassLoadout EngineerClass { get; set; }
    public bl_PlayerClassLoadout SupportClass { get; set; }
    public bl_PlayerClassLoadout ReconClass { get; set; }
    public bl_PlayerClassLoadout DragosClass { get; set; }
    public bl_PlayerClassLoadout AngelClass { get; set; }
    public bl_PlayerClassLoadout ShogunClass { get; set; }
    public bl_PlayerClassLoadout ScarlettClass { get; set; }


    [HideInInspector] public int ClassKit = 0;
    public const string LOADOUT_KEY_FORMAT = "mfps.loadout.{0}";

    /// <summary>
    /// 
    /// </summary>
    public void DeleteKeys()
    {
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Assault));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Engineer));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Recon));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Support));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Dragos));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Angel));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Shogun));
        PlayerPrefs.DeleteKey(string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Scarlett));
    }

    /// <summary>
    /// 
    /// </summary>
    void Awake()
    {
        Init();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Init()
    {
        GetID();
    }

    /// <summary>
    /// 
    /// </summary>
    void GetID()
    {
        int c = 0;
        if (PlayerPrefs.HasKey(ClassKey.ClassType)) { c = PlayerPrefs.GetInt(ClassKey.ClassType); }
        switch (c)
        {
            case 0 :
                m_Class = PlayerClass.Assault;
                break;
            case 1:
                m_Class = PlayerClass.Engineer;
                break;
            case 2:
                m_Class = PlayerClass.Support;
                break;
            case 3:
                m_Class = PlayerClass.Recon;
                break;
            case 4:
                m_Class = PlayerClass.Dragos;
                break;
            case 5:
                m_Class = PlayerClass.Angel;
                break;
            case 6:
                m_Class = PlayerClass.Shogun;
                break;
            case 7:
                m_Class = PlayerClass.Scarlett;
                break;
        }

#if ULSP
        if(bl_DataBase.Instance != null)
        {
            ClassKit = bl_DataBase.Instance.LocalUser.metaData.rawData.ClassKit;
            string dbData = bl_DataBase.Instance.LocalUser.metaData.rawData.WeaponsLoadouts;
            if (!string.IsNullOrEmpty(dbData))
            {
                AssaultClass = Instantiate(DefaultAssaultClass);
                AssaultClass.FromString(dbData, 0);

                EngineerClass = Instantiate(DefaultEngineerClass);
                EngineerClass.FromString(dbData, 1);

                ReconClass = Instantiate(DefaultReconClass);
                ReconClass.FromString(dbData, 2);

                SupportClass = Instantiate(DefaultSupportClass);
                SupportClass.FromString(dbData, 3);

                DragosClass = Instantiate(DefaultDragosClass);
                DragosClass.FromString(dbData, 4);

                AngelClass = Instantiate(DefaultAngelClass);
                AngelClass.FromString(dbData, 4);

                ShogunClass = Instantiate(DefaultShogunClass);
                ShogunClass.FromString(dbData, 5);

                ScarlettClass = Instantiate(DefaultScarlettClass);
                ScarlettClass.FromString(dbData, 6);
                return;
            }
        }
        else
        {
            Debug.Log("Use local data since player is not logged yet.");
        }
#endif

        ClassKit = PlayerPrefs.GetInt(ClassKey.ClassKit, 0);

        string format = LOADOUT_KEY_FORMAT;
        string key = string.Format(format, PlayerClass.Assault);

        string data = PlayerPrefs.GetString(key, DefaultAssaultClass.ToString());
        AssaultClass = Instantiate(DefaultAssaultClass);
        AssaultClass.FromString(data);

        key = string.Format(format, PlayerClass.Engineer);
        data = PlayerPrefs.GetString(key, DefaultEngineerClass.ToString());
        EngineerClass = Instantiate(DefaultEngineerClass);
        EngineerClass.FromString(data);

        key = string.Format(format, PlayerClass.Recon);
        data = PlayerPrefs.GetString(key, DefaultReconClass.ToString());
        ReconClass = Instantiate(DefaultReconClass);
        ReconClass.FromString(data);

        key = string.Format(format, PlayerClass.Support);
        data = PlayerPrefs.GetString(key, DefaultSupportClass.ToString());
        SupportClass = Instantiate(DefaultSupportClass);
        SupportClass.FromString(data);

        key = string.Format(format, PlayerClass.Dragos);
        data = PlayerPrefs.GetString(key, DefaultDragosClass.ToString());
        DragosClass = Instantiate(DefaultDragosClass);
        DragosClass.FromString(data);

        key = string.Format(format, PlayerClass.Angel);
        data = PlayerPrefs.GetString(key, DefaultAngelClass.ToString());
        AngelClass = Instantiate(DefaultAngelClass);
        AngelClass.FromString(data);

        key = string.Format(format, PlayerClass.Shogun);
        data = PlayerPrefs.GetString(key, DefaultShogunClass.ToString());
        ShogunClass = Instantiate(DefaultShogunClass);
        ShogunClass.FromString(data);

        key = string.Format(format, PlayerClass.Scarlett);
        data = PlayerPrefs.GetString(key, DefaultScarlettClass.ToString());
        ScarlettClass = Instantiate(DefaultScarlettClass);
        ScarlettClass.FromString(data);

    }

    public void SetUpClasses(bl_GunManager gm)
    {
        if (AssaultClass == null) { Init(); }

        bl_PlayerClassLoadout pcl = null;
        switch (m_Class)
        {
            case PlayerClass.Assault:
                pcl = AssaultClass;
                break;
            case PlayerClass.Recon:
                pcl = ReconClass;
                break;
            case PlayerClass.Engineer:
                pcl = EngineerClass;
                break;
            case PlayerClass.Support:
                pcl = SupportClass;
                break;
            case PlayerClass.Dragos:
                pcl = DragosClass;
                break;
            case PlayerClass.Angel:
                pcl = AngelClass;
                break;
            case PlayerClass.Shogun:
                pcl = ShogunClass;
                break;
            case PlayerClass.Scarlett:
                pcl = ScarlettClass;
                break;
        }

        if (pcl == null)
        {
            Debug.LogError($"Player Class Loadout has not been assigned for the class {m_Class.ToString()}");
            return;
        }

        gm.PlayerEquip[0] = gm.GetGunOnListById(pcl.Primary);
        gm.PlayerEquip[1] = gm.GetGunOnListById(pcl.Secondary);
        gm.PlayerEquip[2] = gm.GetGunOnListById(pcl.Letal);
        gm.PlayerEquip[3] = gm.GetGunOnListById(pcl.Perks);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SaveClass(Action callBack = null)
    {
#if ULSP
        if (bl_DataBase.Instance != null)
        {
            string dbdata = $"{AssaultClass.ToString()},{EngineerClass.ToString()},{ReconClass.ToString()},{SupportClass.ToString()},{DragosClass.ToString()},{AngelClass.ToString()},{ShogunClass.ToString()},{ScarlettClass.ToString()}";
            bl_DataBase.Instance.LocalUser.metaData.rawData.WeaponsLoadouts = dbdata;
            bl_DataBase.Instance.LocalUser.metaData.rawData.ClassKit = ClassKit;
            bl_DataBase.Instance.SaveUserMetaData(() => { callBack?.Invoke(); });
        }
        else
            callBack?.Invoke();
#else
        callBack?.Invoke();
#endif
        string key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Assault);
        string data = AssaultClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Engineer);
        data = EngineerClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Recon);
        data = ReconClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Support);
        data = SupportClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Dragos);
        data = DragosClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Angel);
        data = AngelClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Shogun);
        data = ShogunClass.ToString();
        PlayerPrefs.SetString(key, data);

        key = string.Format(LOADOUT_KEY_FORMAT, PlayerClass.Scarlett);
        data = ScarlettClass.ToString();
        PlayerPrefs.SetString(key, data);

        PlayerPrefs.SetInt(ClassKey.ClassKit, ClassKit);
    }

    public bool isEquiped(int gunID, PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Assault:
                return (AssaultClass.Primary == gunID || AssaultClass.Secondary == gunID || AssaultClass.Perks == gunID || AssaultClass.Letal == gunID);
            case PlayerClass.Recon:
                return (ReconClass.Primary == gunID || ReconClass.Secondary == gunID || ReconClass.Perks == gunID || ReconClass.Letal == gunID);
            case PlayerClass.Engineer:
                return (EngineerClass.Primary == gunID || EngineerClass.Secondary == gunID || EngineerClass.Perks == gunID || EngineerClass.Letal == gunID);
            case PlayerClass.Support:
                return (SupportClass.Primary == gunID || SupportClass.Secondary == gunID || SupportClass.Perks == gunID || SupportClass.Letal == gunID);
            case PlayerClass.Dragos:
                return (DragosClass.Primary == gunID || DragosClass.Secondary == gunID || DragosClass.Perks == gunID || DragosClass.Letal == gunID);
            case PlayerClass.Angel:
                return (AngelClass.Primary == gunID || AngelClass.Secondary == gunID ||AngelClass.Perks == gunID || AngelClass.Letal == gunID);
            case PlayerClass.Shogun:
                return (ShogunClass.Primary == gunID || ShogunClass.Secondary == gunID || ShogunClass.Perks == gunID || ShogunClass.Letal == gunID);
            case PlayerClass.Scarlett:
                return (ScarlettClass.Primary == gunID || ScarlettClass.Secondary == gunID || ScarlettClass.Perks == gunID || ScarlettClass.Letal == gunID);
        }
        return false;
    }

    private static bl_ClassManager _instance;
    public static bl_ClassManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<bl_ClassManager>("ClassManager") as bl_ClassManager;
            }
            return _instance;
        }
    }
}