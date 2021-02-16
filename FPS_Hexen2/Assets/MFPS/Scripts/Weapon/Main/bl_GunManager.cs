/////////////////////////////////////////////////////////////////////////////////
///////////////////////////bl_GunManager.cs//////////////////////////////////////
/////////////Use this to manage all weapons Player///////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
//////////////////////////////Lovatto Studio/////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class bl_GunManager : bl_MonoBehaviour
{
    #region Public members
    [Header("Weapons List")]
    public List<bl_Gun> AllGuns = new List<bl_Gun>();
    [HideInInspector] public List<bl_Gun> PlayerEquip = new List<bl_Gun>() { null, null, null, null };

    [Header("Player Class")]
    public bl_PlayerClassLoadout m_AssaultClass;
    public bl_PlayerClassLoadout m_EngineerClass;
    public bl_PlayerClassLoadout m_ReconClass;
    public bl_PlayerClassLoadout m_SupportClass;

    [Header("Settings")]
    /// <summary>
    /// ID the weapon to take at start
    /// </summary>
    public int currentWeaponIndex = 0;
    /// <summary>
    /// time it takes to switch weapons
    /// </summary>
    public float SwichTime = 1;
    public float PickUpTime = 2.5f;
    public ChangeWeaponStyle changeWeaponStyle = ChangeWeaponStyle.HideAndDraw;

    [Header("References")]
    public Animator HeadAnimator;
    public Transform TrowPoint = null;
    public AudioClip SwitchFireAudioClip;
    #endregion

    #region Public properties
    public bl_Gun CurrentGun { get; set; }
    public bool CanSwich { get; set; } = true;
    public bool isGameStarted { get; set; }
    public AutoChangeOnPickup EquipPickUpMode { get; set; } = AutoChangeOnPickup.Always;
    #endregion

    #region Private members
    private bl_GunPickUpManager pickupManager;
    private int PreviousGun = 0;
    private bool isFastFire = false;
    public bool ObservedComponentsFoldoutOpen = false;
    AudioSource ASource;
#if GR
    public bool isGunRace { get; set; }
    private bl_GunRace GunRace;
#endif
    #endregion

    /// <summary>
    /// 
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        pickupManager = bl_GunPickUpManager.Instance;
        ASource = GetComponent<AudioSource>();
        isGameStarted = bl_MatchTimeManager.Instance.TimeState == RoomTimeState.Started;
#if GR
        if (transform.root.GetComponent<PhotonView>().IsMine)
        {
            GunRace = FindObjectOfType<bl_GunRace>();
            if (GunRace != null) { GunRace.SetGunManager(this); }
            else { Debug.Log("Gun Race is not integrated in this map, just go to MFPS -> Addons -> Gun Race -> Integrate, with the map scene open)."); }
        }
#endif
        //when player instance select player class select in bl_RoomMenu
        SetupLoadout();
    }

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        //Disable all weapons in children and take the first
        foreach (bl_Gun g in PlayerEquip) { g?.Setup(true); }
        foreach (bl_Gun guns in AllGuns) { guns.gameObject?.SetActive(false); }
        bl_UIReferences.Instance.PlayerUI.LoadoutUI.SetInitLoadout(PlayerEquip);
        EquipPickUpMode = bl_GameData.Instance.switchToPickupWeapon;
#if GR
        if (isGunRace)
        {
            PlayerEquip[0] = GunRace.GetGunInfo(AllGuns);
            currentWeaponIndex = 0;
        }
#endif
        var firstWeapon = PlayerEquip[currentWeaponIndex];
        TakeWeapon(firstWeapon);
        if(firstWeapon != null)
        bl_EventHandler.ChangeWeaponEvent(firstWeapon.GunID);

        if (bl_GameManager.Instance.GameMatchState == MatchState.Waiting && !bl_GameManager.Instance.FirstSpawnDone)
        {
            BlockAllWeapons();
        }
#if LMS
        if (GetGameMode == GameMode.BR)
        {
            UnEquipEverything();
        }
