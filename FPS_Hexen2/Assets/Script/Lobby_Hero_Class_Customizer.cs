using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

namespace MFPS.ClassCustomization
{
    public class Lobby_Hero_Class_Customizer : MonoBehaviour
    {
        public PlayerClass m_Class { get; set; } = PlayerClass.Assault;
        [Header("Info")]
        public string SceneToReturn = "";
        public bool CloseListOnChangeWeapon = true;

        [Header("Weapons Info")]
        public ClassWeapons assaultWeapons;
        public ClassWeapons engineerWeapons;
        public ClassWeapons supportWeapons;
        public ClassWeapons reconWeapons;
        public ClassWeapons dragosWeapons;

        [Header("Slots Rules")]
        public ClassAllowedWeaponsType PrimaryAllowedWeapons;
        public ClassAllowedWeaponsType SecondaryAllowedWeapons;
        public ClassAllowedWeaponsType KnifeAllowedWeapons;
        public ClassAllowedWeaponsType GrenadesAllowedWeapons;

        private bl_ClassManager ClassManager;
        private int CurrentSlot = 0;
        private ClassPanel_UI UI;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            //Fix issue
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            UI = FindObjectOfType<ClassPanel_UI>();
            ClassManager = bl_ClassManager.Instance;
            ClassManager.Init();
        }