#endif
    }

    /// <summary>
    /// Setup the player equipped weapons from the selected player class
    /// </summary>
    void SetupLoadout()
    {
#if CLASS_CUSTOMIZER
        bl_ClassManager.Instance.SetUpClasses(this);
#else
        //when player instance select player class select in bl_RoomMenu
        bl_PlayerClassLoadout pcl = null;
        var currentClass = PlayerClass.Assault.GetSavePlayerClass();
        switch (currentClass)
        {
            case PlayerClass.Assault:
                pcl = m_AssaultClass;
                break;
            case PlayerClass.Recon:
                pcl = m_ReconClass;
                break;
            case PlayerClass.Engineer:
                pcl = m_EngineerClass;
                break;
            case PlayerClass.Support:
                pcl = m_SupportClass;
                break;
        }

        if (pcl == null)
        {
            Debug.LogError($"Player Class Loadout has not been assigned for the class {currentClass.ToString()}");
            return;
        }

        PlayerEquip[0] = GetGunOnListById(pcl.Primary);
        PlayerEquip[1] = GetGunOnListById(pcl.Secondary);
        PlayerEquip[2] = GetGunOnListById(pcl.Letal);
        PlayerEquip[3] = GetGunOnListById(pcl.Perks);
#endif
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            if (PlayerEquip[i] == null) continue;
            PlayerEquip[i].Initialized();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onPickUpGun += this.PickUpGun;
        bl_EventHandler.onMatchStart += OnMatchStart;
        bl_EventHandler.onGameSettingsChange += OnGameSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onPickUpGun -= this.PickUpGun;
        bl_EventHandler.onMatchStart -= OnMatchStart;
        bl_EventHandler.onGameSettingsChange -= OnGameSettingsChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnUpdate()
    {
        if (!bl_RoomMenu.Instance.isCursorLocked)
            return;

        InputControl();
        CurrentGun = PlayerEquip[currentWeaponIndex];
    }

    /// <summary>
    /// 
    /// </summary>
    void InputControl()
    {
        if (!CanSwich || bl_GameData.Instance.isChating)
            return;
#if GR
        if (isGunRace) return;
#endif

#if !INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeCurrentWeaponTo(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeCurrentWeaponTo(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeCurrentWeaponTo(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ChangeCurrentWeaponTo(3);
        }

        //fast fire knife
        if (Input.GetKeyDown(KeyCode.V) && PlayerEquip[3] != null && PlayerEquip[3].m_AllowQuickFire && currentWeaponIndex != 3 && !isFastFire)
        {
            DoFastKnifeShot();
        }

        //fast throw grenade
        if (Input.GetKeyDown(KeyCode.G) && PlayerEquip[2] != null && PlayerEquip[2].AllowQuickFire() && currentWeaponIndex != 2 && !isFastFire)
        {
            DoSingleGrenadeThrow();
        }

#else
        InputManagerControll();
#endif
        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return;
        //change gun with Scroll mouse
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            SwitchNext();
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            SwitchPrevious();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public int SwitchNext()
    {
        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return 0;
#if GR
        if (isGunRace) return 0;
#endif

        int next = (this.currentWeaponIndex + 1) % this.PlayerEquip.Count;
        if (PlayerEquip[next] == null) return 0;

        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public int SwitchPrevious()
    {
        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return 0;
#if GR
        if (isGunRace) return 0;
#endif

        int next = 0;
        if (this.currentWeaponIndex != 0)
        {
            next = (this.currentWeaponIndex - 1) % this.PlayerEquip.Count;
        }
        else
        {
            next = PlayerEquip.Count - 1;
        }
        if (PlayerEquip[next] == null) return 0;
        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoFastKnifeShot()
    {
        var equippedKnife = PlayerEquip[3];
        if (equippedKnife == null || equippedKnife.Info.Type != GunType.Knife) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = 3; // 3 = knife position in list
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedKnife.gameObject.SetActive(true);
        equippedKnife.QuickMelee(OnReturnWeapon);
        CanSwich = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void DoSingleGrenadeThrow()
    {
        var equippedGrenade = PlayerEquip[2];// 2 = GRENADE position in list
        if (equippedGrenade == null || equippedGrenade.Info.Type != GunType.Grenade) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = 2; 
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedGrenade.gameObject.SetActive(true);
        StartCoroutine(equippedGrenade.FastGrenadeFire(OnReturnWeapon));
        CanSwich = false;
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnReturnWeapon()
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = PreviousGun;
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(true);
        CanSwich = true;
        isFastFire = false;
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].GunID);
    }

    /// <summary>
    /// 
    /// </summary>
    void TakeWeapon(bl_Gun gun, bool forced = false)
    {
        if (!CanSwich && !forced) return;
        if (gun == null) return;
        gun.gameObject.SetActive(true);
        CanSwich = true;
    }

    /// <summary>
    /// Call to set none weapon in the local player and can't select any weapon
    /// </summary>
    public void BlockAllWeapons()
    {
        foreach (bl_Gun g in PlayerEquip) { g.gameObject.SetActive(false); }
        bl_UCrosshair.Instance.Show(false);
        bl_UCrosshair.Instance.Block = true;
        CanSwich = false;
        PlayerSync.SetWeaponBlocked(1);
    }

    /// <summary>
    /// Make local player can switch weapons again
    /// </summary>
    public void ReleaseWeapons(bool takeFirst)
    {
        CanSwich = true;
        bl_UCrosshair.Instance.Block = false;
        bl_UCrosshair.Instance.Show(true);
        if (takeFirst)
        {
            TakeWeapon(PlayerEquip[0]);
        }
        else
        {
            TakeWeapon(PlayerEquip[currentWeaponIndex]);
        }
        PlayerSync.SetWeaponBlocked(0);
    }

    /// <summary>
    /// Unequipped all the player weapons leaving him with anything to attack.
    /// </summary>
    public void UnEquipEverything()
    {
        foreach (bl_Gun g in PlayerEquip) { g.gameObject.SetActive(false); }
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            PlayerEquip[i] = null;
        }
        bl_UCrosshair.Instance.Show(false);
        bl_UCrosshair.Instance.Block = true;
        bl_EventHandler.ChangeWeaponEvent(-1);
        bl_UIReferences.Instance.PlayerUI.WeaponStatsUI.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetInfinityAmmoToAllEquippeds(bool infinity)
    {
        AllGuns.ForEach((x)=>
        {
            if (x != null) x.SetInifinityAmmo(infinity);
        });
    }

    /// <summary>
    /// Change the current weapon (if any) to the given one
    /// </summary>
    public void ChangeTo(int AllWeaponsIndex)
    {
        StartCoroutine(ChangeGun(currentWeaponIndex, AllGuns[AllWeaponsIndex].gameObject, currentWeaponIndex));
        PlayerEquip[currentWeaponIndex] = AllGuns[AllWeaponsIndex];
    }

    /// <summary>
    /// Change to a weapon without delay/animation
    /// </summary>
    public void ChangeToInstant(int AllWeaponsIndex)
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        AllGuns[AllWeaponsIndex].gameObject.SetActive(true);
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].GunID);
        PlayerEquip[currentWeaponIndex] = AllGuns[AllWeaponsIndex];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nextSlotID"></param>
    public void ChangeCurrentWeaponTo(int nextSlotID)
    {
        if (currentWeaponIndex == nextSlotID || PlayerEquip[nextSlotID] == null) return;
        if (!CanSwich || bl_GameData.Instance.isChating)
            return;

        var next = PlayerEquip[nextSlotID];
        if (next == null || !next.CanBeTaken()) return;//if the weapons can't be taken when is empty (of ammo)

        PreviousGun = currentWeaponIndex;
        StartCoroutine(ChangeGun(currentWeaponIndex, next.gameObject, nextSlotID));
        currentWeaponIndex = nextSlotID;
    }

    /// <summary>
    /// Coroutine to Change of Gun
    /// </summary>
    /// <returns></returns>
    public IEnumerator ChangeGun(int IDfrom, GameObject t_next, int newID)
    {
        CanSwich = false;
        bl_UIReferences.Instance.PlayerUI.LoadoutUI.ChangeWeapon(newID);
        float hideTime = PlayerEquip[IDfrom].DisableWeapon();
        //instantly disable the current weapon and active the next weapon
        if (changeWeaponStyle == ChangeWeaponStyle.CounterStrike)
        {
            PlayerEquip[IDfrom].gameObject.SetActive(false);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideAndDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //wait a fixed delay before active the next weapon
            yield return new WaitForSeconds(SwichTime);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideCompletelyAndThenDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //wait until the current weapon hide animation complete before active the next weapon
            yield return new WaitForSeconds(hideTime);
        }
        foreach (bl_Gun guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(PlayerEquip[newID], true);
        bl_EventHandler.ChangeWeaponEvent(PlayerEquip[newID].GunID);
    }

    /// <summary>
    /// This function is called when the local player pick up a weapon in the map
    /// </summary>
    public void PickUpGun(GunPickUpData e)
    {
        if (pickupManager == null)
        {
            Debug.LogWarning("Need a 'Pick Up Manager' in the scene!");
            return;
        }
        //find the pick up weapon in the FP weapon list of this player prefab
        bl_Gun pickedLocalGun = AllGuns.Find(x => x.GunID == e.ID);
        if(pickedLocalGun == null)
        {
            Debug.LogWarning($"The weapon {e.ID} is not listed in this player prefab.");
            return;
        }
        //If not already equipped
        if (!PlayerEquip.Exists(x => x != null && x.GunID == e.ID))
        {
            bl_GunInfo gunInfo = bl_GameData.Instance.GetWeapon(e.ID);
            //first we need to make sure that there's not an empty slot in the loadout
            for (int i = 0; i < PlayerEquip.Count; i++)
            {
                //if there's an empty slot
                if(PlayerEquip[i] == null)
                {
                    //check if the this weapon can by equipped in this slot
                    if (bl_GameData.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, i))
                    {
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode.IsEnumFlagPresent(AutoChangeOnPickup.OnlyOnEmptySlots | AutoChangeOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(null, pickedLocalGun, null, i));
                            currentWeaponIndex = i;
                        }
                        return;
                    }
                }
            }

            //if get there means that there's not empty slots so we have to replace the current weapon
            //so first let's check if the pick up weapon can replace the weapon in the current slot.

            bool isEmpty = PlayerEquip[currentWeaponIndex] == null;
            int actualID = isEmpty ? -1 : PlayerEquip[currentWeaponIndex].GunID;

            //Get the required data from the weapon that we are going to replace
            int[] info = new int[2];
            int clips = isEmpty ? 3 : PlayerEquip[currentWeaponIndex].numberOfClips;
            info[0] = clips;
            info[1] = isEmpty ? 30 : PlayerEquip[currentWeaponIndex].bulletsLeft;

            bl_Gun oldGun;
            //if the pick up weapon can't replace the current slot
            if (!bl_GameData.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, currentWeaponIndex))
            {
                //let's find out the a compatible slot for it and replace the weapon there
                for (int i = 0; i < 4; i++)
                {
                    //when find the slot for this pick up weapon
                    if (bl_GameData.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, i))
                    {
                        oldGun = PlayerEquip[i];
                        //change the weapon on that slot with the pick up one
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode == (AutoChangeOnPickup.OnlyOnReplacements | AutoChangeOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, i));
                            currentWeaponIndex = i;
                        }
                        else
                        {
                            pickupManager.ThrowGun(oldGun.GunID, TrowPoint.position, info, false);
                        }
                        return;
                    }
                }
            }

            oldGun = PlayerEquip[currentWeaponIndex];
            //Replace the current equipped weapon
            SetUpPickUpWeapon(e, pickedLocalGun, currentWeaponIndex);
            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, currentWeaponIndex));
        }
        else//if the weapon is already equipped
        {
            foreach (bl_Gun g in PlayerEquip)
            {
                if (g != null && g.GunID == e.ID)
                {
                    g.OnPickUpAmmo(e.Bullets * e.Clips, 1, e.ID);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void SetUpPickUpWeapon(GunPickUpData data, bl_Gun gun, int slotID)
    {
        PlayerEquip[slotID] = gun;
        gun.Setup(true);

        if (gun.Info.Type == GunType.Grenade)
        {
            if (gun.bulletsLeft <= 0) gun.bulletsLeft = 1;
            else
                gun.numberOfClips++;
        }
        else
        {
            gun.bulletsLeft = data.Bullets;
            if (bl_GameData.Instance.AmmoType == AmmunitionType.Bullets)
                gun.numberOfClips = data.Bullets * data.Clips;
            else
                gun.numberOfClips = data.Clips;
        }

        bl_UIReferences.Instance.PlayerUI.LoadoutUI.ReplaceSlot(slotID, gun);
    }

    /// <summary>
    /// 
    /// </summary>
    public IEnumerator SwitchToPickUpGun(bl_Gun currentGun, bl_Gun nextGun, int[] info, int newSlotID)
    {
        HeadAnimator?.Play("TakeGun", 0, 0);
        currentGun?.DisableWeapon();
        if(currentWeaponIndex != newSlotID)
        bl_UIReferences.Instance.PlayerUI.LoadoutUI.ChangeWeapon(newSlotID);
        yield return new WaitForSeconds(PickUpTime);
        AllGuns.ForEach(x => { if (x != null) { x.gameObject.SetActive(false); } });
        TakeWeapon(nextGun, true);
        if (currentGun != null)
        {
            pickupManager.ThrowGun(currentGun.GunID, TrowPoint.position, info, false);
        }
        bl_EventHandler.ChangeWeaponEvent(nextGun.GunID);
        bl_UIReferences.Instance.PlayerUI.WeaponStatsUI.SetActive(true);
        CanSwich = true;
    }

    public void ThrwoCurrent(bool AutoDestroy) => ThrwoCurrent(AutoDestroy, TrowPoint.position);

    /// <summary>
    /// Throw the current gun
    /// </summary>
    public void ThrwoCurrent(bool AutoDestroy, Vector3 throwPosition)
    {
        if (PlayerEquip[currentWeaponIndex] == null)
            return;

        int actualID = PlayerEquip[currentWeaponIndex].GunID;
        int[] info = new int[2];
        int clips = (bl_GameData.Instance.AmmoType == AmmunitionType.Bullets) ? PlayerEquip[currentWeaponIndex].numberOfClips / PlayerEquip[currentWeaponIndex].bulletsPerClip : PlayerEquip[currentWeaponIndex].numberOfClips;
        info[0] = clips;
        info[1] = PlayerEquip[currentWeaponIndex].bulletsLeft;
        pickupManager.ThrowGun(actualID, throwPosition, info, AutoDestroy);
    }

    /// <summary>
    /// 
    /// </summary>
    public bl_Gun GetGunOnListById(int id)
    {
        bl_Gun gun = null;
        if (AllGuns.Exists(x => x != null && x.GunID == id))
        {
            gun = AllGuns.Find(x => x.GunID == id);
        }
        else
        {
            Debug.LogError("The FPWeapon: " + id + " has not been added on this player prefab.");
        }
        return gun;
    }

    /// <summary>
    /// Called when the game settings changed in runtime
    /// </summary>
    public void OnGameSettingsChanged()
    {
        if (bl_MFPS.Settings == null) return;

        float fov = (float)bl_MFPS.Settings.GetSettingOf("Weapon FOV");
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            item.SetDefaultWeaponCameraFOV(fov);
        }
    }

#if INPUT_MANAGER
    void InputManagerControll()
    {
        if (!CanSwich) return;

        if (bl_Input.isGamePad)
        {
            if (bl_Input.isButtonDown("NextWeapon"))
            {
                SwitchNext();
            }

            if (bl_Input.isButtonDown("PreviousWeapon"))
            {
                SwitchPrevious();
            }
        }
        else
        {
            if (bl_Input.isButtonDown("Weapon1"))
            {
               ChangeCurrentWeaponTo(0);
            }
            if (bl_Input.isButtonDown("Weapon2"))
            {
                ChangeCurrentWeaponTo(1);
            }
            if (bl_Input.isButtonDown("Weapon3"))
            {
               ChangeCurrentWeaponTo(2);
            }
            if (bl_Input.isButtonDown("Weapon4"))
            {
               ChangeCurrentWeaponTo(3);
            }
        }

        //fast fire knife
        if (bl_Input.isButtonDown("FastKnife") && CanSwich && currentWeaponIndex != 3 && !isFastFire)
        {
            DoFastKnifeShot();
        }
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void HeadAnimation(int state, float speed)
    {
        if (HeadAnimator == null)
            return;

        switch (state)
        {
            case 0:
                HeadAnimator.SetInteger("Reload", 0);
                break;
            case 1:
                HeadAnimator.SetInteger("Reload", 1);
                break;
            case 2:
                HeadAnimator.SetInteger("Reload", 2);
                break;
            case 3:
                HeadAnimator.CrossFade("Insert", 0.2f, 0, 0);
                break;
        }
    }

    public void PlaySound(int id)
    {
        if (ASource == null) return;
        if (id == 0)
        {
            if (SwitchFireAudioClip == null) return;
            ASource.clip = SwitchFireAudioClip;
            ASource.Play();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_Gun GetCurrentWeapon()
    {
        if (CurrentGun == null)
        {
            return PlayerEquip[currentWeaponIndex];
        }
        else
        {
            return CurrentGun;
        }
    }

    /// <summary>
    /// The GunID of the current equipped weapon
    /// -1 means that the player doesn't have a weapon equipped yet
    /// </summary>
    public int GetCurrentGunID
    {
        get
        {
            if (GetCurrentWeapon() == null) { return -1; }
            return GetCurrentWeapon().GunID;
        }
    }

    void OnMatchStart() { isGameStarted = true; }

    private bl_PlayerNetwork _Sync;
    public bl_PlayerNetwork PlayerSync
    {
        get
        {
            if (_Sync == null) { _Sync = bl_MFPS.LocalPlayerReferences.playerNetwork; }
            return _Sync;
        }
    }

    [System.Serializable]
    public enum ChangeWeaponStyle
    {
        HideAndDraw,
        CounterStrike,
        HideCompletelyAndThenDraw,
    }

    [System.Serializable, System.Flags]
    public enum AutoChangeOnPickup
    {
        Always = 0,
        OnlyOnEmptySlots,
        OnlyOnReplacements,
        Never,
    }
}