        /// <summary>
        /// 
        /// </summary>
        void Start()
        {
            TakeCurrentClass(bl_ClassManager.Instance.m_Class);
            SelectClassButton();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void ChangeSlotClass(int id, int slot, int listid)
        {
            switch (CurrentSlot)
            {
                case 0:
                    switch (bl_ClassManager.Instance.m_Class)
                    {
                        case PlayerClass.Assault:
                            ClassManager.AssaultClass.Primary = id;
                            break;
                        case PlayerClass.Engineer:
                            ClassManager.EngineerClass.Primary = id;
                            break;
                        case PlayerClass.Support:
                            ClassManager.SupportClass.Primary = id;
                            break;
                        case PlayerClass.Recon:
                            ClassManager.ReconClass.Primary = id;
                            break;
                        case PlayerClass.Dragos:
                            ClassManager.DragosClass.Primary = id;
                            break;
                    }

                    break;
                case 1:
                    switch (bl_ClassManager.Instance.m_Class)
                    {
                        case PlayerClass.Assault:
                            ClassManager.AssaultClass.Secondary = id;
                            break;
                        case PlayerClass.Engineer:
                            ClassManager.EngineerClass.Secondary = id;
                            break;
                        case PlayerClass.Support:
                            ClassManager.SupportClass.Secondary = id;
                            break;
                        case PlayerClass.Recon:
                            ClassManager.ReconClass.Secondary = id;
                            break;
                        case PlayerClass.Dragos:
                            ClassManager.DragosClass.Secondary = id;
                            break;
                    }
                    break;
                case 2:
                    switch (bl_ClassManager.Instance.m_Class)
                    {
                        case PlayerClass.Assault:
                            ClassManager.AssaultClass.Perks = id;
                            break;
                        case PlayerClass.Engineer:
                            ClassManager.EngineerClass.Perks = id;
                            break;
                        case PlayerClass.Support:
                            ClassManager.SupportClass.Perks = id;
                            break;
                        case PlayerClass.Recon:
                            ClassManager.ReconClass.Perks = id;
                            break;
                        case PlayerClass.Dragos:
                            ClassManager.DragosClass.Perks = id;
                            break;
                    }
                    break;
                case 3:
                    switch (bl_ClassManager.Instance.m_Class)
                    {
                        case PlayerClass.Assault:
                            ClassManager.AssaultClass.Letal = id;
                            break;
                        case PlayerClass.Engineer:
                            ClassManager.EngineerClass.Letal = id;
                            break;
                        case PlayerClass.Support:
                            ClassManager.SupportClass.Letal = id;
                            break;
                        case PlayerClass.Recon:
                            ClassManager.ReconClass.Letal = id;
                            break;
                        case PlayerClass.Dragos:
                            ClassManager.DragosClass.Letal = id;
                            break;
                    }
                    break;

            }
            UpdateClassUI(listid, slot);
            if (CloseListOnChangeWeapon)
            {
                CleanList();
            }
            //UI.SaveButton.SetActive(true);
        }

        void UpdateClassUI(int id, int slot)
        {
            switch (bl_ClassManager.Instance.m_Class)
            {
                case PlayerClass.Assault:
                    switch (slot)
                    {
                        case 0:
                            UI.Active_HUD.Icon.sprite = assaultWeapons.AllWeapons[id].Info.GunIcon;
                            UI.Active_HUD.WeaponNameText.text = assaultWeapons.AllWeapons[id].Info.Name.ToUpper();
                            UI.Active_HUD.AccuracySlider.value = assaultWeapons.AllWeapons[id].Info.Accuracy;
                            UI.Active_HUD.DamageSlider.value = assaultWeapons.AllWeapons[id].Info.Damage;
                            UI.Active_HUD.RateSlider.value = assaultWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 1:
                            //UI.SecundaryHUD.Icon.sprite = assaultWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.SecundaryHUD.WeaponNameText.text = assaultWeapons.AllWeapons[id].Info.Name.ToUpper();
                            // UI.SecundaryHUD.AccuracySlider.value = assaultWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.SecundaryHUD.DamageSlider.value = assaultWeapons.AllWeapons[id].Info.Damage;
                            //UI.SecundaryHUD.RateSlider.value = assaultWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 2:
                            //UI.KnifeHUD.Icon.sprite = assaultWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.WeaponNameText.text = assaultWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.KnifeHUD.AccuracySlider.value = assaultWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.KnifeHUD.DamageSlider.value = assaultWeapons.AllWeapons[id].Info.Damage;
                            //UI.KnifeHUD.RateSlider.value = assaultWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 3:
                            //UI.GrenadeHUD.Icon.sprite = assaultWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.GrenadeHUD.WeaponNameText.text = assaultWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.GrenadeHUD.AccuracySlider.value = assaultWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.GrenadeHUD.DamageSlider.value = assaultWeapons.AllWeapons[id].Info.Damage;
                            //UI.GrenadeHUD.RateSlider.value = assaultWeapons.AllWeapons[id].Info.FireRate;
                            break;
                    }
                    break;
                //------------------------------------------------------------------
                case PlayerClass.Engineer:
                    switch (slot)
                    {
                        case 0:
                            //UI.PrimaryHUD.Icon.sprite = engineerWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.PrimaryHUD.WeaponNameText.text = engineerWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.PrimaryHUD.AccuracySlider.value = engineerWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.PrimaryHUD.DamageSlider.value = engineerWeapons.AllWeapons[id].Info.Damage;
                            //UI.PrimaryHUD.RateSlider.value = engineerWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 1:
                            //UI.SecundaryHUD.Icon.sprite = engineerWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.SecundaryHUD.WeaponNameText.text = engineerWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.SecundaryHUD.AccuracySlider.value = engineerWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.SecundaryHUD.DamageSlider.value = engineerWeapons.AllWeapons[id].Info.Damage;
                            //UI.SecundaryHUD.RateSlider.value = engineerWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 2:
                            //UI.KnifeHUD.Icon.sprite = engineerWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.WeaponNameText.text = engineerWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.KnifeHUD.AccuracySlider.value = engineerWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.KnifeHUD.DamageSlider.value = engineerWeapons.AllWeapons[id].Info.Damage;
                            //UI.KnifeHUD.RateSlider.value = engineerWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 3:
                            //UI.GrenadeHUD.Icon.sprite = engineerWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.GrenadeHUD.WeaponNameText.text = engineerWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.GrenadeHUD.AccuracySlider.value = engineerWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.GrenadeHUD.DamageSlider.value = engineerWeapons.AllWeapons[id].Info.Damage;
                            //UI.GrenadeHUD.RateSlider.value = engineerWeapons.AllWeapons[id].Info.FireRate;
                            break;
                    }
                    break;
                case PlayerClass.Support:
                    switch (slot)
                    {
                        case 0:
                            //UI.PrimaryHUD.Icon.sprite = supportWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.PrimaryHUD.WeaponNameText.text = supportWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.PrimaryHUD.AccuracySlider.value = supportWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.PrimaryHUD.DamageSlider.value = supportWeapons.AllWeapons[id].Info.Damage;
                            // UI.PrimaryHUD.RateSlider.value = supportWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 1:
                            //UI.SecundaryHUD.Icon.sprite = supportWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.SecundaryHUD.WeaponNameText.text = supportWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.SecundaryHUD.AccuracySlider.value = supportWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.SecundaryHUD.DamageSlider.value = supportWeapons.AllWeapons[id].Info.Damage;
                            //UI.SecundaryHUD.RateSlider.value = supportWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 2:
                            //UI.KnifeHUD.Icon.sprite = supportWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.WeaponNameText.text = supportWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.KnifeHUD.AccuracySlider.value = supportWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.KnifeHUD.DamageSlider.value = supportWeapons.AllWeapons[id].Info.Damage;
                            //UI.KnifeHUD.RateSlider.value = supportWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 3:
                            //UI.GrenadeHUD.Icon.sprite = supportWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.GrenadeHUD.WeaponNameText.text = supportWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.GrenadeHUD.AccuracySlider.value = supportWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.GrenadeHUD.DamageSlider.value = supportWeapons.AllWeapons[id].Info.Damage;
                            //UI.GrenadeHUD.RateSlider.value = supportWeapons.AllWeapons[id].Info.FireRate;
                            break;
                    }
                    break;
                case PlayerClass.Recon:
                    switch (slot)
                    {
                        case 0:
                            //UI.PrimaryHUD.Icon.sprite = reconWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.PrimaryHUD.WeaponNameText.text = reconWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.PrimaryHUD.AccuracySlider.value = reconWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.PrimaryHUD.DamageSlider.value = reconWeapons.AllWeapons[id].Info.Damage;
                            //UI.PrimaryHUD.RateSlider.value = reconWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 1:
                            //UI.SecundaryHUD.Icon.sprite = reconWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.SecundaryHUD.WeaponNameText.text = reconWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.SecundaryHUD.AccuracySlider.value = reconWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.SecundaryHUD.DamageSlider.value = reconWeapons.AllWeapons[id].Info.Damage;
                            //UI.SecundaryHUD.RateSlider.value = reconWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 2:
                            //UI.KnifeHUD.Icon.sprite = reconWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.Icon.sprite = reconWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.WeaponNameText.text = reconWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.KnifeHUD.AccuracySlider.value = reconWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.KnifeHUD.DamageSlider.value = reconWeapons.AllWeapons[id].Info.Damage;
                            //UI.KnifeHUD.RateSlider.value = reconWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 3:
                            //UI.GrenadeHUD.Icon.sprite = reconWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.GrenadeHUD.WeaponNameText.text = reconWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.GrenadeHUD.AccuracySlider.value = reconWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.GrenadeHUD.DamageSlider.value = reconWeapons.AllWeapons[id].Info.Damage;
                            //UI.GrenadeHUD.RateSlider.value = reconWeapons.AllWeapons[id].Info.FireRate;
                            break;
                    }
                    break;
                case PlayerClass.Dragos:
                    switch (slot)
                    {
                        case 0:
                            //UI.PrimaryHUD.Icon.sprite = dragosWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.PrimaryHUD.WeaponNameText.text = dragosWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.PrimaryHUD.AccuracySlider.value = dragosWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.PrimaryHUD.DamageSlider.value = dragosWeapons.AllWeapons[id].Info.Damage;
                            //UI.PrimaryHUD.RateSlider.value = dragosWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 1:
                            //UI.SecundaryHUD.Icon.sprite = dragosWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.SecundaryHUD.WeaponNameText.text = dragosWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.SecundaryHUD.AccuracySlider.value = dragosWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.SecundaryHUD.DamageSlider.value = dragosWeapons.AllWeapons[id].Info.Damage;
                            //UI.SecundaryHUD.RateSlider.value = dragosWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 2:
                            //UI.KnifeHUD.Icon.sprite = dragosWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.Icon.sprite = dragosWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.KnifeHUD.WeaponNameText.text = dragosWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.KnifeHUD.AccuracySlider.value = dragosWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.KnifeHUD.DamageSlider.value = dragosWeapons.AllWeapons[id].Info.Damage;
                            //UI.KnifeHUD.RateSlider.value = dragosWeapons.AllWeapons[id].Info.FireRate;
                            break;
                        case 3:
                            //UI.GrenadeHUD.Icon.sprite = dragosWeapons.AllWeapons[id].Info.GunIcon;
                            //UI.GrenadeHUD.WeaponNameText.text = dragosWeapons.AllWeapons[id].Info.Name.ToUpper();
                            //UI.GrenadeHUD.AccuracySlider.value = dragosWeapons.AllWeapons[id].Info.Accuracy;
                            //UI.GrenadeHUD.DamageSlider.value = dragosWeapons.AllWeapons[id].Info.Damage;
                            //UI.GrenadeHUD.RateSlider.value = dragosWeapons.AllWeapons[id].Info.FireRate;
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SaveClass()
        {
            //UI.loadingUI.SetActive(true);
            //bl_ClassManager.Instance.SaveClass(() => { this.InvokeAfter(1, () => { UI.loadingUI.SetActive(false); }); });
            //UI.SaveButton.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReturnToScene()
        {
            bl_UtilityHelper.LoadLevel(SceneToReturn);
        }

        /// <summary>
        /// 
        /// </summary>
        void CleanList()
        {
           // foreach (Transform t in UI.PanelWeaponList.GetComponentsInChildren<Transform>())
           // {
           //     if (t.GetComponent<bl_ClassInfoUI>() != null)
           //     {
           //         Destroy(t.gameObject);
            //    }
            //}
        }

        /// <summary>
        /// 
        /// </summary>
        private bool isAllowedWeapon(bl_GunInfo info, int slot)
        {
            ClassAllowedWeaponsType rules = PrimaryAllowedWeapons;
            if (slot == 1) { rules = SecondaryAllowedWeapons; }
            else if (slot == 2) { rules = KnifeAllowedWeapons; }
            else if (slot == 3) { rules = GrenadesAllowedWeapons; }

            if ((rules.AllowMachineGuns && (info.Type == GunType.Machinegun || info.Type == GunType.Burst)) || (rules.AllowPistols && info.Type == GunType.Pistol) || (rules.AllowShotguns && info.Type == GunType.Shotgun)
                || (rules.AllowKnifes && info.Type == GunType.Knife) || (rules.AllowGrenades && (info.Type == GunType.Grenade || info.Type == GunType.Grenade)) || (rules.AllowSnipers && info.Type == GunType.Sniper))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ShowList(int slot)
        {
            CurrentSlot = slot;
            CleanList();

            switch (bl_ClassManager.Instance.m_Class)
            {
                case PlayerClass.Assault:
                    for (int i = 0; i < assaultWeapons.AllWeapons.Count; i++)
                    {
                        if (isAllowedWeapon(assaultWeapons.AllWeapons[i].Info, slot))
                        {
                            if (!assaultWeapons.AllWeapons[i].isEnabled) continue;
                         //   GameObject b = Instantiate(UI.GunSelectPrefabs) as GameObject;
                         //   bl_ClassInfoUI iui = b.GetComponent<bl_ClassInfoUI>();
                         //   iui.GetInfo(assaultWeapons.AllWeapons[i], slot, i);
                        //    b.transform.SetParent(UI.PanelWeaponList, false);
                        }
                    }
                    break;
                case PlayerClass.Engineer://----------------------------------------------------------------------------------------------------
                    for (int i = 0; i < engineerWeapons.AllWeapons.Count; i++)
                    {
                        if (isAllowedWeapon(engineerWeapons.AllWeapons[i].Info, slot))
                        {
                            if (!engineerWeapons.AllWeapons[i].isEnabled) continue;
                         //   GameObject b = Instantiate(UI.GunSelectPrefabs) as GameObject;
                          //  bl_ClassInfoUI iui = b.GetComponent<bl_ClassInfoUI>();
                         //   iui.GetInfo(engineerWeapons.AllWeapons[i], slot, i);
                         //   b.transform.SetParent(UI.PanelWeaponList, false);
                        }
                    }
                    break;
                case PlayerClass.Support://-----------------------------------------------------------------------------------------------------
                    for (int i = 0; i < supportWeapons.AllWeapons.Count; i++)
                    {
                        if (isAllowedWeapon(supportWeapons.AllWeapons[i].Info, slot))
                        {
                            if (!supportWeapons.AllWeapons[i].isEnabled) continue;
                          //  GameObject b = Instantiate(UI.GunSelectPrefabs) as GameObject;
                          //  bl_ClassInfoUI iui = b.GetComponent<bl_ClassInfoUI>();
                          //  iui.GetInfo(supportWeapons.AllWeapons[i], slot, i);
                          //  b.transform.SetParent(UI.PanelWeaponList, false);
                        }
                    }
                    break;
                case PlayerClass.Recon://-----------------------------------------------------------------------------
                    for (int i = 0; i < reconWeapons.AllWeapons.Count; i++)
                    {
                        if (isAllowedWeapon(reconWeapons.AllWeapons[i].Info, slot))
                        {
                            if (!reconWeapons.AllWeapons[i].isEnabled) continue;
                         //   GameObject b = Instantiate(UI.GunSelectPrefabs) as GameObject;
                          //  bl_ClassInfoUI iui = b.GetComponent<bl_ClassInfoUI>();
                          //  iui.GetInfo(reconWeapons.AllWeapons[i], slot, i);
                         //   b.transform.SetParent(UI.PanelWeaponList, false);
                        }
                    }
                    break;
                case PlayerClass.Dragos://-----------------------------------------------------------------------------
                    for (int i = 0; i < reconWeapons.AllWeapons.Count; i++)
                    {
                        if (isAllowedWeapon(reconWeapons.AllWeapons[i].Info, slot))
                        {
                            if (!dragosWeapons.AllWeapons[i].isEnabled) continue;
                         //   GameObject b = Instantiate(UI.GunSelectPrefabs) as GameObject;
                         //   bl_ClassInfoUI iui = b.GetComponent<bl_ClassInfoUI>();
                         //   iui.GetInfo(dragosWeapons.AllWeapons[i], slot, i);
                         //   b.transform.SetParent(UI.PanelWeaponList, false);
                        }
                    }
                    break;
            }
        }

        public void ChangeKit(int kit)
        {
            bl_ClassManager.Instance.ClassKit = kit;
          //  UI.SaveButton.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ChangeClass(PlayerClass newclass)
        {
            if (m_Class == newclass && bl_ClassManager.Instance.m_Class == newclass)
                return;

            m_Class = newclass;
            bl_ClassManager.Instance.m_Class = newclass;
            newclass.SavePlayerClass();
          //  UI.ClassText.text = (newclass.ToString() + " Class").ToUpper();
            ResetClassHUD();
            int i = 0;
            switch (newclass)
            {
                case PlayerClass.Assault:
                    i = 0;
                    UpdateClassUI(GetListId(PlayerClass.Assault, ClassManager.AssaultClass.Primary), 0);
                    UpdateClassUI(GetListId(PlayerClass.Assault, ClassManager.AssaultClass.Secondary), 1);
                    UpdateClassUI(GetListId(PlayerClass.Assault, ClassManager.AssaultClass.Perks), 2);
                    UpdateClassUI(GetListId(PlayerClass.Assault, ClassManager.AssaultClass.Letal), 3);
                    break;
                case PlayerClass.Engineer:
                    i = 1;
                    UpdateClassUI(GetListId(PlayerClass.Engineer, ClassManager.EngineerClass.Primary), 0);
                    UpdateClassUI(GetListId(PlayerClass.Engineer, ClassManager.EngineerClass.Secondary), 1);
                    UpdateClassUI(GetListId(PlayerClass.Engineer, ClassManager.EngineerClass.Perks), 2);
                    UpdateClassUI(GetListId(PlayerClass.Engineer, ClassManager.EngineerClass.Letal), 3);
                    break;
                case PlayerClass.Support:
                    i = 2;
                    UpdateClassUI(GetListId(PlayerClass.Support, ClassManager.SupportClass.Primary), 0);
                    UpdateClassUI(GetListId(PlayerClass.Support, ClassManager.SupportClass.Secondary), 1);
                    UpdateClassUI(GetListId(PlayerClass.Support, ClassManager.SupportClass.Perks), 2);
                    UpdateClassUI(GetListId(PlayerClass.Support, ClassManager.SupportClass.Letal), 3);
                    break;
                case PlayerClass.Recon:
                    i = 3;
                    UpdateClassUI(GetListId(PlayerClass.Recon, ClassManager.ReconClass.Primary), 0);
                    UpdateClassUI(GetListId(PlayerClass.Recon, ClassManager.ReconClass.Secondary), 1);
                    UpdateClassUI(GetListId(PlayerClass.Recon, ClassManager.ReconClass.Perks), 2);
                    UpdateClassUI(GetListId(PlayerClass.Recon, ClassManager.ReconClass.Letal), 3);
                    break;
                case PlayerClass.Dragos:
                    i = 4;
                    UpdateClassUI(GetListId(PlayerClass.Dragos, ClassManager.DragosClass.Primary), 0);
                    UpdateClassUI(GetListId(PlayerClass.Dragos, ClassManager.DragosClass.Secondary), 1);
                    UpdateClassUI(GetListId(PlayerClass.Dragos, ClassManager.DragosClass.Perks), 2);
                    UpdateClassUI(GetListId(PlayerClass.Dragos, ClassManager.DragosClass.Letal), 3);
                    break;
            }
            PlayerPrefs.SetInt(ClassKey.ClassType, i);
            SelectClassButton();
        }

        Vector2 defaulClassSize = Vector2.zero;
        void SelectClassButton()
        {
         //   if (defaulClassSize == Vector2.zero) { defaulClassSize = UI.ClassButtons[0].sizeDelta; }
          //  foreach (var r in UI.ClassButtons)
          //  {
          //      r.sizeDelta = defaulClassSize;
          //  }
          //  Vector2 v = defaulClassSize;
          //  v.y += 10;
          //  UI.ClassButtons[(int)bl_ClassManager.Instance.m_Class].sizeDelta = v;
        }

        /// <summary>
        /// 
        /// </summary>
        private int GetListId(PlayerClass clas, int id)
        {
            switch (clas)
            {
                case PlayerClass.Assault:
                    for (int i = 0; i < assaultWeapons.AllWeapons.Count; i++)
                    {
                        if (assaultWeapons.AllWeapons[i].GunID == id)
                        {
                            return i;
                        }
                    }
                    break;
                case PlayerClass.Engineer:
                    for (int i = 0; i < engineerWeapons.AllWeapons.Count; i++)
                    {
                        if (engineerWeapons.AllWeapons[i].GunID == id)
                        {
                            return i;
                        }
                    }
                    break;
                case PlayerClass.Support:
                    for (int i = 0; i < supportWeapons.AllWeapons.Count; i++)
                    {
                        if (supportWeapons.AllWeapons[i].GunID == id)
                        {
                            return i;
                        }
                    }
                    break;
                case PlayerClass.Recon:
                    for (int i = 0; i < reconWeapons.AllWeapons.Count; i++)
                    {
                        if (reconWeapons.AllWeapons[i].GunID == id)
                        {
                            return i;
                        }
                    }
                    break;
                case PlayerClass.Dragos:
                    for (int i = 0; i < dragosWeapons.AllWeapons.Count; i++)
                    {
                        if (dragosWeapons.AllWeapons[i].GunID == id)
                        {
                            return i;
                        }
                    }
                    break;
            }

            return 0;
        }

        void ResetClassHUD()
        {
        //    UI.PrimaryHUD.WeaponNameText.text = "None";
        //    UI.PrimaryHUD.Icon.sprite = null;
        //    UI.PrimaryHUD.DamageSlider.value = 0;
        //    UI.PrimaryHUD.AccuracySlider.value = 0;
        //    UI.PrimaryHUD.RateSlider.value = 0;
            //
        //    UI.SecundaryHUD.WeaponNameText.text = "None";
        //    UI.SecundaryHUD.Icon.sprite = null;
        //    UI.SecundaryHUD.DamageSlider.value = 0;
        //    UI.SecundaryHUD.AccuracySlider.value = 0;
        //    UI.SecundaryHUD.RateSlider.value = 0;
            //
        //    UI.KnifeHUD.WeaponNameText.text = "None";
        //    UI.KnifeHUD.Icon.sprite = null;
        //    UI.KnifeHUD.DamageSlider.value = 0;
        //    UI.KnifeHUD.AccuracySlider.value = 0;
        //    UI.KnifeHUD.RateSlider.value = 0;
            //
        //    UI.GrenadeHUD.WeaponNameText.text = "None";
        //    UI.GrenadeHUD.Icon.sprite = null;
        //    UI.GrenadeHUD.DamageSlider.value = 0;
        //    UI.GrenadeHUD.AccuracySlider.value = 0;
        //    UI.GrenadeHUD.RateSlider.value = 0;
        }

        /// <summary>
        /// Take the current class
        /// </summary>
        void TakeCurrentClass(PlayerClass mclass)
        {
            switch (mclass)
            {
                case PlayerClass.Assault:
                    foreach (WeaponItemData ci in assaultWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.AssaultClass.Primary)
                        {
                        //    UI.PrimaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                        //    UI.PrimaryHUD.Icon.sprite = ci.Info.GunIcon;
                        //    UI.PrimaryHUD.DamageSlider.value = ci.Info.Damage;
                        //    UI.PrimaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                        //    UI.PrimaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in assaultWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.AssaultClass.Secondary)
                        {
                        //    UI.SecundaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                        //    UI.SecundaryHUD.Icon.sprite = ci.Info.GunIcon;
                        //    UI.SecundaryHUD.DamageSlider.value = ci.Info.Damage;
                        //    UI.SecundaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                        //    UI.SecundaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in assaultWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.AssaultClass.Perks)
                        {
                        //    UI.KnifeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                        //    UI.KnifeHUD.Icon.sprite = ci.Info.GunIcon;
                        //    UI.KnifeHUD.DamageSlider.value = ci.Info.Damage;
                        //    UI.KnifeHUD.AccuracySlider.value = ci.Info.Accuracy;
                        //    UI.KnifeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in engineerWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.AssaultClass.Letal)
                        {
                        //    UI.GrenadeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                        //    UI.GrenadeHUD.Icon.sprite = ci.Info.GunIcon;
                        //    UI.GrenadeHUD.DamageSlider.value = ci.Info.Damage;
                        //    UI.GrenadeHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.GrenadeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    break;
                case PlayerClass.Engineer://---------------------------------------------------------------------------------
                    foreach (WeaponItemData ci in engineerWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.EngineerClass.Primary)
                        {
                         //   UI.PrimaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.PrimaryHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.PrimaryHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.PrimaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.PrimaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in engineerWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.EngineerClass.Secondary)
                        {
                          //  UI.SecundaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.SecundaryHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.SecundaryHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.SecundaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.SecundaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in engineerWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.EngineerClass.Perks)
                        {
                          //  UI.KnifeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.KnifeHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.KnifeHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.KnifeHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.KnifeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in engineerWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.EngineerClass.Letal)
                        {
                          //  UI.GrenadeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.GrenadeHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.GrenadeHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.GrenadeHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.GrenadeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    break;
                case PlayerClass.Recon://-------------------------------------------------------------------------------------
                    foreach (WeaponItemData ci in reconWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.ReconClass.Primary)
                        {
                          //  UI.PrimaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          ///  UI.PrimaryHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.PrimaryHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.PrimaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                           // UI.PrimaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in reconWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.ReconClass.Secondary)
                        {
                         //   UI.SecundaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.SecundaryHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.SecundaryHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.SecundaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.SecundaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in reconWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.ReconClass.Perks)
                        {
                         //   UI.KnifeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.KnifeHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.KnifeHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.KnifeHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.KnifeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in reconWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.ReconClass.Letal)
                        {
                          //  UI.GrenadeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.GrenadeHUD.Icon.sprite = ci.Info.GunIcon;
                          ///  UI.GrenadeHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.GrenadeHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.GrenadeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    break;
                case PlayerClass.Support://--------------------------------------------------------------------------------------
                    foreach (WeaponItemData ci in supportWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.SupportClass.Primary)
                        {
                          //  UI.PrimaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.PrimaryHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.PrimaryHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.PrimaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.PrimaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in supportWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.SupportClass.Secondary)
                        {
                          //  UI.SecundaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.SecundaryHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.SecundaryHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.SecundaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.SecundaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in supportWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.SupportClass.Perks)
                        {
                          //  UI.KnifeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                          //  UI.KnifeHUD.Icon.sprite = ci.Info.GunIcon;
                          //  UI.KnifeHUD.DamageSlider.value = ci.Info.Damage;
                          //  UI.KnifeHUD.AccuracySlider.value = ci.Info.Accuracy;
                          //  UI.KnifeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in supportWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.SupportClass.Letal)
                        {
                         //   UI.GrenadeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.GrenadeHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.GrenadeHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.GrenadeHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.GrenadeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    break;
                case PlayerClass.Dragos://--------------------------------------------------------------------------------------
                    foreach (WeaponItemData ci in dragosWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.DragosClass.Primary)
                        {
                         //   UI.PrimaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.PrimaryHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.PrimaryHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.PrimaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.PrimaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in dragosWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.DragosClass.Secondary)
                        {
                         //   UI.SecundaryHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.SecundaryHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.SecundaryHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.SecundaryHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.SecundaryHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in dragosWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.DragosClass.Perks)
                        {
                         //   UI.KnifeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.KnifeHUD.Icon.sprite = ci.Info.GunIcon;
                        //    UI.KnifeHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.KnifeHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.KnifeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    foreach (WeaponItemData ci in dragosWeapons.AllWeapons)
                    {
                        if (ci.GunID == ClassManager.DragosClass.Letal)
                        {
                         //   UI.GrenadeHUD.WeaponNameText.text = ci.Info.Name.ToUpper();
                         //   UI.GrenadeHUD.Icon.sprite = ci.Info.GunIcon;
                         //   UI.GrenadeHUD.DamageSlider.value = ci.Info.Damage;
                         //   UI.GrenadeHUD.AccuracySlider.value = ci.Info.Accuracy;
                         //   UI.GrenadeHUD.RateSlider.value = ci.Info.FireRate;
                            break;
                        }
                    }
                    break;
            }
        }

        private static Lobby_Hero_Class_Customizer _instance;
        public static Lobby_Hero_Class_Customizer Instance
        {
            get
            {
                if (_instance == null) { _instance = FindObjectOfType<Lobby_Hero_Class_Customizer>(); }
                return _instance;
            }
        }

#if UNITY_EDITOR

        public void RefreshLists()
        {
            if (!gameObject.activeInHierarchy) return;
            assaultWeapons.UpdateLobbyList(this);
            engineerWeapons.UpdateLobbyList(this);
            supportWeapons.UpdateLobbyList(this);
            reconWeapons.UpdateLobbyList(this);
            dragosWeapons.UpdateLobbyList(this);
        }

        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy) return;
            RefreshLists();
        }
#endif
    }
}